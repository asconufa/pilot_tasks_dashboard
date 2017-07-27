using System;

namespace Ascon.Pilot.Server.Api.Contracts
{
    public interface IServerUpdateApi
    {
        /// <summary>
        /// Загружает частями на сервер пакет обновления
        /// </summary>
        /// <param name="updateVersion">версия пакета обновления</param>
        /// <param name="buffer">данные</param>
        /// <param name="pos">позиция</param>
        void UploadUpdatePackageToServer(string updateVersion, byte[] buffer, long pos);

        /// <summary>
        /// Частями загружает c сервера пакет обновления
        /// </summary>
        /// <param name="pos">начиная с</param>
        /// <param name="count">сколько</param>
        /// <param name="updatePath">путь к файлу на сервере</param>
        byte[] UploadUpdatePackageToClient(long pos, int count, string updatePath);

        /// <summary>
        /// Возвращает размер пакета обновления
        /// </summary>
        long GetUpdatePackageSize(string updateFilePath);

        /// <summary>
        /// Начинает обновление сервера
        /// </summary>
        void StartUpdateServer();

        /// <summary>
        /// Возвращает текущую версию сервера
        /// </summary>
        string GetServerVersion();

        /// <summary>
        /// Возвращает имя файла обновления по тегу
        /// </summary>
        /// <param name="searchTag">префикс приложения</param>
        /// <param name="environment">x86 или x64</param>
        string GetPackagePathOnServer(string searchTag, string environment);
    }
}