using Ascon.Pilot.Core;

namespace Ascon.Pilot.Server.Api.Contracts
{
    /// <summary>
    /// Интерфейс обратного вызова для админки
    /// </summary>
    public interface IServerAdminCallback
    {
        /// <summary>
        /// Функция обратного вызова изменения информации о базе данных
        /// </summary>
        /// <param name="changeset">изменение бд</param>
        void NotifyDatabaseChanged(DatabaseChangeset changeset);

        /// <summary>
        /// Функция обратного вызова изменения информации о пользователе данных
        /// </summary>
        /// <param name="changeset">изменение пользователя</param>
        void NotifyPersonChanged(PersonChangeset changeset);

        /// <summary>
        /// Функция обратного вызова изменения информации об оргюните данных
        /// </summary>
        /// <param name="changeset">изменение оргюнита</param>
        void NotifyOrganisationUnitChanged(OrganisationUnitChangeset changeset);

        /// <summary>
        /// Функция обратного вызова изменения информации о метаданных
        /// </summary>
        /// <param name="changeset">изменение метаданных</param>
        void NotifyDMetadataChanged(DMetadataChangeset changeset);

        /// <summary>
        /// Функция обратного вызова изменения информации о количестве подключений
        /// </summary>
        /// <param name="licenseType">тип лицензии</param>
        /// <param name="licenseCount">кол-во подключенных клиентов по лицензии licenseType</param>
        void NotifyConnectionCountFromServer(int licenseType, int licenseCount);
    }
}
