using System;
using System.Collections.Generic;
using Ascon.Pilot.Core;

namespace Ascon.Pilot.Server.Api.Contracts
{
    /// <summary>
    /// АПИ для импорта
    /// </summary>
    public interface IImportApi
    {
        /// <summary>
        /// Авторизация администратора сервера
        /// </summary>
        /// <param name="login">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <param name="useWindowsAuth">Использовать ли win-авторизацию</param>
        void RootAuthorize(string login, string password, bool useWindowsAuth);

        /// <summary>
        /// Получить список баз данных зарегистрированных на сервере
        /// </summary>
        List<string> GetDatabasesNames();
        
        /// <summary>
        /// Создать базу данных и открыть ее для начала импорта
        /// </summary>
        /// <param name="databaseName">имя базы данных, отображаемое в клиенте</param>
        /// <param name="databaseDirectory">папка для файла бд</param>
        /// <param name="fileArchiveFolder">папка корня файлового архива</param>
        DDatabaseImportInfo StartImport(string databaseName, string databaseDirectory, string fileArchiveFolder);

        /// <summary>
        /// Завершить импорт данных
        /// Записать в настройки сервера
        /// </summary>
        void FinishImport(bool cancelled);

        /// <summary>
        /// Создать пользователя
        /// </summary>
        /// <param name="person">Пользователь</param>
        void UpsertPerson(DPerson person);

        /// <summary>
        /// Создать или обновить тип
        /// </summary>
        /// <param name="type">Тип</param>
        void UpsertType(MType type);

        /// <summary>
        /// Создать или обновить объект
        /// </summary>
        /// <param name="obj">объект</param>
        void UpsertObject(DObject obj);

        /// <summary>
        /// Кладет на сервер тело файла по чатям
        /// </summary>
        /// <param name="id">идентификатор файла</param>
        /// <param name="buffer">данные</param>
        /// <param name="pos">позиция</param>
        void UploadFileChunk(Guid id, byte[] buffer, long pos);
        
        /// <summary>
        /// Создать описание тела файла в базе
        /// </summary>
        /// <param name="fileBody">описание тела файла</param>
        void CreateFileBody(DFileBody fileBody);

        /// <summary>
        /// Создать департамент (в том числе и организацию)
        /// </summary>
        /// <param name="organisationUnit">департамент</param>
        void CreateOrganisationUnit(DOrganisationUnit organisationUnit);

        /// <summary>
        /// Получение списка вложенных элементов файловой системы
        /// </summary>
        /// <param name="parent">родительский элемент</param>
        /// <param name="fileExtensionFilter">расширение отображаемых файлов</param>
        /// <param name="showNetworkFolders">показывать или нет сетевые компьютеры</param>
        IEnumerable<FileSystemNode> GetFileSystemNodes(FileSystemNode parent, string fileExtensionFilter, bool showNetworkFolders);

        /// <summary>
        /// Переименовать папку
        /// </summary>
        void RenameFolder(string oldPath, string newPath);

        /// <summary>
        /// Удалить папку
        /// </summary>
        void DeleteFolder(string path);

        /// <summary>
        /// Создать папку
        /// </summary>
        void CreateFolder(string path);
    }
}
