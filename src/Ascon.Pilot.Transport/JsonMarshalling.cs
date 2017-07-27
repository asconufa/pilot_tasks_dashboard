using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ascon.Pilot.Transport
{
    class JsonUnmarshaller : ICallService
    {
        private readonly IImplementationFactory _factory;
        private readonly ConcurrentDictionary<string, Object> _registered = new ConcurrentDictionary<string, Object>();

        public JsonUnmarshaller(IImplementationFactory factory)
        {
            this._factory = factory;
        }

        public byte[] Call(IInvocation invocation)
        {
            var obj = GetImplementation(invocation.Method.DeclaringType.Name);
            var method = obj.GetType().GetMethod(invocation.Method.Name);
            var parameters = method.GetParameters();

            var values = invocation.Arguments;
            for (var i = 0; i < invocation.Arguments.Length; i++)
            {
                var argumentObject = invocation.Arguments[i];
                if (parameters[i].Name == "protectedPassword")
                    values[i] = Convert.ChangeType(argumentObject.ToString().EncryptAes(), parameters[i].ParameterType);
            }

            try
            {
                var result = method.Invoke(obj, values);
                //TODO пока не знаю что делать. Это костыль!!!
                if (method.ReturnType.FullName == "System.Byte[]")
                {
                    return result as byte[];
                }
                return ResultToBytes(result);
            }
            catch (Exception e)
            {
                if (e is TargetInvocationException && e.InnerException != null)
                    return Encoding.UTF8.GetBytes(e.InnerException.Message);

                return Encoding.UTF8.GetBytes(e.Message);
            }
        }

        public byte[] Call(byte[] data)
        {
            var str = Encoding.Unicode.GetString(data, 0, data.Length);
            var jObject = (JObject) JsonConvert.DeserializeObject(str);
            
            var msg = new MarshallingMessage
            {
                Interface = jObject.SelectToken("api").ToString(),
                Method = jObject.SelectToken("method").ToString(),
                ParamCount = jObject.Count - 2,
            };

            var obj = GetImplementation(msg.Interface);
            var method = obj.GetType().GetMethod(msg.Method);
            var parameters = method.GetParameters();
            
            var values = new object[msg.ParamCount];
            for (var i = 0; i < msg.ParamCount; i++)
            {
                var p = jObject.SelectToken(parameters[i].Name);
                var val = p.ToObject(parameters[i].ParameterType);
                //TODO пока не знаю что делать с паролями. Это костыль!!!
                if (parameters[i].Name == "protectedPassword")
                {
                    val = Convert.ChangeType(p.ToString().EncryptAes(), parameters[i].ParameterType);
                }
                values[i] = val;
            }

            try
            {
                var result = method.Invoke(obj, values);
                //TODO пока не знаю что делать. Это костыль!!!
                if (method.ReturnType.FullName == "System.Byte[]")
                {
                    return result as byte[];
                }
                return ResultToBytes(result);
            }
            catch (Exception e)
            {
                if (e is TargetInvocationException && e.InnerException != null)
                    return Encoding.UTF8.GetBytes(e.InnerException.Message);

                return Encoding.UTF8.GetBytes(e.Message);
            }
        }

        private byte[] ResultToBytes(object result)
        {
            if (result == null)
                return new byte[0];
            
            var sz = JsonConvert.SerializeObject(result);
            return Encoding.UTF8.GetBytes(sz);
        }

        private Object GetImplementation(string interfaceName)
        {
            return _registered.GetOrAdd(interfaceName, (iName) => _factory.GetImplementation(iName));
        }
    }

    static class AesEx
    {
        private static readonly byte[] Password = Encoding.Unicode.GetBytes("123456");
        private static readonly byte[] Salt = Encoding.Unicode.GetBytes("{5AF2954F-02FC-445A-BC27-976DD11A6258}");
        const int ITERATIONS = 1000;

        public static string EncryptAes(this string plainText)
        {
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                using (DeriveBytes rgb = new Rfc2898DeriveBytes(Password, Salt, ITERATIONS))
                {
                    aesAlg.Key = rgb.GetBytes(aesAlg.KeySize >> 3);
                    aesAlg.IV = rgb.GetBytes(aesAlg.BlockSize >> 3);

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }
    }

    internal static class Extention
    {
        internal static string Parse(this NameValueCollection collection)
        {
            StringBuilder result = new StringBuilder("{");
            foreach (var key in collection.AllKeys)
            {
                result.AppendFormat("\"{0}\":\"{1}\",", key, collection[key]);
            }
            result.Remove(result.Length - 1, 1);
            result.Append("}");
            return result.ToString();
        }
    }
}
