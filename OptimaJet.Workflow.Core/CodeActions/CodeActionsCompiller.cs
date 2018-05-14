using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using OptimaJet.Workflow.Core.Model;
#if !NETCOREAPP
using System.CodeDom.Compiler;
using Microsoft.CSharp;
#else
using System.IO;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;
#endif


namespace OptimaJet.Workflow.Core.CodeActions
{
    /// <summary>
    /// Provides compillation of code actions
    /// </summary>
    public static class CodeActionsCompiller
    {
        public static IExternalCompiler ExternalCompiler { get; set; }
        
        public static List<string> DefaultUsings = new List<string>
        {
            "System","System.Collections","System.Collections.Generic","System.Linq","System.Threading","System.Threading.Tasks","OptimaJet.Workflow","OptimaJet.Workflow.Core.Model"
        };

        static CodeActionsCompiller()
        {
            CompillationEnable = true;
            DebugMode = false;
        }

        public static bool CompillationEnable { get; set; }
        public static bool DebugMode { get; set; }
        
        public static void RegisterAssembly(string longName)
        {
#if NETCOREAPP
            var assembly = Assembly.Load(new AssemblyName(longName));
#else
            var assembly = Assembly.Load(longName);
#endif

            RegisterAssembly(assembly);
        }

        public static void RegisterAssembly(Assembly assembly)
        {
            if (assembly != null && !string.IsNullOrEmpty(assembly.Location))
            {
                LazyLocations.Value.AddOrUpdate(assembly.Location, 0, (s, b) => 0);
                GetAllNamespacesFromAssembly(assembly).ToList().ForEach(ns=>LazyUsings.Value.AddOrUpdate(ns,0,(s, b) => 0));
            }
        }
        
        private static readonly Lazy<ConcurrentDictionary<string, byte>> LazyLocations = new Lazy<ConcurrentDictionary<string, byte>>(FillDefaultLocations);

        private static ConcurrentDictionary<string, byte> FillDefaultLocations()
        {
            var locations = new HashSet<string>();
#if NETCOREAPP
    #if NETCORE2
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
    #else
            var loadedAssemblies = AppDomainNetCore.CurrentDomain.GetAssemblies().ToList();
    #endif
#else
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
#endif

            foreach (var loadedAssembly in loadedAssemblies)
            {
                try
                {
                    var loadedAssemblyLocation = loadedAssembly.Location;
                    if (!string.IsNullOrEmpty(loadedAssemblyLocation) && !locations.Contains(loadedAssemblyLocation))
                    {
                        locations.Add(loadedAssemblyLocation);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return new ConcurrentDictionary<string, byte>(locations.Select(l=>new KeyValuePair<string, byte>(l,0)));
        }

        private static readonly Lazy<ConcurrentDictionary<string, byte>> LazyUsings = new Lazy<ConcurrentDictionary<string, byte>>(FillDefaultUsings);

        private static ConcurrentDictionary<string, byte> FillDefaultUsings()
        {
            return new ConcurrentDictionary<string, byte>(DefaultUsings.Select(l => new KeyValuePair<string, byte>(l, 0)));
        }

        public static IEnumerable<string> Usings => LazyUsings.Value.Keys;

        public static IEnumerable<string> Locations => LazyLocations.Value.Keys;

        private static IEnumerable<string> GetAllNamespacesFromAssembly(Assembly loadedAssembly)
        {
            return loadedAssembly.GetTypes()
                .Select(t => t.Namespace)
                .Distinct().Where(n => !string.IsNullOrEmpty(n) && !n.StartsWith("<"));
        }

        private const string RuleGetFormat = @"public static IEnumerable<string> {0} (ProcessInstance processInstance, OptimaJet.Workflow.Core.Runtime.WorkflowRuntime runtime, string parameter) {{";
        
        private const string RuleCheckFormat = @"public static bool {0} (ProcessInstance processInstance, OptimaJet.Workflow.Core.Runtime.WorkflowRuntime runtime, string identityId, string parameter) {{";
        
        public static CodeActionsInvoker CompileCodeActions(ProcessDefinition processDefinition, Guid schemeId)
        {
            var codeActionDefinitions = processDefinition.CodeActions;

            if (!codeActionDefinitions.Any())
                return new CodeActionsInvoker();
            var nsname = $"CodeActions_{processDefinition.Name}_{schemeId:N}";

            return CodeActionsInvoker(nsname, codeActionDefinitions, false, out _);
        }

        public static CodeActionsInvoker CompileGlobalCodeActions(List<CodeActionDefinition> codeActionDefinitions, bool ignoreNotCompilled,
            out Dictionary<string, string> compileErrors)
        {
            compileErrors = new Dictionary<string, string>();
            
            if (!codeActionDefinitions.Any())
                return new CodeActionsInvoker();

            const string typeprefix = "Global";

            return CodeActionsInvoker(typeprefix, codeActionDefinitions, ignoreNotCompilled, out compileErrors);
        }

        public static CodeActionsInvoker CompileCodeActions(List<CodeActionDefinition> codeActionDefinitions, string namespaceName)
        {
            if (!codeActionDefinitions.Any())
                return new CodeActionsInvoker();

            return CodeActionsInvoker(namespaceName, codeActionDefinitions, false, out _);
        }

        private static CodeActionsInvoker CodeActionsInvoker(string namespacePostfix,
            List<CodeActionDefinition> codeActionDefinitions, bool ignoreNotCompilled,
        out Dictionary<string, string> compileErrors)
        {
            if (!codeActionDefinitions.Any() || !CompillationEnable)
            {
                compileErrors = new Dictionary<string, string>();
                return new CodeActionsInvoker();
            }

            if (ExternalCompiler != null)
            {
                var codeItems = new List<CodeForCompillation>();
                foreach (var codeActionDefinition in codeActionDefinitions)
                {
                    var codeToCompile = GetCodeToCompile(codeActionDefinitions, codeActionDefinition, namespacePostfix, out var lineshift);
                    codeItems.Add(new CodeForCompillation(codeToCompile,lineshift,codeActionDefinition));
                }

                return ExternalCompiler.Compile(codeItems, Locations.ToList(), DebugMode, ignoreNotCompilled, out compileErrors);
            }
            
            return Compile(namespacePostfix, codeActionDefinitions, ignoreNotCompilled, out compileErrors);
        }

        private static string GetCodeToCompile(List<CodeActionDefinition> codeActionDefinitions, CodeActionDefinition codeActionDefinition, string namespacePostfix, out int lineshift)
        {
            lineshift = 0;
            var codeToCompile = new StringBuilder();
            var usings = codeActionDefinition.Usings.Split(';');

            foreach (var use in usings)
            {
                if (string.IsNullOrEmpty(use))
                    continue;
                codeToCompile.AppendFormat("using {0};\r\n", use);
                lineshift++;
            }

            var namespaceName = CodeActionUtils.GetNamespaceName(namespacePostfix);
            codeToCompile.AppendFormat(@"namespace {0} {{", namespaceName);
            codeToCompile.Append("\r\n");
            lineshift++;

            var className = CodeActionUtils.GetClassName(codeActionDefinition);
            codeToCompile.AppendFormat("public static class {0} {{", className);
            codeToCompile.Append("\r\n");
            lineshift++;


            var methodName = CodeActionUtils.GetMethodName(codeActionDefinition).ToValidCSharpIdentifierName();

            var actionCode = codeActionDefinition.ActionCode;

            if (DebugMode)
            {
                actionCode = actionCode.Replace(@"/*break*/", @"if (System.Diagnostics.Debugger.IsAttached) {
System.Diagnostics.Debugger.Break();
}");
            }

            if (codeActionDefinition.Type == CodeActionType.Condition)
            {
                codeToCompile.Append(
                    codeActionDefinition.IsAsync
                        ? $"public static async Task<bool> {methodName} (ProcessInstance processInstance, OptimaJet.Workflow.Core.Runtime.WorkflowRuntime runtime, string parameter, CancellationToken token) {{"
                        : $"public static bool {methodName} (ProcessInstance processInstance, OptimaJet.Workflow.Core.Runtime.WorkflowRuntime runtime, string parameter) {{");

                codeToCompile.Append("\r\n");
                codeToCompile.Append(actionCode);
                codeToCompile.Append("\r\n}\r\n");
            }
            else if (codeActionDefinition.Type == CodeActionType.Action)
            {
                codeToCompile.Append(
                    codeActionDefinition.IsAsync
                        ? $"public static async Task {methodName} (ProcessInstance processInstance, OptimaJet.Workflow.Core.Runtime.WorkflowRuntime runtime, string parameter, CancellationToken token) {{"
                        : $"public static void {methodName} (ProcessInstance processInstance, OptimaJet.Workflow.Core.Runtime.WorkflowRuntime runtime, string parameter) {{");

                codeToCompile.Append("\r\n");
                codeToCompile.Append(actionCode);
                codeToCompile.Append("\r\n}\r\n");
            }
            else
            {
                if (codeActionDefinition.Type == CodeActionType.RuleGet)
                {
                    codeToCompile.AppendFormat(RuleGetFormat, methodName);
                    codeToCompile.Append("\r\n");
                    codeToCompile.Append(actionCode);
                    codeToCompile.Append("\r\n}\r\n");

                    if (!codeActionDefinitions.Any(cd =>
                        cd.Type == CodeActionType.RuleCheck &&
                        cd.Name.Equals(codeActionDefinition.Name, StringComparison.Ordinal)))
                    {
                        codeToCompile.AppendFormat(
                            RuleCheckFormat,
                            CodeActionUtils.GetMethodName(codeActionDefinition.Name, CodeActionType.RuleCheck));
                        codeToCompile.Append("\r\n");
                        codeToCompile.AppendFormat(
                            "return {0}(processInstance,runtime,parameter).Any(id=>id.Equals(identityId,StringComparison.InvariantCultureIgnoreCase));",
                            methodName);
                        codeToCompile.Append("\r\n}\r\n");
                    }
                }
                else if (codeActionDefinition.Type == CodeActionType.RuleCheck)
                {
                    codeToCompile.AppendFormat(RuleCheckFormat, methodName);
                    codeToCompile.Append("\r\n");
                    codeToCompile.Append(actionCode);
                    codeToCompile.Append("\r\n}\r\n");

                    if (
                        !codeActionDefinitions.Any(
                            cd =>
                                cd.Type == CodeActionType.RuleGet &&
                                cd.Name.Equals(codeActionDefinition.Name, StringComparison.Ordinal)))
                    {
                        codeToCompile.AppendFormat(
                            RuleGetFormat,
                            CodeActionUtils.GetMethodName(codeActionDefinition.Name, CodeActionType.RuleGet));
                        codeToCompile.Append("\r\n");
                        codeToCompile.Append("return new List<string> ();");
                        codeToCompile.Append("\r\n}\r\n");
                    }
                }
            }


            codeToCompile.Append("}");
            codeToCompile.Append("\r\n}");
            return codeToCompile.ToString();
        }



#if NETCOREAPP

       
        private static CodeActionsInvoker Compile(string namespacePostfix, List<CodeActionDefinition> codeActionDefinitions, bool ignoreNotCompilled,
            out Dictionary<string, string> compileErrors, List<(CodeActionType type, string name)> excludes =  null)
        {
            var directory = Path.GetDirectoryName(Environment.GetEnvironmentVariable("TEMP"));
            var assemblyName = $"{CodeActionUtils.GetNamespaceName(namespacePostfix)}_{Guid.NewGuid():N}";

            var trees = new List<(SyntaxTree tree, int lineShift, CodeActionDefinition codeAction)>();

            foreach (var codeActionDefinition in codeActionDefinitions)
            {
                if (excludes != null && excludes.Any(e=> e.name.Equals(codeActionDefinition.Name,StringComparison.Ordinal) && e.type == codeActionDefinition.Type))
                    continue;
                
                var codeToCompile = GetCodeToCompile(codeActionDefinitions, codeActionDefinition, namespacePostfix, out var lineshift);

                SyntaxTree syntaxTree;
                if (DebugMode)
                {
                    var combine = Path.Combine(directory, string.Format("{0}_{1}.cs", assemblyName, CodeActionUtils.GetClassName(codeActionDefinition)));
                    File.WriteAllText(combine, codeToCompile, Encoding.UTF8);
                    syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile, CSharpParseOptions.Default, combine, Encoding.UTF8);
                }
                else
                {
                    syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);
                }

                trees.Add((tree: syntaxTree, lineShift: lineshift, codeAction: codeActionDefinition));
            }
           

            var references = Locations.Select(l => MetadataReference.CreateFromFile(l)).ToList();

            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: DebugMode ? OptimizationLevel.Debug : OptimizationLevel.Release);

            var method = typeof(CSharpCompilationOptions).GetMethod("WithTopLevelBinderFlags", BindingFlags.NonPublic | BindingFlags.Instance);
            // we need to pass BinderFlags.IgnoreCorLibraryDuplicatedTypes, but it's an internal class 
            if (method != null)
            {
                options = (CSharpCompilationOptions) method.Invoke(options, new object[] {1u << 26});
            }

            var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees: trees.Select(t => t.Item1), references: references,
                options: options);

            EmitResult result;
            Assembly compilledAssembly = null;

            if (DebugMode)
            {
                var dllName = string.Format("{0}.dll", assemblyName);
                var pdbName = string.Format("{0}.pdb", assemblyName);
                 
                result = compilation.Emit(Path.Combine(directory, dllName), Path.Combine(directory, pdbName));

                if (result.Success)
                {
                    compilledAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(directory, dllName));
                }
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    result = compilation.Emit(ms);

                    if (result.Success)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        compilledAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    }
                }
            }

            if (!result.Success)
            {
                var newExcludes = new List<(CodeActionType type, string name)>();
                var errorBuilder = new StringBuilder();
                bool forceThrow = false;
                
                var groopedErrors = new Dictionary<string,StringBuilder>();
                
                foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    var tuple = trees.FirstOrNull(t => t.tree.IsEquivalentTo(diagnostic.Location.SourceTree));
                   
                    if (tuple == null)
                    {
                        errorBuilder.AppendLine(diagnostic.GetMessage());
                        forceThrow = true;
                    }
                    else
                    {
                        var exclude = (type: tuple.Value.codeAction.Type, name: tuple.Value.codeAction.Name);
                        if (!newExcludes.Contains(exclude))
                        {
                            newExcludes.Add(exclude);
                        }
                        
                        var errorName = $"{tuple.Value.codeAction.Type}_{tuple.Value.codeAction.Name}";
                        if (!groopedErrors.ContainsKey(errorName))
                            groopedErrors.Add(errorName, new StringBuilder());
                        
                        var shift = tuple.Value.lineShift;
                        var position = diagnostic.Location.GetLineSpan().StartLinePosition;
                        var lineNumber = position.Line - shift;
                        if (lineNumber >= 0)
                        {
                            var errorInCode = $"({lineNumber}:{position.Character}): error {diagnostic.Descriptor.Id}: {diagnostic.GetMessage()}";
                            groopedErrors[errorName].AppendLine(errorInCode);
                            errorBuilder.AppendLine(errorInCode);
                        }
                        else
                        {
                            var errorInUsings = String.Format("({0}): error {1}: {2}", "Using section", diagnostic.Descriptor.Id, diagnostic.GetMessage());
                            groopedErrors[errorName].AppendLine(errorInUsings);
                            errorBuilder.AppendLine(errorInUsings);
                        }
                    }
                }

                if (!ignoreNotCompilled || forceThrow || excludes != null)
                    throw new InvalidOperationException(errorBuilder.ToString());

                compileErrors = groopedErrors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
                return Compile(namespacePostfix, codeActionDefinitions, true, out _, newExcludes);
            }

            var invoker = new CodeActionsInvoker();

            if (compilledAssembly != null)
                foreach (var type in compilledAssembly.GetTypes())
                {
                    invoker.AddCompilledType(type);
                }

            compileErrors = new Dictionary<string, string>();

            return invoker;
        }
#else
        private static CodeActionsInvoker Compile(string namespacePostfix, List<CodeActionDefinition> codeActionDefinitions,  bool ignoreNotCompilled,
            out Dictionary<string, string> compileErrors)
        {
            compileErrors = new Dictionary<string, string>();
            var errorsBuilder = new StringBuilder();
            var invoker = new CodeActionsInvoker();
            foreach (var codeActionDefinition in codeActionDefinitions)
            {
                var codeToCompile = GetCodeToCompile(codeActionDefinitions, codeActionDefinition, namespacePostfix, out var lineshift);

                using (var provider = new CSharpCodeProvider(new Dictionary<String, String> {{"CompilerVersion", "v4.0"}}))
                {
                    var parameters = new CompilerParameters();

                    if (DebugMode)
                    {
                        parameters.GenerateInMemory = false;
                        parameters.IncludeDebugInformation = true;
                        parameters.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
                    }
                    else
                        parameters.GenerateInMemory = true;

                    parameters.ReferencedAssemblies.AddRange(Locations.ToArray());
                    var results = provider.CompileAssemblyFromSource(parameters, codeToCompile.ToString());


                    var hasErrors = results.Errors.HasErrors;

                    if (!hasErrors)
                    {
                        var assembly = results.CompiledAssembly;
                        var type = assembly.GetTypes().First();
                        invoker.AddCompilledType(type);
                    }
                    else
                    {
                        var localErrorBuilder = new StringBuilder();
                        foreach (CompilerError error in results.Errors)
                        {

                            if (error.IsWarning)
                                continue;

                            var lineNumber = error.Line - lineshift - 1;
                            localErrorBuilder.AppendLine(lineNumber > 0
                                ? $"({lineNumber}:{error.Column}): error {error.ErrorNumber}: {error.ErrorText}"
                                : $"(Using section): error {error.ErrorNumber}: {error.ErrorText}");
                        }

                        if (!ignoreNotCompilled)
                        {
                            errorsBuilder.Append(localErrorBuilder);
                        }

                        compileErrors.Add($"{codeActionDefinition.Type}_{codeActionDefinition.Name}", localErrorBuilder.ToString());
                    }
                }
            }

            if (errorsBuilder.Length > 0 && !ignoreNotCompilled)
                throw new InvalidOperationException(errorsBuilder.ToString());

            return invoker;

        }
#endif

    }
}
