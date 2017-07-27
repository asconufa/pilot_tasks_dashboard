using System;
using System.Collections.Generic;
using Ascon.Pilot.Core;

namespace Ascon.Pilot.Server.Api.Contracts
{
    public interface IServerApi
    {
        /// <summary>
        /// открыть базу данных
        /// </summary>
        /// <param name="database">имя базы данных</param>
        /// <param name="login">логин пользователя</param>
        /// <param name="protectedPassword">пароль</param>
        /// <param name="useWindowsAuth">использовать windows авторизацию</param>
        /// <param name="licenseType">тип забираемой лицензии</param>
        /// <returns>описание базы данных</returns>
        DDatabaseInfo OpenDatabase(string database, string login, string protectedPassword, bool useWindowsAuth, int licenseType = 100);

        /// <summary>
        /// Получить описание базы данных
        /// </summary>
        /// <param name="database">имя базы данных</param>
        /// <returns>описание базы данных</returns>
        DDatabaseInfo GetDatabase(string database);

        /// <summary>
        /// Получить текущие метаданные
        /// </summary>
        /// <param name="localVersion">версия метаданных</param>
        /// <returns>метаданные</returns>
        DMetadata GetMetadata(long localVersion);

        /// <summary>
        /// Получение значений настроек
        /// </summary>
        /// <param name="loadAll">Получить все настройки для всех пользователей. Требуются права администратора.</param>
        /// <returns>значения настроек</returns>
        DSettings GetSettings(bool loadAll);

        /// <summary>
        /// Изменение значений настроек. Для изменения значений обобщенных настроек требуются права администратора.
        /// </summary>
        /// <param name="change">изменение</param>
        void ChangeSettings(DSettingsChange change);

        /// <summary>
        /// Получить объекты
        /// </summary>
        /// <param name="ids">список идентификаторов</param>
        /// <returns>список объектов</returns>
        List<DObject> GetObjects(Guid[] ids);

        /// <summary>
        /// Получить изменения 
        /// </summary>
        /// <param name="first">с позиции</param>
        /// <param name="last">до позиции</param>
        /// <returns></returns>
        List<DChangeset> GetChangesets(long first, long last);

        /// <summary>
        /// Применить изменения объектов
        /// </summary>
        /// <param name="changes">изменения</param>
        /// <returns>слитые измения с др. клиентами</returns>
        DChangeset Change(DChangesetData changes);

        /// <summary>
        /// Получить часть тела файлыа
        /// </summary>
        /// <param name="id">идентификатор файла</param>
        /// <param name="pos">позиция</param>
        /// <param name="count">кол-во</param>
        /// <returns>часть тела файла</returns>
        byte[] GetFileChunk(Guid id, long pos, int count);

        /// <summary>
        /// Положить часть тела файла
        /// </summary>
        /// <param name="id">идентификатор файла</param>
        /// <param name="buffer">часть тела</param>
        /// <param name="pos">позиция</param>
        void PutFileChunk(Guid id, byte[] buffer, long pos);

        /// <summary>
        /// Создать описание файла в базе данных
        /// </summary>
        /// <param name="fileBody">описание файла</param>
        void CreateFileBody(DFileBody fileBody);

        /// <summary>
        /// Получить текущую позицию при считывании тела файла
        /// </summary>
        /// <param name="id">идентификатор файла</param>
        /// <returns>позиция</returns>
        long GetFilePosition(Guid id);
        
        /// <summary>
        /// Загрузить всех пользователей базы данных
        /// </summary>
        /// <returns>список пользователей</returns>
        List<DPerson> LoadPeople();

        /// <summary>
        /// Получить список организационных единиц
        /// </summary>
        /// <returns>список орг. единиц</returns>
        List<DOrganisationUnit> LoadOrganisationUnits();

        /// <summary>
        /// Добавить условие поиска
        /// </summary>
        /// <param name="searchDefinition">описание</param>
        void AddSearch(DSearchDefinition searchDefinition);

        /// <summary>
        /// Удалить условие поиска
        /// </summary>
        /// <param name="searchDefinitionId">идентификатор описания поиска</param>
        void RemoveSearch(Guid searchDefinitionId);

        /// <summary>
        /// Получить информацию о лицензии
        /// </summary>
        /// <returns>инфо</returns>
        byte[] GetLicenseInformation();

        /// <summary>
        /// Захватить лицензию с типом licenseType
        /// </summary>
        void ConsumeLicense(int licenseType);

        /// <summary>
        /// Отпустить лицензию с типом licenseType
        /// </summary>
        void ReleaseLicense(int licenseType);

        /// <summary>
        /// Запустить поиск по геометрии
        /// </summary>
        /// <param name="searchDefinition">условия поиска</param>
        void GeometrySearch(DGeometrySearchDefinition searchDefinition);

        /// <summary>
        /// Запустить поиск по файлам
        /// </summary>
        /// <param name="searchDefinition">условия поиска</param>
        void ContentSearch(DSearchDefinition searchDefinition);
    }

    public interface IFileArchiveApi
    {
        /// <summary>
        /// Получить часть тела файла из архива
        /// </summary>
        /// <param name="databaseName">имя базы данных</param>
        /// <param name="id">идентификатор файла</param>
        /// <param name="pos">позиция</param>
        /// <param name="count">кол-во</param>
        /// <returns>часть тела файла</returns>
        byte[] GetFileChunk(string databaseName, Guid id, long pos, int count);

        /// <summary>
        /// Положить часть тела файла
        /// </summary>
        /// <param name="databaseName">имя базы данных</param>
        /// <param name="id">идентификатор файла</param>
        /// <param name="buffer">часть тела</param>
        /// <param name="pos">позиция</param>
        void PutFileChunk(string databaseName, Guid id, byte[] buffer, long pos);

        /// <summary>
        /// Получить текущую позицию при считывании тела файла
        /// </summary>
        /// <param name="databaseName">имя базы данных</param>
        /// <param name="id">идентификатор файла</param>
        /// <returns>позиция</returns>
        long GetFilePosition(string databaseName, Guid id);

        /// <summary>
        /// Зафиксировать отправленные данные в файловом архиве. 
        /// При возникновении ошибки (например, несовпадении контрольной суммы или размера файла) данные во временном хранилище очищаются.
        /// </summary>
        /// <param name="databaseName">имя базы данных</param>
        /// <param name="fileBody">Описание файла</param>
        void PutFileInArchive(string databaseName, DFileBody fileBody);
    }
}
