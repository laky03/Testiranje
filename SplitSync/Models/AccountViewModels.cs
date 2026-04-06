namespace SplitSync.Models
{
    public class AccountLoginViewModel
    {
        public string Identifier { get; set; } = ""; // username ili email
        public string Password { get; set; } = "";
        public string? ErrorMessage { get; set; }
    }

    public class AccountRegisterViewModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public string? ErrorMessage { get; set; }
        public string? InfoMessage { get; set; }
    }

    public class AccountVerifyViewModel
    {
        public string Email { get; set; } = "";
        public string Code { get; set; } = "";
        public string? ErrorMessage { get; set; }
        public string? InfoMessage { get; set; }
    }

    public class AccountGoogleUsernameViewModel
    {
        public string Username { get; set; } = "";
        public string? ErrorMessage { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        public string Email { get; set; } = "";
        public string? ErrorMessage { get; set; }
        public string? InfoMessage { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Email { get; set; } = "";
        public string Code { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public string? ErrorMessage { get; set; }
        public string? InfoMessage { get; set; }
    }

    public class AccountEditViewModel
    {
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = "";

        public bool HasPassword { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }

        public string? OldPictureBase64 { get; set; }
        public IFormFile? NovaSlika { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
