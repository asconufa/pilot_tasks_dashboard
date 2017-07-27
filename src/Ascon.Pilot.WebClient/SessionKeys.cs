namespace Ascon.Pilot.WebClient
{
    /// <summary>
    /// Особые значения состояния сессии
    /// </summary>
    public static class SessionKeys
    {
        /// <summary>
        /// Id клиента
        /// </summary>
        public static readonly string ClientId = "ClientId";
        /// <summary>
        /// Количество визитов
        /// </summary>
        public static readonly string VisitsCount = "VisitsCount";
        /// <summary>
        /// Вид
        /// </summary>
        public static readonly string MetaTypes = "MetaTypes";
        /// <summary>
        /// Имя базы данных
        /// </summary>
        public static readonly string DatabaseName = "DbName";
        /// <summary>
        /// Имя пользвоателя
        /// </summary>
        public static readonly string Login = "Login";
        /// <summary>
        /// Защищёный пароль
        /// </summary>
        public static readonly string ProtectedPassword = "ProtectedPassword";
        /// <summary>
        /// Скрытая боковая панель
        /// </summary>
        public static readonly string IsSidePanelHidden = "IsSidePanelHidden";
        /// <summary>
        /// Фанель типов файлов
        /// </summary>
        public static readonly string FilesPanelType = "FilesPanelType";
    }
}