using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;

namespace OptimaJet.Workflow.Core.CodeActions
{
    /// <summary>
    /// Code for compile by <see cref="IExternalCompiler"/>.  System purpose only.
    /// </summary>
    public struct CodeForCompillation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="codeShift">The code line with which the class definition begins</param>
        /// <param name="codeActionDefinition">Source definition</param>
        public CodeForCompillation(string code, int codeShift, CodeActionDefinition codeActionDefinition)
        {
            Code = code;
            CodeShift = codeShift;
            CodeActionDefinition = codeActionDefinition;
        }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; private set; }
        
        /// <summary>
        /// The code line with which the class definition begins
        /// </summary>
        public int CodeShift { get; private set; }
        
        /// <summary>
        /// Source definition
        /// </summary>
        public CodeActionDefinition CodeActionDefinition  { get; private set; }
    }
    
    /// <summary>
    /// External compiller interface. System purpose only.
    /// </summary>
    public interface IExternalCompiler
    {
        /// <summary>
        ///  System purpose only.
        /// </summary>
        /// <param name="codeItems"></param>
        /// <param name="referenceLocations"></param>
        /// <param name="isDebug"></param>
        /// <param name="ignoreNotCompilled"></param>
        /// <param name="compileErrors"></param>
        /// <returns></returns>
        CodeActionsInvoker Compile(List<CodeForCompillation> codeItems, List<string> referenceLocations, bool isDebug, bool ignoreNotCompilled,
            out Dictionary<string, string> compileErrors);
    }
}