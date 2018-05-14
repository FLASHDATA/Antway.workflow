using System;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Represent a base object in a process scheme
    /// </summary>
    public abstract class BaseDefinition 
    {
        protected BaseDefinition()
        {
            DesignerSettings = new DesignerSettings();
        }

        /// <summary>
        /// Name of the object in scheme
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Designer settings which used by Workflow Designer
        /// </summary>
        public DesignerSettings DesignerSettings { get; set; }

        public BaseDefinition Clone()
        {
            var newBaseDefinition = MemberwiseClone() as BaseDefinition;

            if (newBaseDefinition == null)
                throw new Exception("Error cloning BaseDefinition object");

            newBaseDefinition.DesignerSettings = DesignerSettings.Clone();

            return newBaseDefinition;
        }
    }
}
