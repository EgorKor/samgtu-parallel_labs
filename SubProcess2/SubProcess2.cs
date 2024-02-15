// See https://aka.ms/new-console-template for more information
using System.Diagnostics.SymbolStore;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

Console.WriteLine("SubProcess2 started...");
try
{
    byte[] buffer = new byte[4];
    Semaphore semaphore = Semaphore.OpenExisting("Global\\GraphicsSemaphore");
    Console.WriteLine("Semaphore get succesfully...");
    Socket connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    connectSocket.Connect(new IPEndPoint(new IPAddress(new byte[4] { 127, 0, 0, 1 }), 20001));
    Console.WriteLine("Succesfully connected...");
    connectSocket.Receive(buffer);
    int width = BitConverter.ToInt32(buffer, 0);
    connectSocket.Receive(buffer);
    int height = BitConverter.ToInt32(buffer, 0);
    Console.WriteLine($"Getted width = {width}, getted height = {height}");
    connectSocket.Receive(buffer);
    Random random = new Random();
    bool firstIter = true;
    while (true)
    {
        semaphore.WaitOne();
        if (firstIter)
        {
            Console.WriteLine("Generation cycle start working...\nGeneration two pairs of (x,y) for line...");
            firstIter = false;
        }
        int x = random.Next(20, width - 20);
        int y = random.Next(20, height - 20);
        connectSocket.Send(BitConverter.GetBytes(x));
        connectSocket.Send(BitConverter.GetBytes(y));
        Console.WriteLine($"first pair x = {x}, y = {y}");
        x = random.Next(20, width - 20);
        y = random.Next(20, width - 20);
        Console.WriteLine($"second pair x = {x}, y = {y}");
        connectSocket.Send(BitConverter.GetBytes(x));
        connectSocket.Send(BitConverter.GetBytes(y));
        connectSocket.Receive(buffer);
        semaphore.Release(1);
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
Console.ReadLine();

