using System;

namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Represent settings used by Workflow Designer
    /// </summary>
    public class DesignerSettings
    {
        public string Id { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Bending { get; set; }
        public string Scale { get; set; }

        public bool IdIsEmpty
        {
            get { return string.IsNullOrEmpty(Id); }
        }


        public static DesignerSettings Empty
        {
            get
            {
                return new DesignerSettings();
            }
        }

        public static string GenerateNewDesignerId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public DesignerSettings Clone()
        {
            return MemberwiseClone() as DesignerSettings;
        }
    }
}
