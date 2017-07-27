using System;
using System.Collections.Generic;
using Ascon.Pilot.Core;

namespace Ascon.Pilot.Server.Api.Contracts
{
    /// <summary>
    /// АПИ для админки
    /// </summary>
    public interface IServerAdminApi
    {
        /// <summary>
        /// Авторизация администратора сервера
        /// </summary>
        /// <param name="login">Имя пользователя</param>
        /// <param name="protectedPassword">Пароль</param>
        /// <param name="useWindowsAuth">Win авторизация</param>
        void Authorize(string login, string protectedPassword, bool useWindowsAuth);

        /// <summary>
        /// Получить список баз данных зарегистрированных на сервере
        /// </summary>
        List<AdminDatabaseInfo> GetDatabaseInfoList();
        
        /// <summary>
        /// Создать новую базу данных на сервере
        /// </summary>
        /// <param name="databaseName">имя базы данных</param>
        /// <param name="databaseDirectory">полный путь к файлу бд</param>
        /// <param name="fileArchiveFolders">список полных путей к файловым архивам</param>
        void CreateDatabase(string databaseName, string databaseDirectory, IEnumerable<string> fileArchiveFolders);

        /// <summary>
        /// Подключить существующую базу к серверу
        /// </summary>
        void AddDatabase(string databaseName, string databaseFilename, IEnumerable<string> fileArchiveFolders);

        /// <summary>
        /// Удалить базу данных
        /// </summary>
        void DeleteDatabase(string databaseName);

        /// <summary>
        /// Отсоединить базу данных
        /// </summary>
        void DetachDatabase(string databaseName);

        /// <summary>
        /// Отсоединить базу данных и сбросить ИД базы
        /// </summary>
        void DetachAsNewDatabase(string databaseName);

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

        /// <summary>
        /// Установить статус базы данных
        /// </summary>
        void SetDatabaseState(string databaseName, int state);

        /// <summary>
        /// Открыть базу для начала редактирования
        /// </summary>
        void OpenDatabase(string name);

        /// <summary>
        /// Получить пользователей
        /// </summary>
        List<DPerson> LoadPeople();

        /// <summary>
        /// Импортировать пользователей с ad
        /// </summary>
        DPerson ImportPerson(string sid);

        /// <summary>
        /// Получить список ad
        /// </summary>
        IEnumerable<String> GetDomains();

        /// <summary>
        /// Получить список пользователей ad
        /// </summary>
        IEnumerable<AdUser> GetUsers(string domainName);

        /// <summary>
        /// Получить пользователя ad по sid
        /// </summary>
        AdUser GetUser(string sid);

        /// <summary>
        /// Синхронизировать пользователей с ad
        /// </summary>
        void SyncPeople();

        /// <summary>
        /// Обновить пользователя
        /// </summary>
        void UpdatePerson(DPerson person, string password = null);

        /// <summary>
        /// Создать пользователя
        /// </summary>
        DPerson CreatePerson(DPerson person, string encryptedPassword);

        /// <summary>
        /// Загрузить департаменты
        /// </summary>
        List<DOrganisationUnit> LoadOrganisationUnits();

        /// <summary>
        /// Обновить структуру подразделения
        /// </summary>
        DOrganisationUnitChangesetData ChangeOrganisationUnit(DOrganisationUnitChangesetData changes);

        /// <summary>
        /// Перемещение орг юнитов
        /// </summary>
        void MoveOrganisationUnits(IEnumerable<int> orgUnitsToMove, int newParent);

        /// <summary>
        /// Назначить пользователя на позицию
        /// </summary>
        IEnumerable<DPerson> SetPeopleOnPosition(DPersonOnPositionData data);
        
        /// <summary>
        /// Получить метаданные
        /// </summary>
        DMetadata GetMetadata(long localVersion);

        /// <summary>
        /// Обновить метаданные
        /// </summary>
        long UpdateMetadata(DMetadata metadata);

        /// <summary>
        /// Переименовать базу данных
        /// </summary>
        void RenameDatabase(string oldName, string newName);

        /// <summary>
        /// Получить список администраторов сервера
        /// </summary>
        IEnumerable<DServerAdministrator> GetServerAdministrators();

        /// <summary>
        /// Создать администратора сервера
        /// </summary>
        void CreateServerAdministrator(DServerAdministrator admin, string protectedPassword);

        /// <summary>
        /// Изменить администратора сервера
        /// </summary>
        void UpdateServerAdministrator(DServerAdministrator admin, string protectedPassword);

        /// <summary>
        /// Удалить администратора сервера
        /// </summary>
        void DeleteServerAdministrator(DServerAdministrator admin);

        /// <summary>
        /// Загружает частями на сервер файл лицензии
        /// </summary>
        string UploadLicenseToServer(byte[] buffer);

        /// <summary>
        /// Метод удаления лицензии
        /// </summary>
        string DeleteLicenseFromServer();

        /// <summary>
        /// Метод запроса информации о текущей лицензии на сервере
        /// </summary>
        byte[] GetLicenseInformation();

        /// <summary>
        /// Запрос к сервер на отсылку уведомлений о текущих подключениях
        /// </summary>
        void GetLicenseConnections();

        /// <summary>
        /// Проверить соединение с сервером геометрического поиска
        /// </summary>
        /// <returns>версия сервера поиска по геометрии</returns>
        string DraftSearchConnect();

        /// <summary>
        /// Обновить все результаты геометрического поиска
        /// </summary>
        void DraftSearchRebuildIndex();

        /// <summary>
        /// Проверить соединение с сервером текстового поиска
        /// </summary>
        /// <returns>версия сервера поиска по файлам</returns>
        string TextSearchConnect();

        /// <summary>
        /// Обновить все результаты текстового поиска
        /// </summary>
        void TextSearchRebuildIndex();

        /// <summary>
        /// Переиндексировать базу данных
        /// </summary>
        void AttrSearchRebuildIndex();

        /// <summary>
        /// Получить список всех счётчиков нумератора
        /// </summary>
        /// <returns>список</returns>
        IEnumerable<DCounter> GetCounters();

        /// <summary>
        /// Обновить информацию о счётчике нумератора
        /// </summary>
        /// <param name="counter">новый счётчик нумератора</param>
        void UpdateCounter(DCounter counter);

        /// <summary>
        /// Удалить счётчик нумератора
        /// </summary>
        /// <param name="counterName">имя счетчика</param>
        void DeleteCounter(string counterName);
    }
}
