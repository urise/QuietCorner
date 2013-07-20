namespace CommonClasses
{
    public class Messages
    {
        public const string SessionTimeOut = "Время рабочей сессии истекло.";
        public const string NotEnoughPermissions = "У Вас недостаточно прав для выполнения операции.";

        public const string LoginAlreadyUsed = "Такой логин уже зарегистрирован в системе";
        public const string EmailAlreadyUsed = "Такой электронный адрес уже зарегистрирован в системе";
        public const string ConfirmPasswordDoNotMatch = "Ошибка при подтверждении пароля";
        public const string EmailRequired = "Введите электронный адрес";
        public const string LoginRequired = "Введите логин";
        public const string PasswordRequired = "Введите пароль";
        public const string ConfirmPasswordRequired = "Введите пароль повторно";
        public const string WrondEmailFormat = "Электронный адрес имеет неверный формат";

        public const string WrongLoginOrPassword = "Пользователь с указанным логином или паролем не был найден";
        public const string UserEmailUnapproved = "Необходимо подтвердить электронный адрес прежде, чем начать использовать этот логин.";
        public const string UserInstanceDoesntMatch = "Этот пользователь не имеет прав на выбранную компанию.";
        public const string UserNotFoundByPassword = "Введен неверный пароль";
        public const string NewPasswordIsNotDifferentFromTheOld = "Новый пароль должен отличаться от старого";
        public const string WrongLogin = "Пользователь с указанным логином не был найден";
        public const string CantForgotPassword = "Ошибка восстановления пароля";
        public const string TemporaryCodeExpired = "Срок годности ссылки востановления пароля истек";
        public const string EmailSentPasswordReset = "Ссылка для восстановления пароля была отправлена на Ваш электронный адрес";
        public const string UserNotFoundByLogin = "Пользователь с таким логином не зарегистрирован в системе";
        public const string UserNotFoundByEmail = "Пользователь с таким электронным адресом не существует";

        public const string InstanceNotFound = "Компания не найдена.";

        public const string RegisterSuccess = "Регистрация прошла успешно. На ваш электронный адрес отослано письмо для подтверждения регистрации.";
        public const string ConfirmMailSuccess = "Регистрация подтверждена";
        public const string ConfirmMailError = "Ошибка подтверждения регистрации";
        public const string ChangePasswordSuccess = "Пароль успешно изменен";

        public const string ErrorCompanyCreation = "Ошибка при создании компании. Попробуйте повторить попытку или обратитесь к администратору сиситемы.";
        public const string ExistsCompanyName = "Такая компания уже есть в Вашем списке доступных компаний";
        public const string EmptyCompanyName = "Введите название компании";

        public const string UserInstanceNotFound = "Указанный пользователь не связан с данной компанией";
        public const string UserInstanceAlreadyExist = "Указанный пользователь уже связан с данной компанией";
        public const string UserRolesNotFound = "Ошибка сохранения. Роли для данного пользователя не найдены.";


        public const string RoleNameNotUnique = "Роль с таким именем уже существует. Введите уникальное имя для создаваемой роли";
        public const string RoleNotFound = "Роль не найдена.";
        public const string RoleDisabledToDelete = "Роль не может быть удалена.";
    }
}
