using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OptimaJet.Workflow.Core.Designer
{
    public class ParsedType
    {
        public string Name;
        public bool IsGeneric;
        public List<ArrayDimension> ArrayDimensions;
        public List<ParsedType> TypeArguments;
        public bool IsNullable;

        public class ArrayDimension
        {
            public int Dimensions;

            public ArrayDimension()
            {
                Dimensions = 1;
            }
        }

        public ParsedType()
        {
            Name = null;
            IsGeneric = false;
            ArrayDimensions = new List<ArrayDimension>();
            TypeArguments = new List<ParsedType>();
        }

        public Type ConvertToType()
        {
            return ConvertToType(this);
        }

        private Type ConvertToType(ParsedType parsedType)
        {
            var name = parsedType.Name;

            if (parsedType.IsGeneric)
            {
                var type = Type.GetType(string.Format("{0}`{1}", name, parsedType.TypeArguments.Count));
                type = type.MakeGenericType(parsedType.TypeArguments.Select(ConvertToType).ToArray());
                return type;
            }

            if (parsedType.ArrayDimensions.Any())
            {
                name = parsedType.ArrayDimensions.Aggregate(name, (current, dimension) => string.Format("{0}[{1}]", current, new String(',', dimension.Dimensions - 1)));
            }
            var res = Type.GetType(name) ?? Type.GetType(string.Format("System.{0}", name)) ?? SearchInLoadedAssemblies(name);

            if (res != null && parsedType.IsNullable)
            {
                if (res.GetTypeInfo().IsValueType)
                    return typeof(Nullable<>).MakeGenericType(res);
            }

            return res;
        }

        private Type SearchInLoadedAssemblies(string name)
        {
#if NETCOREAPP
    #if NETCORE2
             var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    #else
            var assemblies = AppDomainNetCore.CurrentDomain.GetAssemblies();
    #endif
#else
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif
            return assemblies.Select(assembly => assembly.GetType(name, false)).FirstOrDefault(type => type != null);
        }

        public static string GetFriendlyName(Type type)
        {
            string friendlyName = type.FullName;
            if (type.GetTypeInfo().IsValueType && type.IsNullable())
            {
                friendlyName = $"{GetFriendlyName(type.GetUnderlyingType())}?";
            }
            else if (type.GetTypeInfo().IsPrimitive || type == typeof(Decimal) || type == typeof(String) || type == typeof(DateTime) ||
                type == typeof(Guid))
            {
                friendlyName = type.Name;

            }
            else if (type.GetTypeInfo().IsGenericType)
            {
                int backtick = friendlyName.IndexOf('`');
                if (backtick > 0)
                {
                    friendlyName = friendlyName.Remove(backtick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; i++)
                {
                    string typeParamName = GetFriendlyName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }
            else if (type.IsArray)
            {

                friendlyName = GetFriendlyName(type.GetElementType()) +
                               $"[{new String(',', type.GetArrayRank() - 1)}]";
            }

            return friendlyName;
        }


        public static ParsedType Parse(string name)
        {
            int pos = 0;
            bool dummy;
            return Parse(name, ref pos, out dummy);
        }

        private static ParsedType Parse(string name, ref int pos, out bool listTerminated)
        {
            var sb = new StringBuilder();
            var tn = new ParsedType();
            listTerminated = true;
            while (pos < name.Length)
            {
                char c = name[pos++];
                switch (c)
                {
                    case ',':
                        if (tn.Name == null)
                            tn.Name = sb.ToString();
                        listTerminated = false;
                        return tn;
                    case '>':
                        if (tn.Name == null)
                            tn.Name = sb.ToString();
                        return tn;
                    case '?':
                        tn.IsNullable = true;
                        if (tn.Name == null)
                            tn.Name = sb.ToString();
                        listTerminated = false;
                        break;
                    case '<':
                        {
                            tn.Name = sb.ToString();
                            tn.IsGeneric = true;
                            sb.Length = 0;
                            bool terminated = false;
                            while (!terminated)
                                tn.TypeArguments.Add(Parse(name, ref pos, out terminated));
                            var t = name[pos - 1];
                            if (t == '>')
                                continue;
                            throw new Exception("Missing closing > of generic type list.");
                        }
                    case '[':
                        ArrayDimension d = new ArrayDimension();
                        tn.ArrayDimensions.Add(d);
                        ParseArray(name, ref pos, d);
                        break;
                    default:
                        sb.Append(c);
                        continue;
                }
            }
            if (tn.Name == null)
                tn.Name = sb.ToString();
            return tn;
        }

        private static void ParseArray(string name, ref int pos, ArrayDimension d)
        {
            while (pos < name.Length)
            {
                char nextChar = name[pos++];
                switch (nextChar)
                {
                    case ']':
                        return; //array specifier terminated
                    case ',': //multidimensional array
                        d.Dimensions++;
                        break;
                    default:
                        throw new Exception(@"Expecting ""]"" or "","" after ""["" for array specifier but encountered """ +
                                            nextChar + @""".");
                }
            }
        }
    }
}
