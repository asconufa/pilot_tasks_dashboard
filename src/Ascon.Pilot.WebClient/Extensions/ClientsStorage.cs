using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Ascon.Pilot.Core;
using Ascon.Pilot.Server.Api;
using Ascon.Pilot.Server.Api.Contracts;
using Ascon.Pilot.Transport;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

using ISession = Microsoft.AspNetCore.Http.ISession;

namespace Ascon.Pilot.WebClient.Extensions
{
    //public static class SessionExtensions
    //{
    //    public static void Set<T>(this ISession session, string key, T value)
    //    {
    //        session.SetString(key, JsonConvert.SerializeObject(value));
    //    }

    //    public static T Get<T>(this ISession session, string key)
    //    {
    //        var value = session.GetString(key);
    //        return value == null ? default(T) :
    //                              JsonConvert.DeserializeObject<T>(value);
    //    }
    //}
    public static class SessionExt
    {
        public static IDictionary<int, MType> GetMetatypes(this ISession session)
        {
            return session.GetSessionValues<IDictionary<int, MType>>(SessionKeys.MetaTypes);
        }

        /// <summary>
        /// Set value of type <typeparam name="T">T</typeparam> at session dictionary using protobuf-net
        /// </summary>
        /// <typeparam name="T">type of value to set. Must be proto-serializable</typeparam>
        /// <param name="session">session to add values</param>
        /// <param name="key">key of value</param>
        /// <param name="value">value to set</param>
        public static void SetSessionValues<T>(this ISession session, string key, T value)
        {
            using (var bs = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(bs, value);
                session.Set(key, bs.ToArray());
            }
        }

        /// <summary>
        /// Deserialize values if type T from session dictionary using protobuf-net
        /// </summary>
        /// <typeparam name="T">type of value</typeparam>
        /// <param name="session">Session where values located</param>
        /// <param name="key">key of values in dictionary</param>
        /// <returns></returns>
        public static T GetSessionValues<T>(this ISession session, string key)
        {
            var val = session.Get(key);
            if (val == null)
                return default(T);
            using (var bs = new MemoryStream(val))
            {
                return ProtoBuf.Serializer.Deserialize<T>(bs);
            }
        }
    }

    public static class HttpContextClientsStorage
    {
        private static readonly Dictionary<Guid, HttpPilotClient>  ClientsDictionary = new Dictionary<Guid, HttpPilotClient>();

        public static HttpPilotClient GetClient(this HttpContext context)
        {
            var clientIdString = context.User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(clientIdString))
                return null;

            var clientId = Guid.Parse(clientIdString);
            if (ClientsDictionary.ContainsKey(clientId))
            {
                var client = ClientsDictionary[clientId];
                if (client.IsClientActive())
                    return client;
            }

            return null;
        }

        public static void SetClient(this HttpContext context, HttpPilotClient client, Guid clientId)
        {
            ClientsDictionary[clientId] = client;
        }

        public static IServerApi GetServerApi(this HttpContext context, IServerCallback callback = null)
        {
            if (callback == null)
                callback = CallbackFactory.Get<IServerCallback>();
            var client = context.GetClient();
            if (client == null)
                client = Reconnect(context, callback);
            return client.GetServerApi(callback);
        }

        private static HttpPilotClient Reconnect(HttpContext context, IServerCallback callback)
        {
            var client = new HttpPilotClient();
            client.Connect(ApplicationConst.PilotServerUrl);

            var serverApi = client.GetServerApi(callback);
            
            var dbName = context.User.FindFirstValue(ClaimTypes.Surname);
            var login = context.User.FindFirstValue(ClaimTypes.Name);
            var protectedPassword = context.User.FindFirstValue(ClaimTypes.UserData);
            var useWindowsAuth = login.Contains("/") || login.Contains("\\");
            var dbInfo = serverApi.OpenDatabase(dbName, login, protectedPassword, useWindowsAuth);
            if (dbInfo == null)
                throw new TransportException();

            var clientIdString = context.User.FindFirstValue(ClaimTypes.Sid);
            ClientsDictionary[Guid.Parse(clientIdString)] = client;

            DMetadata dMetadata = serverApi.GetMetadata(dbInfo.MetadataVersion);
            context.Session.SetSessionValues(SessionKeys.MetaTypes, dMetadata.Types.ToDictionary(x => x.Id, y => y));

            return client;
        }
    }
}
