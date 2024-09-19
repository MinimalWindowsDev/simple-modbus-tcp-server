# Simple Modbus TCP Server

A lightweight Modbus TCP server implementation in C# for Windows.

## Description

This project provides a basic Modbus TCP server that can be compiled and run using the Windows Command Prompt (CMD). It supports the Read Holding Registers (0x03) function and can handle multiple client connections.

## Features

- Listens on port 502 (default Modbus TCP port)
- Handles multiple client connections using threads
- Implements Read Holding Registers (0x03) function
- Responds with an error for unsupported function codes

## Requirements

- Windows operating system
- .NET Framework 4.0 or later

## Usage

1. Clone this repository or download the source files.
2. Open Command Prompt and navigate to the project directory.
3. Run the `build.bat` file to compile and start the server.


## Customization

You can modify the `ModbusTCPServer.cs` file to add more Modbus functions or change the server's behavior. After making changes, simply run the batch file again to recompile and run the updated server.

## License

This project is licensed under the WTFPL (Do What The F*ck You Want To Public License). See the [LICENSE](https://en.wikipedia.org/wiki/WTFPL) for details.

## Disclaimer

This is a basic implementation intended for educational purposes. It may not be suitable for production environments without further development and security considerations.

## Contributing

Feel free to fork this project and submit pull requests with improvements or bug fixes.