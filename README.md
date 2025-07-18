# Telnet Mock Server in C#

A configurable Telnet mock server written in C# for testing, simulation, or educational purposes.

---

## Features

- Configurable welcome message (before login and after successful login), supports static strings or dynamic functions  
- Supports three authentication modes:  
  - None (no authentication)  
  - Username only  
  - Username and password  
- Customizable user credentials store  
- Configurable login success/failure messages and max login attempts  
- Custom commands with dynamic handlers (function-based responses)  
- Built-in example commands: `help`, `time`, `echo`, `add`, `exit`  
- Idle timeout disconnect support  
- Supports multiple simultaneous client connections asynchronously  
- Customizable prompt symbol/message before each command input (static string or function)  
- Easy to extend and customize via configuration object  

---

## Getting Started

### Prerequisites

- [.NET 6 SDK or later](https://dotnet.microsoft.com/download)

### Build and Run

1. Clone the repository:

   ```bash
   git clone https://github.com/yourusername/telnet-mock-server.git
   cd telnet-mock-server
   ```

2. Build the project:

   ```bash
   dotnet build
   ```

3. Run the server:

   ```bash
   dotnet run --project TelnetMockServer
   ```

4. Connect using a Telnet client:

   ```bash
   telnet localhost 23
   ```

---

## Default Credentials for Testing

| Username | Password   |
| -------- | ---------- |
| alice    | wonderland |
| bob      | builder    |
| user     | pass       |

---

## Configuration

All settings are configurable via the `TelnetServerConfig` class. You can customize:

- Welcome messages **before login** and **after login**, either as fixed strings or as functions returning dynamic strings  
- Authentication mode and user credentials  
- Login prompts and messages for success, failure, and disconnection  
- Commands and their handlers, which are functions that receive the full command and return a response string  
- Maximum login attempts and idle timeout duration  
- **Prompt shown before each command input line**, customizable as a static string or a dynamic function  

---

## Extending Commands

Commands are registered as a dictionary of command prefix strings to handler functions:

```csharp
config.Commands["echo"] = cmd =>
{
    var parts = cmd.Split(' ', 2);
    return parts.Length > 1 ? parts[1] : "Usage: echo <message>";
};
```

The handler receives the full command string and returns the response string to send back to the client.

---

## Customizing Prompt

The prompt shown before every command input line can be set as:

- A fixed string, e.g.:

  ```csharp
  config.Prompt = "mock> ";
  ```

- Or a dynamic function that returns a string, e.g.:

  ```csharp
  config.PromptFunc = () => $"server@{DateTime.Now:HH:mm:ss}> ";
  ```

If both are set, `PromptFunc` takes precedence.

---

## Contributing

Contributions and improvements are welcome! Feel free to submit issues or pull requests.

---

## License

This project is licensed under the MIT License.
---

## Contact

Created by [Abdullah AlGrou](https://github.com/aalgrou).  
Feel free to reach out via GitHub or email.
