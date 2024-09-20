using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class ModbusTCPServer
{
    private TcpListener tcpListener;
    private bool isRunning = false;
    private ushort[] holdingRegisters = new ushort[100];
    private Random random = new Random();

    public ModbusTCPServer(int port)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        InitializeRegisters();
    }

    private void InitializeRegisters()
    {
        // Simulate some initial values
        holdingRegisters[0] = 2500; // Temperature sensor 1 (25.00 C)
        holdingRegisters[1] = 2200; // Temperature sensor 2 (22.00 C)
        holdingRegisters[2] = 1; // Control flag 1
        holdingRegisters[3] = 0; // Control flag 2
    }

    public void Start()
    {
        isRunning = true;
        tcpListener.Start();
        Console.WriteLine("Modbus TCP Server started on port {0}", ((IPEndPoint)tcpListener.LocalEndpoint).Port);

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

    public void SimulateDataChanges()
    {
        while (isRunning)
        {
            // Simulate temperature changes
            holdingRegisters[0] = (ushort)(2500 + random.Next(-100, 101)); // +/- 1.00 C
            holdingRegisters[1] = (ushort)(2200 + random.Next(-100, 101)); // +/- 1.00 C

            // Simulate control flag changes
            if (random.Next(10) == 0) // 10% chance to flip a flag
            {
                int flagIndex = random.Next(2) + 2; // Choose between register 2 and 3
                holdingRegisters[flagIndex] = (ushort)(1 - holdingRegisters[flagIndex]); // Flip 0 to 1 or 1 to 0
                Console.WriteLine("Control flag {0} changed to {1}", flagIndex - 1, holdingRegisters[flagIndex]);
            }

            Thread.Sleep(5000); // Wait for 5 seconds before next update
        }
    }

    static void Main(string[] args)
    {
        ModbusTCPServer server = new ModbusTCPServer(502);
        Thread simulationThread = new Thread(server.SimulateDataChanges);
        simulationThread.Start();
        server.Start();
    }
}