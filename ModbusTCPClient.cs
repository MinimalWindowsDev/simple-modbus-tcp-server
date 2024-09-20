using System;
using System.Net.Sockets;
using System.Threading;

class ModbusTCPClient
{
    private string ipAddress;
    private int port;
    private ushort transactionId = 0;

    public ModbusTCPClient(string ipAddress, int port)
    {
        this.ipAddress = ipAddress;
        this.port = port;
    }

    public byte[] SendModbusRequest(byte[] request)
    {
        using (TcpClient client = new TcpClient(ipAddress, port))
        using (NetworkStream stream = client.GetStream())
        {
            stream.Write(request, 0, request.Length);

            byte[] response = new byte[1024];
            int bytesRead = stream.Read(response, 0, response.Length);

            byte[] trimmedResponse = new byte[bytesRead];
            Array.Copy(response, trimmedResponse, bytesRead);

            return trimmedResponse;
        }
    }

    public ushort[] ReadHoldingRegisters(byte unitId, ushort startAddress, ushort quantity)
    {
        byte[] request = new byte[12];
        ushort requestTransactionId = ++transactionId;
        request[0] = (byte)(requestTransactionId >> 8);
        request[1] = (byte)(requestTransactionId & 0xFF);
        request[2] = 0; // Protocol ID
        request[3] = 0;
        request[4] = 0;
        request[5] = 6; // Length
        request[6] = unitId;
        request[7] = 3; // Function code (Read Holding Registers)
        request[8] = (byte)(startAddress >> 8);
        request[9] = (byte)(startAddress & 0xFF);
        request[10] = (byte)(quantity >> 8);
        request[11] = (byte)(quantity & 0xFF);

        Console.WriteLine("Sending Read Holding Registers request - Start: {0}, Quantity: {1}", startAddress, quantity);
        byte[] response = SendModbusRequest(request);

        if (response.Length < 9 || response[7] != 3)
        {
            throw new Exception("Invalid response received");
        }

        int dataLength = response[8];
        ushort[] registers = new ushort[dataLength / 2];
        for (int i = 0; i < registers.Length; i++)
        {
            registers[i] = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
        }

        return registers;
    }

    public void WriteSingleRegister(byte unitId, ushort address, ushort value)
    {
        byte[] request = new byte[12];
        ushort requestTransactionId = ++transactionId;
        request[0] = (byte)(requestTransactionId >> 8);
        request[1] = (byte)(requestTransactionId & 0xFF);
        request[2] = 0; // Protocol ID
        request[3] = 0;
        request[4] = 0;
        request[5] = 6; // Length
        request[6] = unitId;
        request[7] = 6; // Function code (Write Single Register)
        request[8] = (byte)(address >> 8);
        request[9] = (byte)(address & 0xFF);
        request[10] = (byte)(value >> 8);
        request[11] = (byte)(value & 0xFF);

        Console.WriteLine("Sending Write Single Register request - Address: {0}, Value: {1}", address, value);
        byte[] response = SendModbusRequest(request);

        if (response.Length != 12 || response[7] != 6)
        {
            throw new Exception("Invalid response received");
        }

        Console.WriteLine("Write operation successful");
    }

    static void Main(string[] args)
    {
        ModbusTCPClient client = new ModbusTCPClient("127.0.0.1", 502);

        while (true)
        {
            try
            {
                // Read temperature sensors
                ushort[] temperatures = client.ReadHoldingRegisters(1, 0, 2);
                Console.WriteLine("Temperature Sensor 1: {0:F2} C", temperatures[0] / 100.0);
                Console.WriteLine("Temperature Sensor 2: {0:F2} C", temperatures[1] / 100.0);

                // Read control flags
                ushort[] flags = client.ReadHoldingRegisters(1, 2, 2);
                Console.WriteLine("Control Flag 1: {0}", flags[0]);
                Console.WriteLine("Control Flag 2: {0}", flags[1]);

                // Write to a control flag
                Console.Write("Do you want to change a control flag? (y/n): ");
                if (Console.ReadLine().ToLower() == "y")
                {
                    Console.Write("Enter flag number (1 or 2): ");
                    int flagNumber = int.Parse(Console.ReadLine());
                    Console.Write("Enter new value (0 or 1): ");
                    ushort newValue = ushort.Parse(Console.ReadLine());
                    client.WriteSingleRegister(1, (ushort)(flagNumber + 1), newValue);
                }

                Console.WriteLine("Waiting for 5 seconds before next read...");
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }

            Console.WriteLine();
        }
    }
}