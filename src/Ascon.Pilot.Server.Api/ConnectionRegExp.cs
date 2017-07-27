using System.Collections.Generic;

namespace Ascon.Pilot.Server.Api
{
    public static class ConnectionRegExp
    {
        //Строки вида :8080 (от 1 до 5 символов) в конце строки!
        public static string Port = "(:([0-9]{1,5}))$";
        public static string HttpPrefix = @"http://";
        public static string TcpPreffix = @"net.tcp://";

        //Порт в строке встречается 0 или 1 раз
        private static string _port = "(:([0-9]{1,5}))?";
        private static string _ip = @"([01]?\d\d?|2[0-4]\d|25[0-5])\." +
                                    @"([01]?\d\d?|2[0-4]\d|25[0-5])\." +
                                    @"([01]?\d\d?|2[0-4]\d|25[0-5])\." +
                                    @"([01]?\d\d?|2[0-4]\d|25[0-5])";

        //Строки вида http://10.1.5.256:80/some_string
        public static string HttpBindingIP = HttpPrefix + _ip + _port + @"/([0-9a-zA-Z]{1,})";

        //строки вида http://url:8080/some_string
        public static string HttpBindingName = HttpPrefix + @"([a-zA-Z0-9\-\._]+)" + _port + @"/(\w+)";

        //строки вида 10.1.5.6:8080/some_string
        public static string NetBindingIPNoPerfix = @"^" + _ip + _port + @"/([0-9a-zA-Z]{1,})";

        //строки вида some_server:8080/some_string
        public static string NetBindingNameNoPerfix = @"^([a-zA-Z0-9\-\._]+)(:([0-9]{1,5}))?/(\w+)";

        //строки типа net.tcp://10.1.2.6:8080/some_string
        public static string NetBindingIP = TcpPreffix + _ip + _port + @"/([0-9a-zA-Z]{1,})";

        //строки вида net.tcp://some_server:8888/some_string
        public static string NetBindingName = TcpPreffix + @"([a-zA-Z0-9\-\._]+)(:([0-9]{1,5}))?/(\w+)";
  
        //
        public static List<string> ServerRegExpressions = new List<string>
                                                              {
                                                       HttpBindingIP,
                                                       HttpBindingName,
                                                       NetBindingIP,
                                                       NetBindingName,
                                                       NetBindingIPNoPerfix,
                                                       NetBindingNameNoPerfix
                                                   };
        
        //WTF?
        public static List<string> LoginRegExpressions = new List<string>
                                                             {
                                                            @"(\w+)(/(\w+))?"
                                                        };
    }
}
