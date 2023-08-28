// SERVER

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Print
{
    public static void CenterWrite(string text, bool newline = true)
    {
        int x = (Console.WindowWidth - text.Length) / 2;
        Console.SetCursorPosition(x, Console.CursorTop);
        if (newline)
        {
            Console.Write(text + Environment.NewLine);
        }
        else
        {
            Console.Write(text);
        }
    }

    public static void Error(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("!".PadRight(5) + "> " + text);
        Console.ResetColor();
    }
    public static void Success(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("+".PadRight(5) + "> " + text);
        Console.ResetColor();
    }
    public static void Leave(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("-".PadRight(5) + "> " + text);
        Console.ResetColor();
    }
    public static void Logo(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        CenterWrite(text);
        Console.ResetColor();
    }
    public static void Message(string message, string author)
    {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine("MSG".PadRight(5) + $"> {message}".PadRight(90) + $"| {author}");
        Console.ResetColor();
    }
    public static void System(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("SYS".PadRight(5) + "> " + text);
        Console.ResetColor();
    }
}

public class ChatServer
{
    private static List<TcpClient> clients = new List<TcpClient>();
    private static List<string> usernames = new List<string>();

    static void Main(string[] args)
    {
        int port = 646; // changeable
        object ip = IPAddress.Any; // leave this!
        Console.Title = "invictus chat | SERVER";
        Console.Clear();
        Print.Logo("> invictus chat | SERVER <");
        TcpListener server = new TcpListener((IPAddress)ip, port);
        server.Start();
        Print.System("server started");
        Print.System($"ip:port > {(IPAddress)ip}:{port}");
        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
            string username = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Print.Success($"{username} connected");
            clients.Add(client);
            usernames.Add(username);
            Thread thread = new Thread(HandleClient);
            thread.Start(client);
        }
    }

    static void HandleClient(object client)
    {
        TcpClient tcpClient = (TcpClient)client;
        int clientIndex = clients.IndexOf(tcpClient);
        string username = usernames[clientIndex];
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
        int bytesRead;
        while (tcpClient.Connected)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, tcpClient.ReceiveBufferSize);
                if (bytesRead == 0)
                {
                    Print.Leave($"{username} disconnected");
                    if (clients.Contains(tcpClient))
                    {
                        clients.Remove(tcpClient);
                        if (usernames.Count > clientIndex)
                        {
                            usernames.RemoveAt(clientIndex);
                        }
                    }
                    tcpClient.Close();
                    break;
                }
                string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (dataReceived == "usrafkmode: true/1")
                {
                    Print.System($"AFK > {username}");
                    BroadcastMessage($"SYS > {username} is now AFK", tcpClient);
                }
                else
                {
                    Print.Message(dataReceived.ToString(), username.ToString());
                    BroadcastMessage(username + " > " + dataReceived, tcpClient);
                }
            }
            catch (IOException)
            {
                Print.Leave($"{username} disconnected");
                if (clients.Contains(tcpClient))
                {
                    clients.Remove(tcpClient);
                    if (usernames.Count > clientIndex)
                    {
                        usernames.RemoveAt(clientIndex);
                    }
                }
                tcpClient.Close();
                break;
            }
        }
        if (clients.Contains(tcpClient))
        {
            clients.Remove(tcpClient);
            if (usernames.Count > clientIndex)
            {
                usernames.RemoveAt(clientIndex);
            }
        }
        tcpClient.Close();
    }

    static void BroadcastMessage(string message, TcpClient sender)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i] != sender)
            {
                if (clients[i].Connected)
                {
                    NetworkStream stream = clients[i].GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}