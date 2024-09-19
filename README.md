# Simple Modbus TCP Server and Client

A lightweight Modbus TCP server and client implementation in C# for Windows.

## Description

This project provides a basic Modbus TCP server and client that can be compiled and run using the Windows Command Prompt (CMD). The server supports the Read Holding Registers (0x03) function and can handle multiple client connections. The client demonstrates how to connect to the server and read holding registers.

## Features

- Server listens on port 502 (default Modbus TCP port)
- Server handles multiple client connections using threads
- Server implements Read Holding Registers (0x03) function
- Server responds with an error for unsupported function codes
- Client demonstrates reading holding registers from the server
- Combined build script to compile and run both server and client

## Requirements

- Windows operating system
- .NET Framework 4.0 or later

## Usage

1. Clone this repository or download the source files.
2. Open Command Prompt and navigate to the project directory.
3. Run the `build.bat` file to compile and start both the server and client:

```
build.bat
```

This will:
- Compile both the server (ModbusTCPServer.cs) and client (ModbusTCPClient.cs)
- Start the server in a new window
- Run the client in the current window

The server will continue running in the background after the client has finished executing.

## Customization

You can modify the `ModbusTCPServer.cs` file to add more Modbus functions or change the server's behavior. You can also modify `ModbusTCPClient.cs` to test different scenarios or implement additional Modbus functions. After making changes, simply run the batch file again to recompile and run the updated server and client.

## License

This project is licensed under the WTFPL (Do What The F*ck You Want To Public License). See the [LICENSE](https://en.wikipedia.org/wiki/WTFPL) for details.

## Disclaimer

This is a basic implementation intended for educational purposes. It may not be suitable for production environments without further development and security considerations.

## Contributing

Feel free to fork this project and submit pull requests with improvements or bug fixes.