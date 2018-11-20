using System;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Cache;
using OptimaJet.Workflow.Core.CodeActions;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Core.Subprocess;

namespace OptimaJet.Workflow.Core.Builder
{
    /// <summary>
    /// Base workflow builder, which convert not parsed process scheme <see cref="SchemeDefinition{T}"/> to the object model of a scheme of a process <see cref="ProcessDefinition"/>
    /// </summary>
    /// <typeparam name="TSchemeMedium">Type of not parsed scheme <see cref="SchemeDefinition{T}"/></typeparam>
    public sealed class WorkflowBuilder<TSchemeMedium> : IWorkflowBuilder where TSchemeMedium : class
    {
        internal IWorkflowGenerator<TSchemeMedium> Generator;

        internal IWorkflowParser<TSchemeMedium> Parser;

        internal ISchemePersistenceProvider<TSchemeMedium> SchemePersistenceProvider;

        private bool _haveCache;

        private IParsedProcessCache _cache;

        internal WorkflowBuilder()
        {
            
        }

        /// <summary>
        /// Create WorkflowBuilder object
        /// </summary>
        /// <param name="generator">Not parsed scheme generator <see cref="IWorkflowGenerator{T}"/></param>
        /// <param name="parser">Parser from not parsed process scheme <see cref="SchemeDefinition{T}"/> to the object model of a scheme of a process <see cref="ProcessDefinition"/></param>
        /// <param name="schemePersistenceProvider">Scheme persistemce provider <see cref="ISchemePersistenceProvider{T}"/></param>
        public WorkflowBuilder (IWorkflowGenerator<TSchemeMedium> generator,
                               IWorkflowParser<TSchemeMedium> parser,
                               ISchemePersistenceProvider<TSchemeMedium> schemePersistenceProvider)
        {
            Generator = generator;
            Parser = parser;
            SchemePersistenceProvider = schemePersistenceProvider;
        }

        /// <summary>
        /// Returns process scheme by specific id, if scheme not exists creates it 
        /// </summary>
        /// <param name="schemeId">Id of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        public ProcessDefinition GetProcessScheme(Guid schemeId)
        {
            return GetProcessDefinition(SchemePersistenceProvider.GetProcessSchemeBySchemeId(schemeId));
        }

        /// <summary>
        /// Returns process scheme by specific name, if scheme not exists creates it
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        public ProcessDefinition GetProcessScheme(string schemeCode)
        {
            return GetProcessScheme(schemeCode, new Dictionary<string, object>());
        }

        /// <summary>
        /// Returns process scheme by specific name and parameters for creating the scheme of the process, if scheme not exists creates it
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        /// <returns>ProcessDefinition object</returns>
        public ProcessDefinition GetProcessScheme(string schemeCode, IDictionary<string, object> parameters)
        {
            var definingParameters = DefiningParametersSerializer.Serialize(parameters);
            try
            {
                return
                    GetProcessDefinition(SchemePersistenceProvider.GetProcessSchemeWithParameters(schemeCode, definingParameters,
                        null, true));
            }
            catch (SchemeNotFoundException)
            {
                return GetProcessDefinition(CreateNewScheme(schemeCode, parameters));
            }
        }

        /// <summary>
        /// Create new instance of the process.
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="processId">Process id</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        /// <returns>ProcessInstance object</returns>
        public ProcessInstance CreateNewProcess(Guid processId,
                                                string schemeCode,
                                                IDictionary<string, object> parameters)
        {
            var definingParameters = DefiningParametersSerializer.Serialize(parameters);
            SchemeDefinition<TSchemeMedium> schemeDefinition = null;
            try
            {
                schemeDefinition = SchemePersistenceProvider
                                    .GetProcessSchemeWithParameters(schemeCode,
                                                                    definingParameters,
                                                                    null,
                                                                    true);
            }
            catch (SchemeNotFoundException)
            {
                schemeDefinition = CreateNewScheme(schemeCode, parameters);
            }

            var processInstance = ProcessInstance.Create(schemeDefinition.Id,
                                          processId,
                                          GetProcessDefinition(schemeDefinition),
                                          schemeDefinition.IsObsolete, schemeDefinition.IsDeterminingParametersChanged);

            processInstance.RootProcessId = processInstance.ProcessId;

            return processInstance;
        }

        public ProcessInstance CreateNewSubprocess(Guid processId,
            ProcessInstance parentProcessInstance,
            TransitionDefinition startingTransition
            )
        {
            var parentProcessScheme = parentProcessInstance.ProcessScheme;

            //Scheme code for subprocess {RootSchemeCode}_{StartingTransition.Name}
            var schemeCode = GetSubprocessSchemeCode(startingTransition, parentProcessScheme);

            var rootSchemeId = parentProcessScheme.RootSchemeId ?? parentProcessScheme.Id;

            SchemeDefinition<TSchemeMedium> schemeDefinition = null;
            try
            {
                schemeDefinition = SchemePersistenceProvider.GetProcessSchemeWithParameters(schemeCode,
                    parentProcessScheme.DefiningParametersString, rootSchemeId, false);
            }
            catch (SchemeNotFoundException)
            {
                schemeDefinition = CreateSubprocessSchemeDefinition(schemeCode, rootSchemeId, startingTransition, parentProcessScheme);

                try
                {
                    SchemePersistenceProvider.SaveScheme(schemeDefinition);
                }
                catch (SchemeAlredyExistsException)
                {
                    schemeDefinition = SchemePersistenceProvider.GetProcessSchemeWithParameters(schemeCode,
                        parentProcessScheme.DefiningParametersString, rootSchemeId, false);
                }
            }

            var processInstance = ProcessInstance.Create(schemeDefinition.Id,
                                        processId,
                                        GetProcessDefinition(schemeDefinition),
                                        schemeDefinition.IsObsolete, schemeDefinition.IsDeterminingParametersChanged);

            //All not system parameters copy to subprocess
            foreach (var parameter in parentProcessInstance.ProcessParameters.Where(p => p.Purpose != ParameterPurpose.System))
            {
                processInstance.SetParameter(parameter.Name, parameter.Value, parameter.Purpose);
            }

            processInstance.RootProcessId = parentProcessInstance.RootProcessId;
            processInstance.ParentProcessId = parentProcessInstance.ProcessId;

            return processInstance;
        }

        private SchemeDefinition<TSchemeMedium> CreateSubprocessSchemeDefinition(string schemeCode, Guid rootSchemeId,TransitionDefinition startingTransition,
            ProcessDefinition parentProcessScheme)
        {
            var processDefinition = parentProcessScheme.GetSubprocessDefinition(startingTransition);
            processDefinition.Name = schemeCode;
            processDefinition.Id = Guid.NewGuid();
            processDefinition.RootSchemeId = rootSchemeId;
            processDefinition.RootSchemeCode = parentProcessScheme.RootSchemeCode ?? parentProcessScheme.Name;
            processDefinition.StartingTransition = startingTransition.Name;
            processDefinition.IsObsolete = parentProcessScheme.IsObsolete;

            var schemeDefinition = new SchemeDefinition<TSchemeMedium>(processDefinition, Parser);
            return schemeDefinition;
        }

        private static string GetSubprocessSchemeCode(TransitionDefinition startingTransition, ProcessDefinition parentProcessScheme)
        {
            return string.Format("{0}_{1}", parentProcessScheme.RootSchemeCode ?? parentProcessScheme.Name, startingTransition.Name);
        }

        private SchemeDefinition<TSchemeMedium> CreateNewScheme(string schemeCode, IDictionary<string, object> parameters)
        {
            var definingParameters = DefiningParametersSerializer.Serialize(parameters);
            SchemeDefinition<TSchemeMedium> schemeDefinition;
            var schemeId = Guid.NewGuid();
            var newScheme = Generator.Generate(schemeCode, schemeId, parameters);
            try
            {
                
                schemeDefinition = new SchemeDefinition<TSchemeMedium>(schemeId, null, schemeCode, null, newScheme,
                    false, false, null, null, definingParameters);
                SchemePersistenceProvider.SaveScheme(schemeDefinition);
            }
            catch (SchemeAlredyExistsException)
            {
                schemeDefinition = SchemePersistenceProvider.GetProcessSchemeWithParameters(schemeCode,
                    definingParameters, null,
                    true);
            }
            return schemeDefinition;
        }

        private ProcessDefinition GetProcessDefinition(SchemeDefinition<TSchemeMedium> schemeDefinition)
        {
            ProcessDefinition processDefinition;

            if (_haveCache)
            {
                var cachedDefinition = _cache.GetProcessDefinitionBySchemeId(schemeDefinition.Id);
                if (cachedDefinition != null)
                {
                    //IsObbsolete could be changed externally. Need to change it.
                    cachedDefinition.IsObsolete = schemeDefinition.IsObsolete;
                    return cachedDefinition;
                }
                processDefinition = CreateProcessDefinition(schemeDefinition);
                _cache.AddProcessDefinition(schemeDefinition.Id, processDefinition);
                return processDefinition;
            }

            processDefinition = CreateProcessDefinition(schemeDefinition);

            
            return processDefinition;
        }

        private ProcessDefinition CreateProcessDefinition(SchemeDefinition<TSchemeMedium> schemeDefinition)
        {
            var processDefinition = Parser.Parse(schemeDefinition.Scheme);

            //additional parameters for designer
            if (!string.IsNullOrWhiteSpace(schemeDefinition.DefiningParameters))
                processDefinition.AdditionalParams.Add("DefiningParameters", schemeDefinition.DefiningParameters);
            processDefinition.AdditionalParams.Add("IsObsolete", schemeDefinition.IsObsolete);
            processDefinition.Name = schemeDefinition.SchemeCode;
            processDefinition.DefiningParametersString = schemeDefinition.DefiningParameters;
            processDefinition.RootSchemeCode = schemeDefinition.RootSchemeCode;
            processDefinition.RootSchemeId = schemeDefinition.RootSchemeId;
            processDefinition.AllowedActivities = schemeDefinition.AllowedActivities;
            processDefinition.Id = schemeDefinition.Id;
            processDefinition.IsObsolete = schemeDefinition.IsObsolete;
            //code actions compile
            if (!processDefinition.IsSubprocessScheme)
                processDefinition.CodeActionsInvoker = CodeActionsCompiller.CompileCodeActions(processDefinition,
                    schemeDefinition.Id);
            //scheme markup
            processDefinition.MarkupSubprocesses();
            return processDefinition;
        }

        /// <summary>
        /// Returns existing process instance
        /// </summary>
        /// <param name="processId">Process id</param>
        /// <returns>ProcessInstance object</returns>
        public ProcessInstance GetProcessInstance(Guid processId)
        {
            var schemeDefinition = SchemePersistenceProvider.GetProcessSchemeByProcessId(processId);
            var pd = GetProcessDefinition(schemeDefinition);
            pd.Name = schemeDefinition.SchemeCode;
            return ProcessInstance.Create(schemeDefinition.Id,
                                          processId,
                                          pd,
                                          schemeDefinition.IsObsolete,schemeDefinition.IsDeterminingParametersChanged);
        }


        /// <summary>
        /// Create new scheme for existing process
        /// </summary>
        /// <param name="schemeCode">Code of the scheme</param>
        /// <param name="parameters">The parameters for creating scheme of process</param>
        /// <returns>ProcessDefinition object</returns>
        public ProcessDefinition CreateNewProcessScheme(string schemeCode, IDictionary<string, object> parameters)
        {
            SchemeDefinition<TSchemeMedium> schemeDefinition = null;
            var definingParameters = DefiningParametersSerializer.Serialize(parameters);
            var schemeId = Guid.NewGuid();
            var newScheme = Generator.Generate(schemeCode, schemeId, parameters);
            try
            {
                schemeDefinition = new SchemeDefinition<TSchemeMedium>(schemeId, null, schemeCode, null, newScheme,
                    false, false, null, null, definingParameters);
                SchemePersistenceProvider.SaveScheme(schemeDefinition);
            }
            catch (SchemeAlredyExistsException)
            {
                schemeDefinition = SchemePersistenceProvider.GetProcessSchemeWithParameters(schemeCode,
                    definingParameters, null, true);
            }

            return GetProcessDefinition(schemeDefinition);
        }



        public ProcessDefinition CreateNewSubprocessScheme(ProcessDefinition parentProcessScheme,TransitionDefinition startingTransition)
        {
             //Scheme code for subprocess {RootSchemeCode}_{StartingTransition.Name}
            var schemeCode = GetSubprocessSchemeCode(startingTransition, parentProcessScheme);

            var rootSchemeId = parentProcessScheme.RootSchemeId ?? parentProcessScheme.Id;

            var scheme = CreateSubprocessSchemeDefinition(schemeCode, rootSchemeId, startingTransition,
                parentProcessScheme);

            try
            {
                SchemePersistenceProvider.SaveScheme(scheme);
            }
            catch (SchemeAlredyExistsException)
            {
                scheme = SchemePersistenceProvider.GetProcessSchemeWithParameters(schemeCode,
                    parentProcessScheme.DefiningParametersString,rootSchemeId,true);
            }

            return GetProcessDefinition(scheme);
        }

        /// <summary>
        /// Sets the cache to store parsed ProcessDefinition objects <see cref="ProcessDefinition"/> 
        /// </summary>
        /// <param name="cache">Instance of cache object</param>
        public void SetCache(IParsedProcessCache cache)
        {
            _cache = cache;
            _haveCache = true;
        }

        /// <summary>
        /// Removes the cache to store parsed ProcessDefinition objects <see cref="ProcessDefinition"/> 
        /// </summary>
        public void RemoveCache()
        {
            _haveCache = false;
            _cache.Clear();
            _cache = null;
        }

        /// <summary>
        /// Set IsObsolete sign to the scheme with specific name and parameters for creating the scheme of the process
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        /// <param name="parameters">The parameters for creating the scheme of the process</param>
        public void SetSchemeIsObsolete(string schemeCode, Dictionary<string, object> parameters)
        {
            SchemePersistenceProvider.SetSchemeIsObsolete(schemeCode,parameters);
        }

        /// <summary>
        /// Set IsObsolete sign to the scheme with specific name
        /// </summary>
        /// <param name="schemeCode">Name of the scheme</param>
        public void SetSchemeIsObsolete(string schemeCode)
        {
            SchemePersistenceProvider.SetSchemeIsObsolete(schemeCode);
        }

        /// <summary>
        /// Returns existing process scheme directly from scheme persistence store
        /// </summary>
        /// <param name="code">Name of the scheme</param>
        /// <returns>ProcessDefinition object</returns>
        public ProcessDefinition GetProcessSchemeForDesigner(string code)
        {
            var xe = SchemePersistenceProvider.GetScheme(code);
            return Parser.Parse(xe);
        }

        /// <summary>
        /// Saves process scheme to scheme persistence store
        /// </summary>
        /// <param name="schemecode">Code of the scheme</param>
        /// <param name="pd">Object representation of the scheme</param>
        public void SaveProcessScheme(string schemecode, ProcessDefinition pd)
        {
            SchemePersistenceProvider.SaveScheme(schemecode, Parser.SerializeToString(pd));
        }


        /// <summary>
        /// Parses process scheme from the string
        /// </summary>
        /// <param name="scheme">String representation of not parsed scheme</param>
        /// <returns>ProcessDefinition object</returns>
        public ProcessDefinition Parse(string scheme)
        {
            return Parser.Parse(scheme);
        }

        /// <summary>
        /// Serialize process scheme to the string
        /// </summary>
        /// <param name="processDefinition">SProcessDefinition object</param>
        /// <returns>String representation of not parsed scheme</returns>
        public string Serialize(ProcessDefinition processDefinition)
        {
            return Parser.SerializeToString(processDefinition);
        }
    }
}
