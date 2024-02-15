// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

Console.WriteLine("SubProcess1 started...");
try
{
    byte[] buffer = new byte[4];
    Semaphore semaphore = Semaphore.OpenExisting("Global\\GraphicsSemaphore");
    Console.WriteLine("Semaphore get succesfully...");
    Socket connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    connectSocket.Connect(new IPEndPoint(new IPAddress(new byte[4] {127,0,0,1}),20000));
    Console.WriteLine("Connected succesfully...");
    connectSocket.Receive(buffer);
    int width = BitConverter.ToInt32(buffer, 0);
    connectSocket.Receive(buffer);
    int height = BitConverter.ToInt32(buffer, 0);
    Console.WriteLine($"Getted width = {width}, getted height = {height}");
    semaphore.WaitOne();
    connectSocket.Receive(buffer);
    connectSocket.Send(buffer);
    bool firstIter = true;
    Random random = new Random();
    while (true)
    {
        if (firstIter)
        {
            firstIter = false;
            Console.WriteLine("Generation cycle start working...\nGenerating pairs of (x,y) for rectangle...");
        }
        else
        {
            semaphore.WaitOne();
        }
        int x = random.Next(20, width - 20);
        int y = random.Next(20, height - 20);
        Console.WriteLine($"Generated pairx = {x}, y = {y}");
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

