using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using ProtoBuf;

namespace Ascon.Pilot.Transport
{
    public interface ICallService
    {
        byte[] Call(byte[] data);
    }

    public interface IImplementationFactory
    {
        Object GetImplementation(string interfaceName);
    }

    public interface IGetService
    {
        T Get<T>() where T : class;
    }

    public class Marshaller : IGetService
    {
        private readonly ICallService _service;

        public Marshaller(ICallService service)
        {
            this._service = service;
        }

        public T Get<T>() where T : class
        {
            var generator = new ProxyGenerator();
            return generator.CreateInterfaceProxyWithoutTarget<T>(new Interceptor(_service));
        }
    }

    public class JsonMarshaller : IGetService
    {
        private readonly ICallService _service;

        public JsonMarshaller(ICallService service)
        {
            this._service = service;
        }

        public T Get<T>() where T : class
        {
            var generator = new ProxyGenerator();
            return generator.CreateInterfaceProxyWithoutTarget<T>(new JsonInterceptor(_service));
        }
    }

    [ProtoContract]
    public class MarshallingMessage
    {
        [ProtoMember(1)]
        public string Interface { get; set; }

        [ProtoMember(2)]
        public string Method { get; set; }

        [ProtoMember(3)]
        public int ParamCount { get; set; }

        [ProtoMember(4)]
        public List<int> NullParamIndices { get; private set; }

        public MarshallingMessage()
        {
            NullParamIndices = new List<int>();
        }
    }

    public class Unmarshaller : ICallService
    {
        private readonly IImplementationFactory _factory;
        private readonly ConcurrentDictionary<string, Object> _registered = new ConcurrentDictionary<string, Object>();

        public Unmarshaller(IImplementationFactory factory)
        {
            this._factory = factory;
        }

        public byte[] Call(byte[] data)
        {
            using (var mem = new MemoryStream(data))
            {
                var msg = ProtoSerializer.Deserialize<MarshallingMessage>(mem);
                var obj = GetImplementation(msg.Interface);
                var method = obj.GetType().GetMethod(msg.Method);
                var parameters = method.GetParameters();

                if (parameters.Count() != msg.ParamCount && parameters.Count(p => p.IsOptional == false) != msg.ParamCount)
                    throw new InvalidDataException(
                        $"Method {msg.Interface}.{msg.Method} received {msg.ParamCount} parameters, but {parameters.Length} required");
                var values = new object[parameters.Count()];
                for (var i = 0; i < parameters.Count(); i++)
                {
                    if(msg.NullParamIndices.Contains(i))
                        continue;
                    values[i] = ProtoSerializer.Deserialize(mem, parameters[i].ParameterType);
                    if (parameters[i].IsOptional && values[i] == null)
                        values[i] = Type.Missing;
                }

                try
                {
                    var result = method.Invoke(obj, values);
                    return ResultToBytes(result);
                }
                catch (Exception e)
                {
                    if (e is TargetInvocationException && e.InnerException != null)
                        throw e.InnerException;
                    throw;
                }
            }
        }

        private byte[] ResultToBytes(object result)
        {
            if (result == null)
                return new byte[0];
            using (var mem = new MemoryStream())
            {
                ProtoSerializer.Serialize(mem, result);
                return mem.ToArray();
            }
        }

        private object GetImplementation(string interfaceName)
        {
            return _registered.GetOrAdd(interfaceName, (iName) => _factory.GetImplementation(iName));
        }
    }

    public class JsonInterceptor : IInterceptor
    {
        private readonly ICallService _service;

        public JsonInterceptor(ICallService service)
        {
            _service = service;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            /*if (method.Name == "GetHashCode")
                return new ReturnMessage(this.GetHashCode(), null, 0, methodCall.LogicalCallContext, methodCall);
            if (method.Name == "Equals")
                return new ReturnMessage(this.Equals(methodCall.Args[0]), null, 0, methodCall.LogicalCallContext, methodCall);
            if (method.Name == "ToString")
                return new ReturnMessage(this.ToString(), null, 0, methodCall.LogicalCallContext, methodCall);*/

            try
            {
                var data = CallToData(method, invocation);
                var result = _service.Call(data);
                invocation.ReturnValue = DataToResult(result, method);
            }
            catch (Exception e)
            {
                invocation.ReturnValue = null;
            }
        }

        private byte[] CallToData(MethodInfo method, IInvocation invocation)
        {
            dynamic value = new ExpandoObject();
            value.api = method.DeclaringType.Name;
            value.method = method.Name;

            var parameters = invocation.Method.GetParameters();
            for (int i = 0; i < invocation.Arguments.Length; i++)
            {
                JsonConvert.PopulateObject(parameters[i].Name, value);
            }
                
            var res = JsonConvert.SerializeObject(value);
            return Encoding.UTF8.GetBytes(res);
        }

        private object DataToResult(byte[] data, MethodInfo method)
        {
            if (data.Length == 0)
                return Convert.ChangeType(null, method.ReturnType);

            var serializedResult = Encoding.UTF8.GetString(data);
            var deserializedObject = JsonConvert.DeserializeObject(serializedResult);
            return Convert.ChangeType(deserializedObject, method.ReturnType);
        }
    }

    public class Interceptor : IInterceptor
    {
        private readonly ICallService _service;

        public Interceptor(ICallService service)
        {
            _service = service;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            /*if (method.Name == "GetHashCode")
                return new ReturnMessage(this.GetHashCode(), null, 0, methodCall.LogicalCallContext, methodCall);
            if (method.Name == "Equals")
                return new ReturnMessage(this.Equals(methodCall.Args[0]), null, 0, methodCall.LogicalCallContext, methodCall);
            if (method.Name == "ToString")
                return new ReturnMessage(this.ToString(), null, 0, methodCall.LogicalCallContext, methodCall);*/

            try
            {
                var data = CallToData(method, invocation);
                var result = _service.Call(data);
                invocation.ReturnValue = DataToResult(result, method);
            }
            catch (Exception e)
            {
                invocation.ReturnValue = null;
            }
        }

        private byte[] CallToData(MethodInfo method, IInvocation invocation)
        {
            using (var mem = new MemoryStream())
            {
                var call = new MarshallingMessage
                {
                    Interface = method.DeclaringType.Name,
                    Method = method.Name,
                    ParamCount = invocation.Arguments.Length
                };
                for (int i = 0; i < invocation.Arguments.Length; i++)
                {
                    if (invocation.Arguments[i] == null)
                        call.NullParamIndices.Add(i);
                }
                ProtoSerializer.Serialize(mem, call);

                foreach (var arg in invocation.Arguments)
                    if (arg != null)
                        ProtoSerializer.Serialize(mem, arg);

                return mem.ToArray();
            }
        }

        private object DataToResult(byte[] data, MethodInfo method)
        {
            if (data.Length == 0)
                return Convert.ChangeType(null, method.ReturnType);
            using (var mem = new MemoryStream(data))
            {
                var result = ProtoSerializer.Deserialize(mem, method.ReturnType);
                return Convert.ChangeType(result, method.ReturnType);
            }
        }
    }

    static class ProtoSerializer
    {
        public static void Serialize(Stream stream, object instance)
        {
            ProtoBuf.Meta.RuntimeTypeModel.Default.SerializeWithLengthPrefix(stream, instance, instance.GetType(), PrefixStyle.Base128, 0);
        }

        public static object Deserialize(Stream stream, Type type)
        {
            return ProtoBuf.Meta.RuntimeTypeModel.Default.DeserializeWithLengthPrefix(stream, null, type, PrefixStyle.Base128, 0);
        }
        
        public static T Deserialize<T>(Stream stream)
        {
            return Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Base128, 0);
        }
    }
}
