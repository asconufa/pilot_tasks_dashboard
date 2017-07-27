using System.ComponentModel.DataAnnotations;


namespace Ascon.Pilot.WebClient.ViewModels
{
    /// <summary>
    /// Модель пользователя    
    /// </summary>
    public class LogInViewModel
    {
        /// <summary>
        /// Название баззы данных .
        /// </summary>
        [Required]
        [Display(Name = "Название базы данных")]
        public string DatabaseName { get; set; }

        /// <summary>
        ///Имя пользвоателя для входа в систему.
        /// </summary>
        [Required]
        [Display(Name = "Имя пользователя")]
        public string Login { get; set; }

        /// <summary>
        /// Пароль для входа в систему.
        /// </summary>
        [Required]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
