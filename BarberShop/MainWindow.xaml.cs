using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BarberShop
{
    public partial class MainWindow : Window
    {
        private Semaphore semaphore;
        private int maxChairs = 5;
        private bool running = true;
        private Thread barberThread;
        private List<Client> waitingList = new List<Client>();
        private object listLock = new object(); //Задать вопрос по поводу лока lock(this)?
        private int clientCount = 0;
        private Timer clientTimer;
        private Timer randomTimer;
        private Random random;

        public MainWindow()
        {
            InitializeComponent();
            semaphore = new Semaphore(maxChairs, maxChairs);
            random = new Random();
            StartSimulation();
        }

        private void StartSimulation()
        {
            barberThread = new Thread(BarberWork);
            barberThread.Start();
            Thread currentThread = Thread.CurrentThread;
            currentThread.Priority = ThreadPriority.AboveNormal;
            clientTimer = new Timer(AddClient, null, 1000, 2000);
            randomTimer = new Timer(RemoveClient, null, 5000, 10000);
            Task.Run(UpdateClientRectangles);
        }

        private void RemoveClient(object state)
        {
            lock (listLock)
            {
                if (waitingList.Count > 0)
                {
                    int randomIndex = random.Next(waitingList.Count);
                    waitingList.RemoveAt(randomIndex);
                    clientCount--;
                    UpdateUI("Клиент ушел из барбершопа по причине каких-либо обстоятельств");
                }
            }
        }


        private void AddClient(object state)
        {
            ThreadPool.QueueUserWorkItem(ClientVisit);
        }


        private void BarberWork()
        {
            while (running)
            {
                

                Client? client = null;
                bool updateUIRequired = false;

                lock (listLock)
                {
                    if (waitingList.Count > 0)
                    {
                        client = waitingList[0];
                        updateUIRequired = client != null;
                    }
                }

                if (client != null)
                {
                    Action updateBarberUI = () => BarberRect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 255));
                    Application.Current.Dispatcher.Invoke(updateBarberUI);
                    if (updateUIRequired)
                    {
                        UpdateUI($"Барбер стрижет волосы клиенту: {client.Id}");
                    }

                    Thread.Sleep(random.Next(2500, 3500));

                    lock (listLock)
                    {
                        if (waitingList.Count > 0 && waitingList[0] == client)
                        {
                            waitingList.RemoveAt(0);
                        }
                    }
                    semaphore.Release();
                    UpdateUI($"Барбер закончил стричь волосы клиенту: {client.Id}");
                    UpdateUI($"Клиент {client.Id} ушел после стрижки");
                }
                else
                {
                    UpdateUI("Барбер спит...");
                    Action updateBarberSleepUI = () => BarberRect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 165, 0));
                    Application.Current.Dispatcher.Invoke(updateBarberSleepUI);
                    Thread.Sleep(random.Next(300, 700));
                }
            }
        }


        private void ClientVisit(object state)
        {
            int clientId;
            Client client;

            lock (listLock)
            {
                clientId = ++clientCount;
                client = new Client(clientId);
            }

            semaphore.WaitOne();

            lock (listLock)
            {
                if (waitingList.Count < maxChairs)
                {
                    waitingList.Add(client);
                    UpdateUI($"Клиент {client.Id} зашел в барбершоп и ожидает на стуле");
                }
                else
                {
                    UpdateUI($"Потенциальный клиент {client.Id} ушел, так как нет свободных мест");
                }
            }

            
        }




        private async Task UpdateClientRectangles()
        {
            while (true)
            {
                await Task.Delay(100); 

                Application.Current.Dispatcher.Invoke(() =>
                {
                    lock (listLock)
                    {
                        for (int i = 0; i < maxChairs; i++)
                        {
                            if (i < waitingList.Count)
                            {
                                SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
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
                            SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
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
        private void UpdateUI(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogListBox.Items.Add(message);
                LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
            });
        }
    }
}