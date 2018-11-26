using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

#if NETCOREAPP
using  OptimaJet.Workflow.Core;
#endif


namespace OptimaJet.Workflow.Core.License
{
    internal class TheadsCountChangedEventArgs : EventArgs
    {
        public TheadsCountChangedEventArgs(int oldValue, int newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public int OldValue { get; }
        public int NewValue { get; }
    }
    /// <summary>
    /// License help methods
    /// </summary>
    public static class Licensing
    {
        internal static event EventHandler<TheadsCountChangedEventArgs> TheadsCountChanged;
        
        internal static JsonSerializerSettings LicenseKeySerializerSettings => new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver(),
            Formatting = Formatting.None
        };

        private const string LicensePublicKey =
            "<RSAKeyValue><Modulus>wg4zQAiXK7FefNGxEAfRIhZscy5tSKFPcWFAIwwjYQ7ZFDODFoUCQcIBu+p3DnZw7h3qLDmzlSW8K86bAQTyEXP1OSKrGxNf6Ai2p9++/W0WF7FazzLtk4fKnPUDjb0Q2LCI6W6BAFNm1zllVsE2VVSpn7W8nZDuffh2fLYoEVU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private static Dictionary<Type, object> _licenses = new Dictionary<Type, object>();

        private static DateTime _assemblyDate;

        static Licensing()
        {
            var freeWorkflowEngine = new LicenseKey<WorkflowEngineNetRestrictions>()
            {
                Restrictions = new WorkflowEngineNetRestrictions
                {
                    MaxNumberOfActivities = 15,
                    MaxNumberOfTransitions = 25,
                    MaxNumberOfThreads = 2,
                    MaxNumberOfSchemes = 1,
                    MaxNumberOfCommands = 5
                }
            };

            _licenses.Add(typeof(WorkflowEngineNetRestrictions), freeWorkflowEngine);

            _assemblyDate = RetrieveLinkerTimestamp();
        }

        public static LicenseKey<T> GetLicense<T>() where T : BaseRestrictions
        {
            var licenseKey = _licenses[typeof(T)] as LicenseKey<T>;
            if (licenseKey != null)
                return licenseKey.Clone();
            throw new LicenseException(string.Format("License of type {0} not found", typeof(T).Name));
        }

        public static T GetLicenseRestrictions<T>() where T : BaseRestrictions
        {
            var licenseKey = _licenses[typeof (T)] as LicenseKey<T>;
            if (licenseKey != null)
                return (T) licenseKey.Restrictions.Clone();
            throw new LicenseException($"License of type {typeof(T).Name} not found");
        }

        internal static void RegisteWorkflowLicense (string licenseText)
        {
            int oldMaxNumberOfThreads
                = GetLicenseRestrictions<WorkflowEngineNetRestrictions>().MaxNumberOfThreads;
            RegisterLicense<WorkflowEngineNetRestrictions>(licenseText);
            int newMaxNumberOfThreads
                = GetLicenseRestrictions<WorkflowEngineNetRestrictions>().MaxNumberOfThreads;

            if (newMaxNumberOfThreads != oldMaxNumberOfThreads)
            {
                TheadsCountChanged?.Invoke(null,new TheadsCountChangedEventArgs(oldMaxNumberOfThreads,newMaxNumberOfThreads));
            }
        }
        

        internal static void RegisterLicense<T>(string licenseText) where T : BaseRestrictions
        {
            LicenseKey<T> license;

            var result = VerifyLicenseKeyText(licenseText, out license);

            if (!result)
            {
                throw new LicenseException("Incorrect license");
            }

            if (!_licenses.ContainsKey(typeof (T)))
            {
                _licenses.Add(typeof(T),license);
            }
            else
            {
                _licenses[typeof (T)] = license;
            }
        }


        public static bool VerifyLicenseKeyText<T>(this string licenseKeyText, out LicenseKey<T> key) where T : BaseRestrictions
        {
            var rsa = new RSACryptoServiceProvider();
#if NETCOREAPP
            RSACryptoServiceProviderExtensions.FromXmlString(rsa,LicensePublicKey);
#else
            rsa.FromXmlString(LicensePublicKey);
#endif
            var rsaParameter = rsa.ExportParameters(false);
            key = ToLicenseKey<T>(licenseKeyText);
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(GetHashKeyToSign(key));
            byte[] numArray = Convert.FromBase64String(key.Hash);
            return VerifySignedHash(utf8Bytes, numArray, rsaParameter);
        }

        public static string GetHashKeyToSign<T>(LicenseKey<T> key) where T : BaseRestrictions
        {
#if NETCOREAPP
            var expire = key.Expiry.ToString("MM.dd.yyyy");
#else
            IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
            var expire = key.Expiry.ToString("MM.dd.yyyy", mmddFormat);
#endif
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", key.Ref, expire,
                JsonConvert.SerializeObject(key.Restrictions,LicenseKeySerializerSettings));
          
        }

        private static LicenseKey<T> ToLicenseKey<T>(string licenseKeyText) where T : BaseRestrictions
        {
            licenseKeyText = Regex.Replace(licenseKeyText, "\\s+", "");
            string[] strArrays = SplitOnFirst(licenseKeyText, '-');
            string refStr = strArrays[0];
            string licenseStr = strArrays[1];
            var licenseKey = LicenseKey<T>.LoadFromString(licenseStr);
            if (licenseKey.Ref != refStr)
            {
                throw new LicenseException($"The license '{licenseKey.Ref}' is not assigned to CustomerId '{refStr}'.");
            }
            if (licenseKey.Expiry < _assemblyDate.Date)
            {
                throw new LicenseException($"The license '{licenseKey.Ref}' is expired.");
            }
            return licenseKey;
        }

        private static string[] SplitOnFirst(string strVal, char needle)
        {
            if (strVal == null)
            {
                return new string[0];
            }
            var num = strVal.IndexOf(needle);
            if (num == -1)
            {
                return new[] {strVal};
            }
            var strArrays = new[] {strVal.Substring(0, num), strVal.Substring(num + 1)};
            return strArrays;
        }

        private static bool VerifySignedHash(byte[] dataToVerify, byte[] signedData, RSAParameters key)
        {
            try
            {
                var rsaCryptoServiceProvider = new RSACryptoServiceProvider();
                rsaCryptoServiceProvider.ImportParameters(key);
#if NETCOREAPP
                return rsaCryptoServiceProvider.VerifyData(dataToVerify, SHA1.Create(), signedData);
#else
                return rsaCryptoServiceProvider.VerifyData(dataToVerify, new SHA1CryptoServiceProvider(), signedData);
#endif
            }
            catch (CryptographicException)
            {
                return false;
            }
         }

        public static string LicenseKeyToString<T>(LicenseKey<T> key) where T : BaseRestrictions
        {
#if NETCOREAPP
            var expire = key.Expiry.ToString("MM.dd.yyyy");
#else
            IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
            var expire = key.Expiry.ToString("MM.dd.yyyy", mmddFormat);
#endif

            var licenseString = string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}:{3}", key.Ref,
                expire,
                Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(key.Restrictions, LicenseKeySerializerSettings))),
                key.Hash);

            return $"{key.Ref}-{Convert.ToBase64String(Encoding.UTF8.GetBytes(licenseString))}";

        }

        private static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = typeof(Licensing).GetTypeInfo().Assembly.Location; //string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int cPeHeaderOffset = 60;
            const int cLinkerTimestampOffset = 8;
            var b = new byte[2048];
           
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                stream.Read(b, 0, 2048);
            }

            int i = BitConverter.ToInt32(b, cPeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + cLinkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.ToLocalTime();
            return dt;
        }

    }
}
