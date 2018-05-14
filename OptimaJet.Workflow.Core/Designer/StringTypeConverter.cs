using System;
# if !NETCOREAPP
using System.Web;
#else
using System.Net;
#endif
using Newtonsoft.Json;

namespace OptimaJet.Workflow.Core.Designer
{
    public class StringTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var type = value as Type;
            if (type == null)
            {
                writer.WriteNull();
            }
            else
            {
#if !NETCOREAPP
                writer.WriteValue(HttpUtility.UrlEncode(ParsedType.GetFriendlyName(type)).Replace("+", "%20"));
#else
                writer.WriteValue(WebUtility.UrlEncode(ParsedType.GetFriendlyName(type)).Replace("+", "%20"));
#endif
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            try
            {
                var str = Decode(reader);
                var parsedType = ParsedType.Parse(str).ConvertToType();

                if (parsedType == null)
                    throw new JsonSerializationException($"Error converting value {str} to Type.");

                return parsedType;

            }
            catch (Exception ex)
            {

                var str = reader.Value;
                try
                {
                    str = Decode(reader);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }

                throw new JsonSerializationException($"Error converting value {str} to Type.", ex);
            }
        }

        private string Decode(JsonReader reader)
        {
            var str = reader.Value.ToString();
            if (WasEncoded(str))
#if !NETCOREAPP
                str = HttpUtility.UrlDecode(str);
#else
                str = WebUtility.UrlDecode(str);
#endif
            return str;
        }

        private bool WasEncoded(string value)
        {
#if !NETCOREAPP
            var newValue = HttpUtility.UrlDecode(value);
#else
            var newValue = WebUtility.UrlDecode(value);
#endif
            return value != newValue;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Type) == objectType;
        }
    }
}
