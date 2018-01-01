using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Vulcain.Core
{
    public static class JSObject
    {
        public static Dictionary<string, object> PropertiesOf(object obj)
        {
            return ((object)obj)
                .GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(obj));
        }

        public static void Set(object target, string key, object val)
        {
            var p = target.GetType().GetProperty(key);
            p.SetValue(target, val);
        }

        public static object Get(object source, string key)
        {
            var p = source.GetType().GetProperty(key);
            return p.GetValue(source);
        }
    }

    public class JSonParser
    {
        struct Token
        {
            public bool EOF;
            public JsonToken Type;
            public object Value;
            public Type ValueType;
        }

        private JsonTextReader reader;

        public JSonParser(string json)
        {
            reader = new JsonTextReader(new StringReader(json));
            reader.Read();
        }

        public Dictionary<string, object> Parse()
        {
            var root = new Dictionary<string, object>();
            ParseObject(root);
            return root;
        }

        private void ParseObject(Dictionary<string, object> root)
        {
            var tkn = ReadAssert(JsonToken.StartObject);

            while (!tkn.EOF && tkn.Type != JsonToken.EndObject)
            {
                var propertyName = tkn.Value.ToString();
                tkn = ReadAssert(JsonToken.PropertyName);
                object value = null;
                switch (tkn.Type)
                {
                    case JsonToken.StartObject:
                        var obj = new Dictionary<string, object>();
                        ParseObject(obj);
                        value = obj;
                        break;
                    case JsonToken.StartArray:
                        var list = new List<object>();
                        tkn = ReadAssert(JsonToken.StartArray);
                        while (!tkn.EOF && tkn.Type != JsonToken.EndArray)
                        {
                            list.Add(GetValueOf(tkn));
                            tkn = ReadNext();
                        }
                        value = list;
                        break;
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.String:
                    case JsonToken.Boolean:
                        value = GetValueOf(tkn);
                        break;
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                        break;
                    case JsonToken.Date:
                        throw new NotSupportedException("Date type is not supported");
                    case JsonToken.Bytes:
                        throw new NotSupportedException("Bytes type is not supported");
                    default:
                        throw new Exception("Json malformed");
                }
                root[propertyName] = value;
            }
            ReadAssert(JsonToken.EndObject);
        }

        private object GetValueOf(Token tkn)
        {
            return Convert.ChangeType(tkn.Value, tkn.ValueType);
        }

        private Token ReadAssert(JsonToken t)
        {
            if (reader.TokenType != t)
                throw new Exception("Malformed json");
            return ReadNext();
        }

        private Token ReadNext()
        {
            if (!reader.Read())
                return new Token { EOF = true, Type = JsonToken.Undefined };
            return new Token { EOF = false, Type = reader.TokenType, Value = reader.Value, ValueType = reader.ValueType };
        }
    }

    public static class JSON
    {
        class JSonParser
        {
            private JsonTextReader reader;
            private bool EOF = false;

            public object Parse(string json)
            {
                if (String.IsNullOrWhiteSpace(json)) return null;

                reader = new JsonTextReader(new StringReader(json));
                try
                {
                    ReadNext();
                }
                catch
                {
                    return json;
                }
                var root = ParseValue();
                return root;
            }

            private void ParseObject(Dictionary<string, object> root)
            {
                ReadAssert(JsonToken.StartObject);

                while (!EOF && reader.TokenType != JsonToken.EndObject)
                {
                    var propertyName = reader.Value.ToString();
                    ReadAssert(JsonToken.PropertyName);
                    var value = ParseValue();
                    root[propertyName] = value;
                }
                ReadAssert(JsonToken.EndObject);
            }

            public object ParseValue()
            {
                object value = null;
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        var obj = new Dictionary<string, object>();
                        ParseObject(obj);
                        value = obj;
                        break;
                    case JsonToken.StartArray:
                        var list = new List<object>();
                        ReadAssert(JsonToken.StartArray);
                        while (!EOF && reader.TokenType != JsonToken.EndArray)
                        {
                            list.Add(ParseValue());
                        }
                        ReadAssert(JsonToken.EndArray);
                        value = list;
                        break;
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.String:
                    case JsonToken.Boolean:
                        value = GetValueOf();
                        ReadNext();
                        break;
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                        ReadNext();
                        break;
                    case JsonToken.Date:
                        throw new NotSupportedException("Date type is not supported");
                    case JsonToken.Bytes:
                        throw new NotSupportedException("Bytes type is not supported");
                    default:
                        throw new Exception("Json malformed");
                }
                return value;
            }

            private object GetValueOf()
            {
                return Convert.ChangeType(reader.Value, reader.ValueType);
            }

            private void ReadAssert(JsonToken t)
            {
                if (reader.TokenType != t)
                    throw new Exception("Malformed json");
                ReadNext();
            }

            private void ReadNext()
            {
                do
                {
                    if (!reader.Read())
                    {
                        EOF = true;
                        return;
                    }
                }
                while (reader.TokenType == JsonToken.Comment);
            }
        }

        public static object Parse(string json)
        {
            if (String.IsNullOrWhiteSpace(json))
                return null;

            var p = new JSonParser();
            return p.Parse(json);
        }

        public static T DeserializeObject<T>(string str)
        {
            if (String.IsNullOrWhiteSpace(str))
                return default(T);

            try
            {

                return JsonConvert.DeserializeObject<T>(str);
            }
            catch
            {
                return default(T);
            }
        }

        public static string Stringify<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }

    public static class StringEx
    {
        public static bool Match(this string str, string pattern)
        {
            var regex = new Regex(pattern);
            return regex.IsMatch(str);
        }
    }
}
