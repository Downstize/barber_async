using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;

namespace BarberShop;
public class Client
{
    public int Id { get; set; }
    public bool Ready { get; set; } = false;
    public int ClientPosition { get; set; } = -1;
    public SolidColorBrush Brush { get;}

    public Client(int id)
    {
        Id = id;
        Brush = GetRandomColorBrush();
    }
    
    private SolidColorBrush GetRandomColorBrush()
    {
        Random rand = new Random();
        return new SolidColorBrush(Color.FromRgb((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256)));
    }
    
}