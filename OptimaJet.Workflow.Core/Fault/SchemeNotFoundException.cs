using System;

namespace OptimaJet.Workflow.Core.Fault
{
    public enum SchemeLocation
    {
        WorkflowScheme,
        WorkflowProcessScheme,
        WorkflowProcessInstance
    }

    public class SchemeNotFoundException : Exception
    {
        [Obsolete("Use SchemeNotFoundException.Create methods")]
        public SchemeNotFoundException()
        {
        }

        private SchemeNotFoundException(string message) : base(message)
        {
        }

        public static SchemeNotFoundException Create(string code, SchemeLocation location)
        {
            return new SchemeNotFoundException(string.Format("Scheme with the code = \"{0}\" is not found in {1}", code, location));
        }

        public static SchemeNotFoundException Create(string code, SchemeLocation location, string definingParameters = null)
        {
            return !string.IsNullOrEmpty(definingParameters) && !definingParameters.Equals("{}")
                ? new SchemeNotFoundException(string.Format("Scheme with the code = \"{0}\" is not found in {1}", code, location))
                : new SchemeNotFoundException(string.Format("Scheme with the code = \"{0}\" parameters = \"{1}\" is not found in {2}", code, definingParameters, location));
        }

        public static SchemeNotFoundException Create(Guid id, SchemeLocation location)
        {
            if (location == SchemeLocation.WorkflowProcessInstance)
            {
                return new SchemeNotFoundException(string.Format("SchemeId is null for WorkflowProcessInstance with id ={0}", id));
            }

            return new SchemeNotFoundException(string.Format("Scheme with the id = \"{0}\" is not found in {1}", id, location));
        }

    }
}
