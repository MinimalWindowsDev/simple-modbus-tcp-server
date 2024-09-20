using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

class ModbusTCPServer
{
    private TcpListener tcpListener;
    private TcpListener dashboardListener;
    private bool isRunning = false;
    private ushort[] holdingRegisters = new ushort[100];
    private Random random = new Random();
    private ConcurrentBag<TcpClient> dashboardClients = new ConcurrentBag<TcpClient>();

    // Conveyor belt simulation variables
    private bool conveyorRunning = false;
    private ushort conveyorSpeed = 0;
    private ushort itemCount = 0;
    private bool emergencyStop = false;

    public ModbusTCPServer(int port, int dashboardPort)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        dashboardListener = new TcpListener(IPAddress.Any, dashboardPort);
        InitializeRegisters();
    }

    private void InitializeRegisters()
    {
        // Register 0: Conveyor Status (0 = Stopped, 1 = Running)
        holdingRegisters[0] = 0;
        // Register 1: Conveyor Speed (0-100%)
        holdingRegisters[1] = 0;
        // Register 2: Item Count
        holdingRegisters[2] = 0;
        // Register 3: Emergency Stop (0 = Normal, 1 = Emergency Stop)
        holdingRegisters[3] = 0;
    }

    public void Start()
    {
        isRunning = true;
        tcpListener.Start();
        dashboardListener.Start();
        Console.WriteLine("Industrial Modbus TCP Server started on port {0}", ((IPEndPoint)tcpListener.LocalEndpoint).Port);
        Console.WriteLine("Dashboard server started on port {0}", ((IPEndPoint)dashboardListener.LocalEndpoint).Port);

        Thread clientThread = new Thread(HandleClients);
        clientThread.Start();

        Thread dashboardThread = new Thread(HandleDashboardClients);
        dashboardThread.Start();

        Thread simulationThread = new Thread(SimulateConveyorBelt);
        simulationThread.Start();

        while (isRunning)
        {
            Thread.Sleep(100);
        }
    }

    private void HandleClients()
    {
        while (isRunning)
        {
            TcpClient client = tcpListener.AcceptTcpClient();
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
            clientThread.Start(client);
        }
    }

    private void HandleClient(object obj)
    {
        TcpClient tcpClient = (TcpClient)obj;
        NetworkStream stream = tcpClient.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
            byte[] response = ProcessModbusRequest(buffer, bytesRead);
            stream.Write(response, 0, response.Length);
        }

        tcpClient.Close();
    }

    private void HandleDashboardClients()
    {
        while (isRunning)
        {
            TcpClient dashboardClient = dashboardListener.AcceptTcpClient();
            dashboardClients.Add(dashboardClient);
            Console.WriteLine("New dashboard client connected");
        }
    }

    private void BroadcastToDashboards()
    {
        string data = string.Format("{0},{1},{2},{3}",
            conveyorRunning,
            conveyorSpeed,
            itemCount,
            emergencyStop);
        byte[] message = System.Text.Encoding.ASCII.GetBytes(data);

        foreach (var client in dashboardClients)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                stream.Write(message, 0, message.Length);
            }
            catch
            {
                // Remove disconnected clients
                TcpClient removedClient;
                dashboardClients.TryTake(out removedClient);
            }
        }
    }

    public void SimulateConveyorBelt()
    {
        while (isRunning)
        {
            if (conveyorRunning && !emergencyStop)
            {
                // Simulate item movement
                if (random.Next(100) < conveyorSpeed)
                {
                    itemCount++;
                    holdingRegisters[2] = itemCount;
                    Console.WriteLine("Item passed through. Total count: " + itemCount);
                }
            }

            BroadcastToDashboards();
            Thread.Sleep(100); // Update every 100ms
        }
    }

    private byte[] ProcessModbusRequest(byte[] request, int length)
    {
        ushort transactionId = (ushort)((request[0] << 8) | request[1]);
        ushort protocolId = (ushort)((request[2] << 8) | request[3]);
        ushort dataLength = (ushort)((request[4] << 8) | request[5]);
        byte unitId = request[6];
        byte functionCode = request[7];

        Console.WriteLine("Received request - Function: {0}, Transaction ID: {1}", functionCode, transactionId);

        switch (functionCode)
        {
            case 3: // Read Holding Registers
                return HandleReadHoldingRegisters(request, transactionId, unitId);
            case 6: // Write Single Register
                return HandleWriteSingleRegister(request, transactionId, unitId);
            default:
                return CreateExceptionResponse(transactionId, unitId, functionCode, 1); // Illegal function
        }
    }

    private byte[] HandleReadHoldingRegisters(byte[] request, ushort transactionId, byte unitId)
    {
        ushort startAddress = (ushort)((request[8] << 8) | request[9]);
        ushort quantity = (ushort)((request[10] << 8) | request[11]);

        if (startAddress + quantity > holdingRegisters.Length)
        {
            return CreateExceptionResponse(transactionId, unitId, 3, 2); // Illegal data address
        }

        byte[] responseData = new byte[quantity * 2];
        for (int i = 0; i < quantity; i++)
        {
            responseData[i * 2] = (byte)(holdingRegisters[startAddress + i] >> 8);
            responseData[i * 2 + 1] = (byte)(holdingRegisters[startAddress + i] & 0xFF);
        }

        byte[] response = new byte[9 + responseData.Length];
        response[0] = (byte)(transactionId >> 8);
        response[1] = (byte)(transactionId & 0xFF);
        response[2] = 0; // Protocol ID
        response[3] = 0;
        response[4] = (byte)((responseData.Length + 3) >> 8);
        response[5] = (byte)((responseData.Length + 3) & 0xFF);
        response[6] = unitId;
        response[7] = 3; // Function code
        response[8] = (byte)(responseData.Length);
        Array.Copy(responseData, 0, response, 9, responseData.Length);

        Console.WriteLine("Sent response - Read Holding Registers: Start: {0}, Quantity: {1}", startAddress, quantity);
        return response;
    }

    private byte[] HandleWriteSingleRegister(byte[] request, ushort transactionId, byte unitId)
    {
        ushort address = (ushort)((request[8] << 8) | request[9]);
        ushort value = (ushort)((request[10] << 8) | request[11]);

        if (address >= holdingRegisters.Length)
        {
            return CreateExceptionResponse(transactionId, unitId, 6, 2); // Illegal data address
        }

        holdingRegisters[address] = value;

        // Process the write based on the register
        switch (address)
        {
            case 0: // Conveyor Status
                conveyorRunning = value != 0;
                Console.WriteLine("Conveyor is now " + (conveyorRunning ? "running" : "stopped"));
                break;
            case 1: // Conveyor Speed
                conveyorSpeed = value;
                Console.WriteLine("Conveyor speed set to " + conveyorSpeed + "%");
                break;
            case 3: // Emergency Stop
                emergencyStop = value != 0;
                if (emergencyStop)
                {
                    conveyorRunning = false;
                    conveyorSpeed = 0;
                    holdingRegisters[0] = 0; // Update conveyor status
                    holdingRegisters[1] = 0; // Update conveyor speed
                    Console.WriteLine("Emergency stop activated!");
                }
                else
                {
                    Console.WriteLine("Emergency stop deactivated");
                }
                break;
        }

        byte[] response = new byte[12];
        Array.Copy(request, response, 12); // Echo the request for a write response

        Console.WriteLine("Wrote value {0} to register {1}", value, address);
        return response;
    }

    private byte[] CreateExceptionResponse(ushort transactionId, byte unitId, byte functionCode, byte exceptionCode)
    {
        byte[] response = new byte[9];
        response[0] = (byte)(transactionId >> 8);
        response[1] = (byte)(transactionId & 0xFF);
        response[2] = 0; // Protocol ID
        response[3] = 0;
        response[4] = 0;
        response[5] = 3;
        response[6] = unitId;
        response[7] = (byte)(functionCode | 0x80); // Error response
        response[8] = exceptionCode;

        Console.WriteLine("Sent exception response - Function: {0}, Exception: {1}", functionCode, exceptionCode);
        return response;
    }

    static void Main(string[] args)
    {
        ModbusTCPServer server = new ModbusTCPServer(502, 5001);
        server.Start();
    }
}