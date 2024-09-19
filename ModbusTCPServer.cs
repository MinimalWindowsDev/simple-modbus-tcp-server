using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class ModbusTCPServer
{
    private TcpListener tcpListener;
    private bool isRunning = false;
    private byte[] holdingRegisters = new byte[100];

    public ModbusTCPServer(int port)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
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
            // Process Modbus TCP request
            byte[] response = ProcessModbusRequest(buffer, bytesRead);

            // Send response
            stream.Write(response, 0, response.Length);
        }

        tcpClient.Close();
    }

    private byte[] ProcessModbusRequest(byte[] request, int length)
    {
        // Extract Modbus TCP header
        ushort transactionId = (ushort)((request[0] << 8) | request[1]);
        ushort protocolId = (ushort)((request[2] << 8) | request[3]);
        ushort dataLength = (ushort)((request[4] << 8) | request[5]);
        byte unitId = request[6];
        byte functionCode = request[7];

        // Process function code (only implementing Read Holding Registers - 0x03)
        if (functionCode == 0x03)
        {
            ushort startAddress = (ushort)((request[8] << 8) | request[9]);
            ushort quantity = (ushort)((request[10] << 8) | request[11]);

            byte[] responseData = new byte[quantity * 2];
            Array.Copy(holdingRegisters, startAddress * 2, responseData, 0, quantity * 2);

            // Construct response
            byte[] response = new byte[9 + responseData.Length];
            response[0] = (byte)(transactionId >> 8);
            response[1] = (byte)(transactionId & 0xFF);
            response[2] = (byte)(protocolId >> 8);
            response[3] = (byte)(protocolId & 0xFF);
            response[4] = (byte)((responseData.Length + 3) >> 8);
            response[5] = (byte)((responseData.Length + 3) & 0xFF);
            response[6] = unitId;
            response[7] = functionCode;
            response[8] = (byte)(responseData.Length);
            Array.Copy(responseData, 0, response, 9, responseData.Length);

            return response;
        }
        else
        {
            // Unsupported function code
            byte[] response = new byte[9];
            response[0] = (byte)(transactionId >> 8);
            response[1] = (byte)(transactionId & 0xFF);
            response[2] = (byte)(protocolId >> 8);
            response[3] = (byte)(protocolId & 0xFF);
            response[4] = 0;
            response[5] = 3;
            response[6] = unitId;
            response[7] = (byte)(functionCode | 0x80); // Error response
            response[8] = 0x01; // Illegal function

            return response;
        }
    }

    static void Main(string[] args)
    {
        ModbusTCPServer server = new ModbusTCPServer(502);
        server.Start();
    }
}