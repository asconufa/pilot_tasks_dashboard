using Castle.DynamicProxy;

namespace Ascon.Pilot.WebClient.Extensions
{
    public static class CallbackFactory
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public static T Get<T>() where T : class
        {
            return ProxyGenerator.CreateInterfaceProxyWithoutTarget<T>();
        }
    }
}
