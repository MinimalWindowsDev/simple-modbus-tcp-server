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
                // Read conveyor status, speed, item count, and emergency stop status
                ushort[] data = client.ReadHoldingRegisters(1, 0, 4);
                Console.WriteLine("Conveyor Status: " + (data[0] == 0 ? "Stopped" : "Running"));
                Console.WriteLine("Conveyor Speed: " + data[1] + "%");
                Console.WriteLine("Item Count: " + data[2]);
                Console.WriteLine("Emergency Stop: " + (data[3] == 0 ? "Inactive" : "Active"));

                Console.WriteLine("\nChoose an action:");
                Console.WriteLine("1. Start/Stop Conveyor");
                Console.WriteLine("2. Set Conveyor Speed");
                Console.WriteLine("3. Activate/Deactivate Emergency Stop");
                Console.WriteLine("4. Exit");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        client.WriteSingleRegister(1, 0, (ushort)(data[0] == 0 ? 1 : 0));
                        break;
                    case "2":
                        Console.Write("Enter new speed (0-100): ");
                        ushort speed = ushort.Parse(Console.ReadLine());
                        client.WriteSingleRegister(1, 1, speed);
                        break;
                    case "3":
                        client.WriteSingleRegister(1, 3, (ushort)(data[3] == 0 ? 1 : 0));
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }

            Thread.Sleep(1000); // Wait for 1 second before next iteration
        }
    }
}