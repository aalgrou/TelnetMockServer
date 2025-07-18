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
