using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
# if !NETCOREAPP
using System.Web;
using System.Web.UI.WebControls;
#else
using System.Net;
#endif
using OptimaJet.Workflow.Core.CodeActions;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Designer;
using OptimaJet.Workflow.Core.Subprocess;

namespace OptimaJet.Workflow
{
    /// <summary>
    /// Extension which provides API for HTML5 Workflow Designer
    /// </summary>
    public static partial class Designer
    {
        /// <summary>
        /// API for HTML5 Workflow Designer. Contains following operations:
        /// - bool exists(schemecode, schemeid, processid) Checks existence of the scheme of the process with specific code, id or id of the process 
        /// - JSON load(schemecode, schemeid, processid) Returns the scheme of the process in JSON format by specific code, id or id of the process 
        /// - JSON save(schemecode, data) Saves the scheme with specified code. Ruturns updated scheme.
        /// - JSON uploadscheme(filestream) Converts scheme uploaded as XML file to JSON.
        /// - XML downloadscheme(data) Converts scheme from JSON object to XML.
        /// - object compile(data) Method for test compile of code actions
        /// - JSON getemptytype(typename) returns serialized empty type
        /// - array getautocompletesuggestions (category, value) returns a list of autocomplete suggestions
        /// - JSON uploadschemebpmn(filestream) Converts BPMN scheme uploaded as XML file to JSON.
        /// - XML downloadschemebpmn(data) Converts BPMN scheme from JSON object to XML.
        /// Type of the operation determined by "operation" parameter from request parameters 
        /// </summary>
        /// <param name="runtime">The instance of the runtime</param>
        /// <param name="form">Parameters from request. Allowed parameters is: "operation", "schemecode", "schemeid", "processid", "data"</param>
        /// <param name="filestream">Stream which represent uploaded file</param>
        /// <param name="setIsObsoleForOperationSave">If true and operation is "save" IsObsolete property will be set to all shemes of processes with specified code</param>
        /// <returns>Operation execution result</returns>
        public static string DesignerAPI(this WorkflowRuntime runtime,
            NameValueCollection form,
            Stream filestream = null,
            bool setIsObsoleForOperationSave = true)
        {
            if (string.IsNullOrWhiteSpace(form["operation"]))
            {
                throw new Exception("operation is not exists!");
            }
            var operation = form["operation"].ToLower();
            var schemecode = form["schemecode"];
            var schemeid = form["schemeid"];
            var processid = form["processid"];

            switch (operation)
            {
                case "exists":
                    bool exists = false;
                    try
                    {
                        InitializeProcessDefinition(runtime, schemecode, schemeid, processid);
                        exists = true;
                    }
                    catch (ProcessNotFoundException)
                    {
                    }
                    catch (SchemeNotFoundException)
                    {
                    }

                    return JsonConvert.SerializeObject(exists);
                case "load":
                    try
                    {
                        return Load(runtime, schemecode, schemeid, processid);
                    }
                    catch (Exception ex)
                    {
                        return JsonConvert.SerializeObject(new {isError = true, errorMessage = ex.Message});
                    }
                case "save":
                    try
                    {
                        return Save(runtime, form, setIsObsoleForOperationSave, processid, schemeid, schemecode);
                    }
                    catch (Exception ex)
                    {
                        return JsonConvert.SerializeObject(new {isError = true, errorMessage = ex.Message});
                    }
                case "uploadscheme":
                    try
                    {
                        return Upload(runtime, filestream, processid, schemeid);
                    }
                    catch (Exception ex)
                    {
                        return JsonConvert.SerializeObject(new {isError = true, errorMessage = ex.Message, stackTrace = ex.StackTrace });
                    }
                case "downloadscheme":
                    try
                    {
                        return Download(runtime, form);
                    }
                    catch (Exception ex)
                    {
                        return JsonConvert.SerializeObject(new { isError = true, errorMessage = ex.Message, stackTrace = ex.StackTrace });
                    }
                case "uploadschemebpmn":
                    try
                    {
                        return UploadBPMN(runtime, filestream, processid, schemeid);
                    }
                    catch (Exception ex)
                    {
                        return JsonConvert.SerializeObject(new { isError = true, errorMessage = ex.Message, stackTrace = ex.StackTrace });
                    }
                case "downloadschemebpmn":
                    return DownloadBPMN(runtime, form);
                case "compile":
                    var codeAction = JsonConvert.DeserializeObject<CodeActionDefinition>(form["data"]);
                    if (string.IsNullOrEmpty(codeAction.Name))
                        codeAction.Name = string.Format("Test_{0}", Guid.NewGuid().ToString("N"));
                    var res = Compile(codeAction);
                    return JsonConvert.SerializeObject(res);
                case "getemptytype":
                    try
                    {
                        var typeName = JsonConvert.DeserializeObject<string>(form["data"]);
#if !NETCOREAPP
                        typeName = HttpUtility.UrlDecode(typeName);
#else
                        typeName = WebUtility.UrlDecode(typeName);
#endif
                        var parsedType = ParsedType.Parse(typeName);
                        var type = parsedType.ConvertToType();
                        if (type == null)
                            return string.Empty;
                        return JsonConvert.SerializeObject(Activator.CreateInstance(type));
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                case "getautocompletesuggestions":
                    if (runtime.DesignerAutocompleteProvider == null)
                        return "[]";
                    var category = (SuggestionCategory) Enum.Parse(typeof(SuggestionCategory), form["category"], true);
                    var value = form["value"];
                    return JsonConvert.SerializeObject(runtime.DesignerAutocompleteProvider.GetAutocompleteSuggestions(category, value));
                case "deleteglobalcodeaction":
                    try
                    {
                        return JsonConvert.SerializeObject(DeleteGlobalActions(runtime, form));
                    }
                    catch (Exception ex)
                    {
                        return JsonConvert.SerializeObject(new { isError = true, errorMessage = ex.Message, stackTrace = ex.StackTrace });
                    }
                    break;
            }
            return null;
        }

        private static string Download(WorkflowRuntime runtime, NameValueCollection form)
        {
            var pd = JsonConvert.DeserializeObject<ProcessDefinition>(form["data"]);
            pd.CodeActions.ForEach(ca => ca.Decode());
            return runtime.Builder.Serialize(pd);
        }

        private static string Load(WorkflowRuntime runtime, 
                                   string schemecode, string schemeid, string processid)
        {
            var commands = runtime
                            .CommandMapping
                            .GetMappedCommands();

            if (schemecode == null)
            {
                return NewScheme(runtime, schemeid, processid, commands);
            }

            List<CodeActionDefinition> globalActions;
            ProcessDefinition pd;
            globalActions =
                runtime.PersistenceProvider.LoadGlobalParameters<CodeActionDefinition>(
                    WorkflowRuntime.CodeActionsGlobalParameterName);
            pd = InitializeProcessDefinition(runtime, schemecode, schemeid, processid);
            pd.CodeActions.AddRange(globalActions);

            pd.CodeActions = pd.CodeActions.Select(ca => ca.Encode()).ToList();

            if (commands != null)
            {
                foreach(var command in commands)
                {
                    if (pd.Commands.Any(c => c.Name == command.Name)) continue;
                    pd.Commands.Add(command);
                }
            }

            return JsonConvert.SerializeObject(pd);
        }

        private static string Upload(WorkflowRuntime runtime, Stream filestream, string processid, string schemeid)
        {
            ProcessDefinition pd;
            if (!string.IsNullOrWhiteSpace(processid) || !string.IsNullOrWhiteSpace(schemeid))
            {
                throw new Exception("The UploadScheme is available by schemecode only!");
            }

            StreamReader reader = new StreamReader(filestream);
            var text = reader.ReadToEnd();
            pd = runtime.Builder.Parse(text);
            pd.CodeActions.ForEach(ca =>
            {
                if (!ca.WasEncoded())
                    ca.Encode();
            });
            InitializeAdditionalParams(runtime, pd);
            return JsonConvert.SerializeObject(pd);
        }

        private static string Save(WorkflowRuntime runtime, NameValueCollection form, bool setIsObsoleForOperationSave, string processid, string schemeid, string schemecode)
        {
            ProcessDefinition pd;
            List<CodeActionDefinition> globalActions;
            if (!string.IsNullOrWhiteSpace(processid) || !string.IsNullOrWhiteSpace(schemeid))
            {
                throw new Exception("The Save is available by schemecode only!");
            }

            pd = JsonConvert.DeserializeObject<ProcessDefinition>(form["data"]);
            pd.AdditionalParams = new Dictionary<string, object>();

            pd.CodeActions = pd.CodeActions.Select(ca => ca.Decode()).ToList();

            if (pd.CodeActions.Any(ca => ca.IsGlobal))
            {
                globalActions = pd.CodeActions.Where(ca => ca.IsGlobal).ToList();

                pd.CodeActions = pd.CodeActions.Where(ca => !ca.IsGlobal).ToList();

                globalActions.ForEach(
                    ga =>
                        runtime.PersistenceProvider.SaveGlobalParameter(
                            WorkflowRuntime.CodeActionsGlobalParameterName, ga.Name, ga));
            }

            runtime.Builder.SaveProcessScheme(schemecode, pd);

            if (setIsObsoleForOperationSave)
            {
                runtime.SetSchemeIsObsolete(schemecode);
            }

            pd = InitializeProcessDefinition(runtime, schemecode, schemeid, processid);
            globalActions = runtime.PersistenceProvider.LoadGlobalParameters<CodeActionDefinition>(WorkflowRuntime.CodeActionsGlobalParameterName);
            pd.CodeActions.AddRange(globalActions);
            pd.CodeActions = pd.CodeActions.Select(ca => ca.Encode()).ToList();
            return JsonConvert.SerializeObject(pd);
        }

        private static ProcessDefinition InitializeProcessDefinition(this WorkflowRuntime runtime, string schemecode, string schemeid, string processid)
        {  
            ProcessDefinition pd = null;
            if (!string.IsNullOrWhiteSpace(processid))
            {
                var processId = new Guid(processid);
                var pi = runtime.Builder.GetProcessInstance(processId);
                runtime.PersistenceProvider.FillSystemProcessParameters(pi);
                pd = pi.ProcessScheme.Clone();
                pd.AdditionalParams.Clear();
                if (!pd.AdditionalParams.ContainsKey("ProcessParameters"))
                    pd.AdditionalParams.Add("ProcessParameters", pi.ProcessParameters);
                var subprocessActivities = new List<string>();
                var processTree = runtime.GetProcessInstancesTree(processId);
                if (processTree != null)
                {
                    void AddSubprocessActivity (ProcessInstancesTree tree, List<string> acc, bool includeCurrent)
                    {
                        if (includeCurrent)
                            acc.Add(runtime.GetCurrentActivityName(tree.Id));
                        foreach (var child in tree.Children)
                        {
                            AddSubprocessActivity(child, acc, true);
                        }
                    }
                    
                    AddSubprocessActivity(processTree,subprocessActivities,false);
                }

                if (pd.AdditionalParams.ContainsKey("SubprocessCurrentActivities"))
                    pd.AdditionalParams.Remove("SubprocessCurrentActivities");
                pd.AdditionalParams.Add("SubprocessCurrentActivities",subprocessActivities);



            }
            else if (!string.IsNullOrWhiteSpace(schemeid))
            {
                pd = runtime.Builder.GetProcessScheme(new Guid(schemeid));
                pd.AdditionalParams.Clear();
            }
            else if(!string.IsNullOrEmpty(schemecode))
            {
                pd = runtime.Builder.GetProcessSchemeForDesigner(schemecode);
                pd.AdditionalParams.Clear();
            }
            else
            {
                pd = new ProcessDefinition();
            }

            InitializeAdditionalParams(runtime, pd);
            return pd;
        }

        private static void InitializeAdditionalParams(WorkflowRuntime runtime, ProcessDefinition pd)
        {
            if (!pd.AdditionalParams.ContainsKey("Rules"))
                pd.AdditionalParams.Add("Rules", runtime.RuleProvider.GetRules());
            if (!pd.AdditionalParams.ContainsKey("TimerTypes"))
                pd.AdditionalParams.Add("TimerTypes", Enum.GetValues(typeof(TimerType)).Cast<TimerType>().Select(t => t.ToString()));

            if (!pd.AdditionalParams.ContainsKey("Conditions"))
            {
                List<string> conditions;
                try //Do not transfer try/catch to Java Version
                {
                    conditions = runtime.ActionProvider.GetConditions();
                }
                catch (NotImplementedException)
                {
                    conditions = runtime.ActionProvider.GetActions();
                }

                pd.AdditionalParams.Add("Conditions", conditions);
            }

            if (!pd.AdditionalParams.ContainsKey("Actions"))
                pd.AdditionalParams.Add("Actions", runtime.ActionProvider.GetActions());

            if (!pd.AdditionalParams.ContainsKey("Namespaces"))
                pd.AdditionalParams.Add("Usings", CodeActionsCompiller.Usings);
            if (!pd.AdditionalParams.ContainsKey("Types"))
                pd.AdditionalParams.Add("Types", _registeredTypeNames);
        }

        private static List<string> _registeredTypeNames = new List<string> {"String", "Char", "Byte", "Int16", "Int32", "Int64", "Single", "Double", "Decimal", "DateTime"};

        public static void RegisterTypesFromAssembly(Assembly assembly, Func<Type, bool> filter = null)
        {
            var typeNames = assembly.GetTypes().Where(t => t.GetTypeInfo().IsPublic && (filter == null || filter(t))).Select(t =>
            {
                try
                {
                    return ParsedType.GetFriendlyName(t);
                }
                catch (Exception)
                {
                    //Just for designer purpose
                    return null;
                }
            }).Where(s => !string.IsNullOrEmpty(s)).ToList();
            typeNames.AddRange(_registeredTypeNames);
            _registeredTypeNames = typeNames.Distinct().ToList();
        }

        private static dynamic Compile(CodeActionDefinition codeActionDefinition)
        {
            if (!CodeActionsCompiller.CompillationEnable)
                return new { Success = false, Message = "Code actions compillation disabled." };

            codeActionDefinition = codeActionDefinition.Decode();

            try
            {
                CodeActionsCompiller.CompileCodeActions(new List<CodeActionDefinition> { codeActionDefinition }, string.Format("TestCompile_{0}", Guid.NewGuid().ToString("N")));
            }
            catch (Exception ex)
            {
                return new {Success = false, Message = ex.Message.Replace("\n","<br/>")};
            }

            return new { Success = true};
        }

        private static dynamic DeleteGlobalActions(WorkflowRuntime runtime, NameValueCollection form)
        {
            var names = form["names"];
            if (string.IsNullOrWhiteSpace(names))
            {
                return new { Success = false, Message = "names parameter is required." };
            }

            try
            {
                var namesArray = JsonConvert.DeserializeObject<string[]>(names);
                foreach (var name in namesArray)
                    runtime.PersistenceProvider.DeleteGlobalParameters(WorkflowRuntime.CodeActionsGlobalParameterName, name);
            }
            catch (Exception ex)
            {
                return new { Success = false, Message = ex.Message.Replace("\n", "<br/>") };
            }

            return new { Success = true };
        }

        private static CodeActionDefinition Decode(this CodeActionDefinition codeActionDefinition)
        {
#if !NETCOREAPP
            codeActionDefinition.ActionCode = HttpUtility.UrlDecode(codeActionDefinition.ActionCode);
            codeActionDefinition.Usings = HttpUtility.UrlDecode(codeActionDefinition.Usings);
#else
            codeActionDefinition.ActionCode = WebUtility.UrlDecode(codeActionDefinition.ActionCode);
            codeActionDefinition.Usings = WebUtility.UrlDecode(codeActionDefinition.Usings);
#endif
            return codeActionDefinition;
        }

        private static CodeActionDefinition Encode(this CodeActionDefinition codeActionDefinition)
        {
#if !NETCOREAPP
            codeActionDefinition.ActionCode = HttpUtility.UrlEncode(codeActionDefinition.ActionCode).Replace("+", "%20");
            codeActionDefinition.Usings = HttpUtility.UrlEncode(codeActionDefinition.Usings).Replace("+", "%20");
#else
            codeActionDefinition.ActionCode = WebUtility.UrlEncode(codeActionDefinition.ActionCode).Replace("+", "%20");
            codeActionDefinition.Usings = WebUtility.UrlEncode(codeActionDefinition.Usings).Replace("+", "%20");
#endif
            return codeActionDefinition;
        }

        private static bool WasEncoded(this CodeActionDefinition codeActionDefinition)
        {
            if (!string.IsNullOrEmpty(codeActionDefinition.Usings))
            {
                var oldUsings = codeActionDefinition.Usings;
#if !NETCOREAPP
                var newUsings = HttpUtility.UrlDecode(codeActionDefinition.Usings);
#else
                var newUsings = WebUtility.UrlDecode(codeActionDefinition.Usings);
#endif
                return oldUsings != newUsings;

            }

            var oldCode = codeActionDefinition.ActionCode;
#if !NETCOREAPP
            var newCode = HttpUtility.UrlDecode(codeActionDefinition.ActionCode);
#else
            var newCode = WebUtility.UrlDecode(codeActionDefinition.ActionCode);
#endif
            return oldCode != newCode;
        }

        #region BPMN
        private static string DownloadBPMN(WorkflowRuntime runtime, NameValueCollection form)
        {
            var pd = JsonConvert.DeserializeObject<ProcessDefinition>(form["data"]);
            pd.CodeActions.ForEach(ca => ca.Decode());

            var bpmnScheme = new Core.BPMN.Definition(pd);
            return bpmnScheme.Serialize();
        }

        private static string UploadBPMN(WorkflowRuntime runtime, Stream filestream, string processid, string schemeid)
        {
            ProcessDefinition pd;
            if (!string.IsNullOrWhiteSpace(processid) || !string.IsNullOrWhiteSpace(schemeid))
            {
                throw new Exception("The UploadSchemeBPMN is available by schemecode only!");
            }

            StreamReader reader = new StreamReader(filestream);
            var text = reader.ReadToEnd();

            var bpmnScheme = new Core.BPMN.Definition(text);

            pd = bpmnScheme.ConvertToComplexProcessDefinitions();
            InitializeAdditionalParams(runtime, pd);
            return JsonConvert.SerializeObject(pd);
        }
        #endregion
    }
}
