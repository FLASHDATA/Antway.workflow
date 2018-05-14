using System;

namespace OptimaJet.Workflow.Core.Fault
{
    public class SchemeAlredyExistsException : Exception
    {
        [Obsolete("Use SchemeAlredyExistsException.Create methods")]
        public SchemeAlredyExistsException()
        {
        }

        private SchemeAlredyExistsException(string message) : base(message)
        {
        }

        public static SchemeAlredyExistsException Create(string code, SchemeLocation location, string definingParameters = null)
        {
            return !string.IsNullOrEmpty(definingParameters) && !definingParameters.Equals("{}")
                ? new SchemeAlredyExistsException(string.Format("Scheme with the code = \"{0}\" already exists in {1}", code, location))
                : new SchemeAlredyExistsException(string.Format("Scheme with the code = \"{0}\" parameters = \"{1}\" already exists in {2}", code, definingParameters, location));
        }
    }
}
