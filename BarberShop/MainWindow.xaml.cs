using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Async_lab1;

namespace BarberShop
{
    public partial class MainWindow : Window
    {
        private List<Thread> clientThreads = new List<Thread>();
        private int maxChairs = 5;
        private bool running = true;
        private Thread barberThread;
        private Thread randThread;
        private List<Client> waitingList = new List<Client>();
        private object listLock = new object();
        private int clientCount = 0;
        private Timer clientTimer;
        private Timer randomTimer;

        public MainWindow()
        {
            InitializeComponent();
            StartSimulation();
        }

        private void StartSimulation()
        {
            barberThread = new Thread(BarberWork);
            barberThread.Start();
            Thread currentThread = Thread.CurrentThread;
            currentThread.Priority = ThreadPriority.AboveNormal;
            /*randThread = new Thread(randomRemove);
            Thread.Sleep(20000);*/
            randomTimer = new Timer(randomRemove, null, 0, Timeout.Infinite);
            clientTimer = new Timer(AddClient, null, 0, Timeout.Infinite); // Запускаем таймер для добавления первого клиента
            Task.Run(UpdateClientRectangles); // Start updating client rectangles
        }
        
        private void randomRemove(Object state)
        {
            clientCount--;
            if (waitingList.Count > 0)
            {
                int randomIndex = new Random().Next(waitingList.Count);
                waitingList.RemoveAt(randomIndex);
            }
            randomTimer.Change(new Random().Next(1000, 3000), Timeout.Infinite);
            UpdateUI("Клиент ушел из барбершопа по причине срочно появившегося дела");
        }
        
        private void AddClient(object state)
        {
            clientCount++;
            Thread clientThread = new Thread(ClientVisit);
            clientThreads.Add(clientThread);
            clientThread.Start(clientCount);
            Thread.Sleep(new Random().Next(300, 1000)); // Интервал между появлениями клиентов
            clientTimer.Change(new Random().Next(300, 2000), Timeout.Infinite); // Устанавливаем новый интервал для следующего вызова
        }

        private void BarberWork()
        {
            while (running)
            {
                Client client = null;

                lock (listLock)
                {
                    if (waitingList.Count > 0)
                    {
                        client = waitingList[0];
                        waitingList.RemoveAt(0);
                        foreach (var elem in waitingList)
                        {
                            Console.WriteLine(elem);
                        }
                        Console.WriteLine("-----");
                    }
                }

                if (client != null)
                {
                    UpdateUI($"Барбер стрижет волосы клиенту: {client.Id}");
                    Thread.Sleep(new Random().Next(2000, 4000)); // Время стрижки
                    UpdateUI($"Барбер закончил стричь волосы клиенту: {client.Id}");
                }
                else
                {
                    UpdateUI("Барбер спит...");
                    Thread.Sleep(new Random().Next(300, 700)); // Время, пока парикмахер спит
                }
            }
        }

        private void ClientVisit(object clientId)
        {
            int id = (int)clientId;
            Client client = new Client(id);

            lock (listLock)
            {
                if (waitingList.Count < maxChairs)
                {
                    waitingList.Add(client);
                    UpdateUI($"Клиент {client.Id} зашел в магазин и нежиться на диване, ожидая своей очереди");
                }
                else
                {
                    UpdateUI($"Потенциальный клиент {client.Id} ушел, так как ему негде было присесть");
                }
            }
        }
        
        // Inside MainWindow class

private async Task UpdateClientRectangles()
{
    while (true)
    {
        await Task.Delay(100); // Adjust the delay as needed

        Application.Current.Dispatcher.Invoke(() =>
        {
            lock (listLock)
            {
                for (int i = 0; i < maxChairs; i++)
                {
                    if (i < waitingList.Count)
                    {
                        SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0)); // Green
                        switch (i)
                        {
                            case 0:
                                Client1Rect.Fill = brush;
                                break;
                            case 1:
                                Client2Rect.Fill = brush;
                                break;
                            case 2:
                                Client3Rect.Fill = brush;
                                break;
                            case 3:
                                Client4Rect.Fill = brush;
                                break;
                            case 4:
                                Client5Rect.Fill = brush;
                                break;
                        }
                    }
                    else
                    {
                        SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0)); // Red
                        switch (i)
                        {
                            case 0:
                                Client1Rect.Fill = brush;
                                break;
                            case 1:
                                Client2Rect.Fill = brush;
                                break;
                            case 2:
                                Client3Rect.Fill = brush;
                                break;
                            case 3:
                                Client4Rect.Fill = brush;
                                break;
                            case 4:
                                Client5Rect.Fill = brush;
                                break;
                        }
                    }
                }
            }
        });
    }
}

private void ClientLeaving(int clientId)
{
    lock (listLock)
    {
        var clientToRemove = waitingList.Find(client => client.Id == clientId);
        if (clientToRemove != null)
        {
            waitingList.Remove(clientToRemove);
        }
    }
}

// Modify ClientVisit method





        private void UpdateUI(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogListBox.Items.Add(message);
                LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            running = false;
            foreach (var thread in clientThreads)
            {
                thread.Join();
            }
            barberThread.Join();
        }
    }
}