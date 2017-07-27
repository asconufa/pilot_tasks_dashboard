using Ascon.Pilot.Core;

namespace Ascon.Pilot.Server.Api.Contracts
{
    /// <summary>
    /// Интерфейс обратного вызова
    /// </summary>
    public interface IServerCallback
    {
        /// <summary>
        /// функция обратного вызова изменения объектов
        /// </summary>
        /// <param name="changeset">изменения над объектами</param>
        void NotifyChangeset(DChangeset changeset);

        /// <summary>
        /// функция обратного вызова изменения организационных единиц
        /// </summary>
        /// <param name="changeset">изменения над орг. единицами</param>
        void NotifyOrganisationUnitChangeset(OrganisationUnitChangeset changeset);

        /// <summary>
        /// функция обратного вызова изменения пользователей
        /// </summary>
        /// <param name="changeset">изменения над пользователями</param>
        void NotifyPersonChangeset(PersonChangeset changeset);

        /// <summary>
        /// функция обратного вызова изменения метаданных
        /// </summary>
        /// <param name="changeset">изменения метаданных</param>
        void NotifyDMetadataChangeset(DMetadataChangeset changeset);

        /// <summary>
        /// функция обратного вызова поискового запроса
        /// </summary>
        /// <param name="searchResult">результаты поиска</param>
        void NotifySearchResult(DSearchResult searchResult);

        /// <summary>
        /// функция обратного вызова поискового запроса по геометрии
        /// </summary>
        /// <param name="searchResult">результаты поиска</param>
        void NotifyGeometrySearchResult(DGeometrySearchResult searchResult);

        /// <summary>
        /// функция обратного вызова нотификации об изменениях объекта
        /// </summary>
        /// <param name="changeset">Changeset со списоком сообщений</param>
        void NotifyDNotificationChangeset(DNotificationChangeset changeset);

    }
}
