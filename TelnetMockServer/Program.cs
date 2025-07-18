using TelnetMockServer.Telnet;

namespace TelnetMockServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new TelnetServerConfig
            {
                WelcomeBeforeLogin = "=== Welcome! Please login ===",
                WelcomeAfterLoginFunc = () => $"Login time: {DateTime.Now}",
                PromptFunc = () => $"mockserver@{DateTime.Now:HH:mm}> ",
                AuthenticationMode = AuthMode.UsernameAndPassword,
                LoginPrompt = "Username:",
                PasswordPrompt = "Password:",
                SuccessLoginMessage = "You are now logged in!",
                FailLoginMessage = "Incorrect credentials, please try again.",
                TooManyAttemptsMessage = "Maximum login attempts reached. Disconnecting.",
                InvalidCommandMessage = "Unknown command, type 'help' for list.",
                IdleTimeoutMessage = "Disconnected due to inactivity.",
                MaxLoginAttempts = 3,
                IdleTimeoutSeconds = 120,
                Credentials = new Dictionary<string, string>
                {
                    ["user"] = "pass",
                    ["alice"] = "wonderland",
                    ["bob"] = "builder"
                },
                Commands = new Dictionary<string, Func<string, string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["help"] = cmd => "Commands: help, time, echo <msg>, add <num1> <num2>, exit",
                    ["time"] = cmd => DateTime.Now.ToString(),
                    ["echo"] = cmd =>
                    {
                        var parts = cmd.Split(' ', 2);
                        return parts.Length > 1 ? parts[1] : "Usage: echo <message>";
                    },
                    ["add"] = cmd =>
                    {
                        var parts = cmd.Split(' ', 3);
                        if (parts.Length < 3)
                            return "Usage: add <num1> <num2>";
                        if (int.TryParse(parts[1], out int n1) && int.TryParse(parts[2], out int n2))
                            return $"Sum is: {n1 + n2}";
                        return "Invalid numbers.";
                    },
                    ["exit"] = cmd => "Goodbye!",
                    
                }
            };

            config.Commands["#OUTPUT"] = cmd =>
            {
                // Expected format: #OUTPUT,<num1>,<num2>
                // Example: #OUTPUT,225,1

                var parts = cmd.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3 && parts[0].Equals("#OUTPUT", StringComparison.OrdinalIgnoreCase))
                {
                    // Validate the two numbers
                    if (int.TryParse(parts[1], out int num1) && int.TryParse(parts[2], out int num2))
                    {
                        return $"?OUTPUT,{num1},{num2}";
                    }
                    else
                    {
                        return "Invalid number format.";
                    }
                }
                return "Invalid command format. Use #OUTPUT,<num1>,<num2>";
            };

            var server = new Telnet.TelnetMockServer(23, config);
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Shutting down...");
                cts.Cancel();
                e.Cancel = true;
            };

            await server.StartAsync(cts.Token);
        }
    }
}
