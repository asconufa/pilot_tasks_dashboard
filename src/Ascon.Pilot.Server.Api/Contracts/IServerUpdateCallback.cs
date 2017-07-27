using System;

namespace Ascon.Pilot.Server.Api.Contracts
{
    public interface IServerUpdateCallback
    {
        /// <summary>
        /// функция обратного вызова ошибка загрузки пакета на сервере
        /// </summary>
        /// <param name="error">сообщение об ошибке</param>
        void NotifyUploadToServerError(string error);

        /// <summary>
        /// функция обратного вызова ошибка выгрузки пакета с сервера
        /// </summary>
        /// <param name="messageError">сообщение об ошибке</param>
        void NotifyDownloadFromServerError(string messageError);
    }
}
