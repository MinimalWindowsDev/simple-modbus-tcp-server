# Industrial Modbus TCP Server and Client Simulation

A C# implementation of a Modbus TCP server and client, simulating an industrial conveyor belt system.

## Description

This project provides a Modbus TCP server that simulates a conveyor belt system and a client that interacts with it. The simulation demonstrates the use of Modbus TCP in an industrial setting, allowing users to control and monitor a virtual conveyor belt system.

## Features

- Server simulates a conveyor belt system with:
  - Conveyor status (running/stopped)
  - Conveyor speed control (0-100%)
  - Item counter
  - Emergency stop functionality
- Client provides a user interface to:
  - Monitor conveyor status, speed, and item count
  - Start/stop the conveyor
  - Adjust conveyor speed
  - Activate/deactivate emergency stop
- Uses Modbus TCP for communication
- Implemented in C# with MSBuild project files

## Requirements

- Windows operating system
- .NET Framework 4.0 or later
- MSBuild (included in .NET Framework)

## Usage

1. Clone this repository or download the source files.
2. Open Command Prompt and navigate to the project directory.
3. Run the `build.bat` file to compile and start both the server and client:

```
build.bat
```

This will:

- Compile both the server (ModbusTCPServer.csproj) and client (ModbusTCPClient.csproj) using MSBuild
- Start the server in a new window
- Run the client in the current window

The server will simulate the conveyor belt system, and the client will allow you to interact with it.

## Project Structure

- `ModbusTCPServer.cs`: Server implementation (conveyor belt simulation)
- `ModbusTCPClient.cs`: Client implementation (user interface)
- `ModbusTCPServer.csproj`: MSBuild project file for the server
- `ModbusTCPClient.csproj`: MSBuild project file for the client
- `build.bat`: Batch script to build and run the projects

## Simulation Details

The simulation uses the following Modbus registers:

- Register 0: Conveyor Status (0 = Stopped, 1 = Running)
- Register 1: Conveyor Speed (0-100%)
- Register 2: Item Count
- Register 3: Emergency Stop (0 = Inactive, 1 = Active)

The client allows you to read these registers and write to registers 0, 1, and 3 to control the conveyor belt system.

## Customization

You can modify the `.cs` files to add more features to the simulation, implement additional Modbus functions, or change the behavior of the conveyor belt system. If you need to add references or change build settings, you can modify the `.csproj` files.

## License

This project is licensed under the WTFPL (Do What The F\*ck You Want To Public License). See the [LICENSE](https://en.wikipedia.org/wiki/WTFPL) for details.

## Disclaimer

This simulation is intended for educational and demonstration purposes. It may not reflect all the complexities and safety considerations of a real industrial system.

## Contributing

Feel free to fork this project and submit pull requests with improvements or bug fixes. Some areas for potential enhancement include:

- Adding more simulated industrial components
- Implementing additional Modbus functions
- Improving error handling and logging
- Enhancing the client's user interface
- Adding unit tests for the simulation logic
