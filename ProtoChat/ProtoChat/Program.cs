using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static TcpListener serverListener;
    public static List<ClientHandler> clients = new List<ClientHandler>();

    static void Main(string[] args)
    {
        Console.WriteLine("PROTOCHAT");
        Console.WriteLine();
        Console.WriteLine("Choose an option:");
        Console.WriteLine("1. Start as Server");
        Console.WriteLine("2. Start as Client");
        Console.WriteLine("3. Exit");
        Console.WriteLine();
        Console.WriteLine("Made by LukeW-Developer");
        Console.WriteLine("Note: This is a Chat System that I designed for enhancing my C# programming skills, expect bugs.");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                StartServer();
                break;

            case "2":
                StartClient();
                break;

            case "3":
                return;

            default:
                Console.WriteLine("Invalid option. Please choose 1, 2, or 3.");
                break;
        }
    }

    static void StartServer()
    {
        Console.Write("Enter the port to listen on: ");
        int port = int.Parse(Console.ReadLine());

        serverListener = new TcpListener(IPAddress.Any, port);
        serverListener.Start();
        Console.WriteLine($"Server Started on Port {port}");

        while (true)
        {
            TcpClient clientSocket = serverListener.AcceptTcpClient();
            ClientHandler clientHandler = new ClientHandler(clientSocket);
            clients.Add(clientHandler);
            Thread clientThread = new Thread(clientHandler.HandleClient);
            clientThread.Start();
        }
    }

    static void StartClient()
    {
        Console.Write("Enter the server IP address: ");
        string serverIpAddress = Console.ReadLine();

        Console.Write("Enter the server port to connect to: ");
        int port = int.Parse(Console.ReadLine());

        Console.Write("Enter your name: ");
        string clientName = Console.ReadLine();

        TcpClient client = new TcpClient();

        try
        {
            client.Connect(serverIpAddress, port);
            Console.WriteLine($"Connected to Server at {serverIpAddress}:{port}");

            ClientHandler clientHandler = new ClientHandler(client, clientName);
            Thread clientThread = new Thread(clientHandler.HandleClient);
            clientThread.Start();

            while (true)
            {
                string clientMessage = Console.ReadLine();
                clientHandler.SendMessage(clientMessage);
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Failed to connect to the server.");
        }
    }
}

class ClientHandler
{
    private TcpClient client;
    private string name;
    private StreamReader reader;
    private StreamWriter writer;

    public ClientHandler(TcpClient client, string name = "Client")
    {
        this.client = client;
        this.name = name;
        NetworkStream networkStream = client.GetStream();
        reader = new StreamReader(networkStream);
        writer = new StreamWriter(networkStream);
    }

    public void HandleClient()
    {
        try
        {
            while (true)
            {
                string message = reader.ReadLine();
                if (message == null)
                    break;

                Console.WriteLine(message);
                BroadcastMessage(message);
            }
        }
        catch (IOException)
        {
        }
        finally
        {
            reader.Close();
            writer.Close();
            client.Close();
        }
    }

    public void SendMessage(string message)
    {
        try
        {
            if (!message.StartsWith(name + ": "))
            {
                message = $"{name}: {message}";
            }

            writer.WriteLine(message);
            writer.Flush();
        }
        catch (IOException)
        {
        }
    }

    private void BroadcastMessage(string message)
    {
        foreach (ClientHandler clientHandler in Program.clients)
        {
            try
            {
                clientHandler.writer.WriteLine(message);
                clientHandler.writer.Flush();
            }
            catch (IOException)
            {
            }
        }
    }
}
