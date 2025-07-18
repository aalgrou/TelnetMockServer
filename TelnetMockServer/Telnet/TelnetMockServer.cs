using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TelnetMockServer.Telnet;

public class TelnetMockServer
{
    private readonly TcpListener listener;
    private readonly int port;
    private readonly TelnetServerConfig config;

    public TelnetMockServer(int port, TelnetServerConfig config)
    {
        this.port = port;
        this.config = config ?? throw new ArgumentNullException(nameof(config));
        listener = new TcpListener(IPAddress.Any, port);

        if (config.Commands.Count == 0)
        {
            config.Commands["help"] = cmd => "Available commands: help, time, echo <msg>, add <num1> <num2>, exit";
            config.Commands["time"] = cmd => DateTime.Now.ToString();
            config.Commands["echo"] = cmd =>
            {
                var parts = cmd.Split(' ', 2);
                return parts.Length > 1 ? parts[1] : "Usage: echo <message>";
            };
            config.Commands["add"] = cmd =>
            {
                var parts = cmd.Split(' ', 3);
                if (parts.Length < 3)
                    return "Usage: add <num1> <num2>";

                if (int.TryParse(parts[1], out int n1) && int.TryParse(parts[2], out int n2))
                    return $"Result: {n1 + n2}";

                return "Invalid numbers.";
            };
            config.Commands["exit"] = cmd => "Goodbye!";
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        listener.Start();
        Console.WriteLine($"Server started on port {port}.");
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client); // fire and forget
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            var stream = client.GetStream();

            // Send welcome before login
            if (config.WelcomeBeforeLoginFunc != null)
                await SendAsync(stream, config.WelcomeBeforeLoginFunc() + "\r\n");
            else if (!string.IsNullOrEmpty(config.WelcomeBeforeLogin))
                await SendAsync(stream, config.WelcomeBeforeLogin + "\r\n");

            int loginAttempts = 0;
            bool authenticated = false;
            string? username = null;

            if (config.AuthenticationMode != AuthMode.None)
            {
                while (!authenticated && loginAttempts < config.MaxLoginAttempts)
                {
                    await SendAsync(stream, config.LoginPrompt + " ");
                    username = await ReadLineAsync(stream, config.IdleTimeoutSeconds);
                    if (username == null) break;

                    if (config.AuthenticationMode == AuthMode.UsernameAndPassword)
                    {
                        await SendAsync(stream, config.PasswordPrompt + " ");
                        string? password = await ReadLineAsync(stream, config.IdleTimeoutSeconds, maskInput: true);
                        if (password == null) break;

                        if (config.Credentials.TryGetValue(username, out string? correctPassword) && correctPassword == password)
                        {
                            authenticated = true;
                            break;
                        }
                    }
                    else if (config.AuthenticationMode == AuthMode.UsernameOnly)
                    {
                        if (config.Credentials.ContainsKey(username))
                        {
                            authenticated = true;
                            break;
                        }
                    }

                    loginAttempts++;
                    await SendAsync(stream, config.FailLoginMessage + "\r\n");
                }

                if (!authenticated)
                {
                    await SendAsync(stream, config.TooManyAttemptsMessage + "\r\n");
                    return;
                }

                await SendAsync(stream, config.SuccessLoginMessage + "\r\n");

                if (config.WelcomeAfterLoginFunc != null)
                    await SendAsync(stream, config.WelcomeAfterLoginFunc() + "\r\n");
                else if (!string.IsNullOrEmpty(config.WelcomeAfterLogin))
                    await SendAsync(stream, config.WelcomeAfterLogin + "\r\n");
            }
            else
            {
                if (config.WelcomeAfterLoginFunc != null)
                    await SendAsync(stream, config.WelcomeAfterLoginFunc() + "\r\n");
                else if (!string.IsNullOrEmpty(config.WelcomeAfterLogin))
                    await SendAsync(stream, config.WelcomeAfterLogin + "\r\n");
            }

            // Command loop
            while (client.Connected)
            {
                string promptToSend = config.PromptFunc != null ? config.PromptFunc() : config.Prompt;
                await SendAsync(stream, promptToSend);

                string? command = await ReadLineAsync(stream, config.IdleTimeoutSeconds);
                if (command == null)
                {
                    await SendAsync(stream, "\r\n" + config.IdleTimeoutMessage + "\r\n");
                    break;
                }

                if (string.IsNullOrWhiteSpace(command))
                    continue;

                // Exit command handling
                if (command.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    command.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    await SendAsync(stream, "Goodbye!\r\n");
                    break;
                }

                // Improved command matching: exact first, then prefix match
                Func<string, string>? handler = null;

                if (config.Commands.TryGetValue(command, out handler))
                {
                    // exact match
                }
                else
                {
                    foreach (var key in config.Commands.Keys)
                    {
                        if (command.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                        {
                            handler = config.Commands[key];
                            break;
                        }
                    }
                }

                if (handler != null)
                {
                    string response = handler(command);
                    await SendAsync(stream, response + "\r\n");
                }
                else
                {
                    await SendAsync(stream, config.InvalidCommandMessage + "\r\n");
                }
            }
        }
    }

    private async Task SendAsync(NetworkStream stream, string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        await stream.WriteAsync(data, 0, data.Length);
    }

    private async Task<string?> ReadLineAsync(NetworkStream stream, int timeoutSeconds, bool maskInput = false)
    {
        var buffer = new List<byte>();
        var bufferSize = 1;
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (true)
        {
            if (!stream.DataAvailable)
            {
                await Task.Delay(50);
                if (stopwatch.Elapsed > timeout)
                {
                    return null;
                }
                continue;
            }

            var tempBuffer = new byte[bufferSize];
            int bytesRead = await stream.ReadAsync(tempBuffer, 0, bufferSize);

            if (bytesRead == 0)
                return null;

            var b = tempBuffer[0];

            if (b == 13)
            {
                if (stream.DataAvailable)
                    await stream.ReadAsync(tempBuffer, 0, 1);
                break;
            }
            else if (b == 10)
            {
                break;
            }
            else
            {
                buffer.Add(b);
                if (!maskInput)
                {
                    // No echo to avoid double letters; clients handle echo
                }
            }
        }

        return Encoding.ASCII.GetString(buffer.ToArray());
    }
}
