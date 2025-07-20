namespace TelnetMockServer.Telnet;

public class TelnetServerConfig
{
    public Func<string>? WelcomeBeforeLoginFunc { get; set; }
    public string? WelcomeBeforeLogin { get; set; }

    public bool AllowOnlySingleConnection { get; set; } = false;
    public Func<string>? WelcomeAfterLoginFunc { get; set; }
    public string? WelcomeAfterLogin { get; set; }

    public AuthMode AuthenticationMode { get; set; } = AuthMode.UsernameAndPassword;
    public string LoginPrompt { get; set; } = "Login:";
    public string PasswordPrompt { get; set; } = "Password:";
    public string SuccessLoginMessage { get; set; } = "Login successful!";
    public string FailLoginMessage { get; set; } = "Login failed. Try again.";
    public string TooManyAttemptsMessage { get; set; } = "Too many failed attempts. Disconnecting.";
    public string InvalidCommandMessage { get; set; } = "Invalid command.";
    public string IdleTimeoutMessage { get; set; } = "Idle timeout, disconnecting.";
    public int MaxLoginAttempts { get; set; } = 3;
    public int IdleTimeoutSeconds { get; set; } = 60;

    public Func<string>? PromptFunc { get; set; }
    public string Prompt { get; set; } = "> ";

    public Dictionary<string, string> Credentials { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, Func<string, string>> Commands { get; set; } = new Dictionary<string, Func<string, string>>(StringComparer.OrdinalIgnoreCase);
}