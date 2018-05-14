using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Text;

namespace OptimaJet.Workflow.Core.License
{
    public abstract class BaseRestrictions
    {
        public abstract object Clone();
    }

    /// <summary>
    /// Represent restriction settings for WorkflowEngine .Net
    /// </summary>
    public sealed class WorkflowEngineNetRestrictions : BaseRestrictions
    {
        [JsonProperty(Order = 1)]
        public int MaxNumberOfActivities { get; set; }
        [JsonProperty(Order = 2)]
        public int MaxNumberOfTransitions { get; set; }
        [JsonProperty(Order = 3)]
        public int MaxNumberOfSchemes { get; set; }
        [JsonProperty(Order = 4)]
        public int MaxNumberOfThreads { get; set; }
        [JsonProperty(Order = 5)]
        public int MaxNumberOfCommands { get; set; }

        
        public override object Clone()
        {
            return new WorkflowEngineNetRestrictions()
            {
                MaxNumberOfActivities = MaxNumberOfActivities,
                MaxNumberOfSchemes = MaxNumberOfSchemes,
                MaxNumberOfThreads = MaxNumberOfThreads,
                MaxNumberOfTransitions = MaxNumberOfTransitions,
                MaxNumberOfCommands = MaxNumberOfCommands
            };
        }
    }

    /// <summary>
    /// Represent a license key
    /// </summary>
    /// <typeparam name="T">Type of restriction object</typeparam>
    public sealed class LicenseKey<T> where T : BaseRestrictions
    {
        public DateTime Expiry{ get; set; }

        public String Hash { get; set; }

        public String Ref { get; set; }

        public T Restrictions { get; set; }

        internal static LicenseKey<T> LoadFromString(string p)
        {
            var str = FromUtf8Bytes(Convert.FromBase64String(p)).Split(':');
            var restrictions = JsonConvert.DeserializeObject<T>(FromUtf8Bytes(Convert.FromBase64String(str[2])), Licensing.LicenseKeySerializerSettings);
            return new LicenseKey<T>
            {
                Ref = str[0],
                Expiry = DateTime.ParseExact(str[1], "MM.dd.yyyy", CultureInfo.InvariantCulture),
                Restrictions = restrictions,
                Hash = str[3]
            };
        }

        
        private static string FromUtf8Bytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public LicenseKey<T> Clone()
        {
            return new LicenseKey<T>
            {
                Ref = Ref,
                Restrictions = (T)Restrictions.Clone(),
                Expiry = Expiry,
                Hash = Hash
            };
        }
    }

  }
