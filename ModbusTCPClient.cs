using System;
using System.Net.Sockets;

class ModbusTCPClient
{
    private string ipAddress;
    private int port;

    public ModbusTCPClient(string ipAddress, int port)
    {
        this.ipAddress = ipAddress;
        this.port = port;
    }

    public byte[] ReadHoldingRegisters(byte unitId, ushort startAddress, ushort quantity)
    {
        using (TcpClient client = new TcpClient(ipAddress, port))
        using (NetworkStream stream = client.GetStream())
        {
            // Construct Modbus TCP request
            byte[] request = new byte[12];
            ushort transactionId = 1;
            request[0] = (byte)(transactionId >> 8);
            request[1] = (byte)(transactionId & 0xFF);
            request[2] = 0; // Protocol ID (always 0 for Modbus TCP)
            request[3] = 0;
            request[4] = 0; // Length (updated later)
            request[5] = 6; // Length of the rest of the message
            request[6] = unitId;
            request[7] = 3; // Function code (3 for Read Holding Registers)
            request[8] = (byte)(startAddress >> 8);
            request[9] = (byte)(startAddress & 0xFF);
            request[10] = (byte)(quantity >> 8);
            request[11] = (byte)(quantity & 0xFF);

            // Send request
            stream.Write(request, 0, request.Length);

            // Read response
            byte[] response = new byte[1024];
            int bytesRead = stream.Read(response, 0, response.Length);

            // Process response
            if (bytesRead >= 9 && response[7] == 3)
            {
                int dataLength = response[8];
                byte[] data = new byte[dataLength];
                Array.Copy(response, 9, data, 0, dataLength);
                return data;
            }
            else
            {
                throw new Exception("Invalid response received");
            }
        }
    }

    static void Main(string[] args)
    {
        ModbusTCPClient client = new ModbusTCPClient("127.0.0.1", 502);

        try
        {
            byte[] result = client.ReadHoldingRegisters(1, 0, 10);
            Console.WriteLine("Read Holding Registers result:");
            for (int i = 0; i < result.Length; i += 2)
            {
                ushort value = (ushort)((result[i] << 8) | result[i + 1]);
                Console.WriteLine("Register {0}: {1}", i / 2, value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: {0}", ex.Message);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}