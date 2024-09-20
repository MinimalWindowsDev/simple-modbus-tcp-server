using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Threading;
using LiveCharts;
using LiveCharts.Wpf;

namespace ServerDashboard
{
    public partial class MainWindow : Window
    {
        private TcpClient dashboardClient;
        private ChartValues<int> speedValues;
        private ChartValues<int> itemCountValues;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToDashboardServer();
            InitializeCharts();
        }

        private void InitializeCharts()
        {
            speedValues = new ChartValues<int>();
            itemCountValues = new ChartValues<int>();

            SpeedChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Speed",
                    Values = speedValues
                }
            };

            ItemCountChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Item Count",
                    Values = itemCountValues
                }
            };
        }

        private void ConnectToDashboardServer()
        {
            dashboardClient = new TcpClient("localhost", 5001);
            Thread receiveThread = new Thread(ReceiveData);
            receiveThread.Start();
        }

        private void ReceiveData()
        {
            NetworkStream stream = dashboardClient.GetStream();
            byte[] data = new byte[1024];
            while (true)
            {
                int bytesRead = stream.Read(data, 0, data.Length);
                string receivedData = System.Text.Encoding.ASCII.GetString(data, 0, bytesRead);
                string[] values = receivedData.Split(',');

                Dispatcher.Invoke(() =>
                {
                    bool conveyorRunning = bool.Parse(values[0]);
                    int speed = int.Parse(values[1]);
                    int itemCount = int.Parse(values[2]);
                    bool emergencyStop = bool.Parse(values[3]);

                    ConveyorStatus.Fill = conveyorRunning ? Brushes.Green : Brushes.Red;
                    SpeedValue.Text = speed.ToString();
                    ItemCountValue.Text = itemCount.ToString();
                    EmergencyStopStatus.Fill = emergencyStop ? Brushes.Red : Brushes.Green;

                    speedValues.Add(speed);
                    if (speedValues.Count > 50) speedValues.RemoveAt(0);

                    itemCountValues.Add(itemCount);
                    if (itemCountValues.Count > 50) itemCountValues.RemoveAt(0);
                });
            }
        }
    }
}