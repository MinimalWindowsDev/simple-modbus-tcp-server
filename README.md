# Enhanced Modbus TCP Server and Client

A lightweight Modbus TCP server and client implementation in C# for Windows, demonstrating interactive communication.

## Description

This project provides an enhanced Modbus TCP server and client that can be compiled and run using the Windows Command Prompt (CMD). The server simulates real-world data (temperature sensors and control flags) and supports multiple Modbus functions. The client interacts with the server, reading data and allowing user input to modify control flags.

## Features

- Server listens on port 502 (default Modbus TCP port)
- Server simulates temperature sensor data and control flags
- Server supports Read Holding Registers (0x03) and Write Single Register (0x06) functions
- Server handles multiple client connections using threads
- Client reads temperature and control flag data from the server
- Client allows user to modify control flags
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

The server will continue running in the background, simulating data changes. The client will periodically read data from the server and allow you to modify control flags.

## Customization

You can modify the `ModbusTCPServer.cs` file to add more Modbus functions, change the simulated data behavior, or adjust the server's response to client requests. You can also modify `ModbusTCPClient.cs` to implement additional features or change how it interacts with the server.

After making changes, run the `build.bat` file again to recompile and run the updated server and client.

## License

This project is licensed under the WTFPL (Do What The F*ck You Want To Public License). See the [LICENSE](https://en.wikipedia.org/wiki/WTFPL) for details.

## Disclaimer

While this implementation is more sophisticated than the basic version, it is still intended for educational and demonstration purposes. It may not be suitable for production environments without further development, security considerations, and thorough testing.

## Contributing

Feel free to fork this project and submit pull requests with improvements or bug fixes. Some areas for potential enhancement include:
- Implementing more Modbus functions
- Adding error handling and logging
- Improving the simulation of real-world data
- Enhancing the client's user interface