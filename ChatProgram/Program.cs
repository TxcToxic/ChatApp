// CLIENT

using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Xml.Linq;

class HWID
{
    public static string GetUUID()
    {
        string UUID = "";
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c wmic csproduct get uuid",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            UUID = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }
        catch (Exception) { }
        return UUID.Split('\n')[1].Trim();
    }
}

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
    public static void Logo(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        CenterWrite(text);
        Console.ResetColor();
    }
    public static void Message(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Magenta;
    }
    public static void System(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("SYS".PadRight(5) + "> " + text);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Magenta;
    }
}

public class ChatClient
{
    static void Logoclear(bool clear = false)
    {
        if (clear)
        {
            Console.Clear();
            Print.Logo("> invictus chat | CLIENT <");
        }
        else
        {
            Print.Logo("> invictus chat | CLIENT <");
        }
    } 

    private static void CheckUpdate()
    {
        WebClient webClient = new WebClient();
        string current = "1.1";
        string latest = webClient.DownloadString("https://www.cft-devs.xyz/invictus/chat/files/latest");
        if (latest != current)
        {
            Print.Error("OUTDATED | THE PROGRAM IS OUTDATED");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Update (Y/N) > ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            string yn = Console.ReadLine();
            if (yn.ToLower().StartsWith("y"))
            {
                string arguments = $"{current} {Convert.ToInt32(Process.GetCurrentProcess().Id)}";
                try
                {
                    Process.Start("ChatUpdater.exe", arguments);
                }
                catch
                {
                    Print.Error("\"ChatUpdater.exe\" was not found | make sure the updater is with this name in the folder");
                }
                while (true)
                {
                    Thread.Sleep(5000);
                }
            } else
            {
                Console.ResetColor();
                Console.WriteLine("Process is getting terminated in 5 seconds...");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
        }
    }

    static void Main(string[] args)
    {
        Console.Title = "invictus chat | CLIENT";
        Logoclear(true);
        string username;
        bool afk = false;
        int port = 646;
        CheckUpdate();
        TcpClient client = new TcpClient();
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("name".PadRight(5) + "> ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            username = Console.ReadLine();
            Console.ResetColor();
            if (username.Length > 20)
            {
                Print.Error("username is too long | max length is 20");
            }
            else if (username.Length < 3)
            {
                Print.Error("username is too short | min length is 3");
            }
            else if (string.IsNullOrWhiteSpace(username))
            {
                Print.Error("your username cannot be empty");
            }
            else if (username.ToLower().Contains("toxic") || username.ToLower().Contains("txc"))
            {
                Print.System("getting hwid");
                string hwid = HWID.GetUUID();
                Print.System("got hwid");
                Print.System("checking hwid");
                if (hwid == "828FDCDC-7F12-BA4F-6820-A85E4555D57F")
                {
                    Print.Success($"welcome {username}");
                    break;
                }
                else
                {
                    Print.Error("failed | you're not the owner");
                }
            }
            else if (username.ToLower().Contains("sys") || username.ToLower().Contains("system"))
            {
                Print.Error("failed | you're not able to send messages as \"SYSTEM\"");
            }
            else if (username.ToLower().Contains("invictus"))
            {
                Print.Error("failed | you're not able to send messages as \"invictus\"");
            }
            else if (Encoding.UTF8.GetByteCount(username) != username.Length)
            {
                Print.Error("failed | you're not able to use non-utf8-characters in your name");
            }
            else
            {
                break;
            }
        }
        Console.Title = $"chatting as \"{username}\" | establishing connection";
        while (true)
        {
            try
            {
                client.Connect(IPAddress.Parse("45.142.107.217"), port);
                Print.Success("connection established | you can start chatting");
                break;
            }
            catch (SocketException)
            {
                try
                {
                    client.Connect("127.0.0.1", port);
                    Print.Success("connection established | you can start chatting");
                    break;
                }
                catch { }
                Print.Error("connection failed | 10s cooldown");
                Thread.Sleep(10000);
            }
        }
        Console.Title = $"chatting as \"{username}\" | connected";
        NetworkStream stream = client.GetStream();
        byte[] buffer = Encoding.UTF8.GetBytes(username);
        stream.Write(buffer, 0, buffer.Length);
        Thread thread = new Thread(ReadMessages);
        thread.Start(client);
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            string message = Console.ReadLine();
            Console.ResetColor();
            if (afk)
            {
                Print.System("you're no longer afk");
                afk = false;
            }
            if (message.StartsWith("/"))
            {
                if (message.StartsWith("/help"))
                {
                    Print.System(Environment.NewLine + "/help".PadRight(10) + "- prints this text" + Environment.NewLine + "/clear".PadRight(10) + "- clears the window" + Environment.NewLine + "/quit".PadRight(10) + "- quits the program" + Environment.NewLine + "/afk".PadRight(10) + "- sets you afk" + Environment.NewLine);
                }
                else if (message.StartsWith("/clear"))
                {
                    Logoclear(true);
                }
                else if (message.StartsWith("/quit"))
                {
                    Print.System("quitting in 5 seconds...");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                }
                else if (message.StartsWith("/afk"))
                {
                    if (!afk)
                    {
                        Print.Success("the users will get a notification that you're afk");
                        buffer = Encoding.UTF8.GetBytes("usrafkmode: true/1");
                        stream.Write(buffer, 0, buffer.Length);
                        afk = true;
                    }
                }
                else
                {
                    Print.Error("unknown command | command was not found");
                }
            }
            else
            {
                if (message.Length > 80)
                {
                    Print.Error("message is too long | max length is 80");
                }
                else if (string.IsNullOrWhiteSpace(message))
                {
                    Print.Error("you cannot send empty messages");
                } else if (Encoding.UTF8.GetByteCount(message) == message.Length)
                {
                    Print.Error("invalid characters | our system only accept utf8 characters");
                }
                else
                {
                    buffer = Encoding.UTF8.GetBytes(message);
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }

    static void ReadMessages(object client)
    {
        try
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream stream = tcpClient.GetStream();
            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            int bytesRead;
            while (true)
            {
                bytesRead = stream.Read(buffer, 0, tcpClient.ReceiveBufferSize);
                if (bytesRead == 0)
                {
                    Print.Error("an error occured try restarting the client or wait for the server is available again!");
                    Thread.Sleep(10000);
                    Environment.Exit(0);
                }
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (message.StartsWith("SYS > "))
                {
                    string afkmess = message.Replace("SYS > ", "");
                    Print.System(afkmess);
                }
                else
                {
                    Print.Message(message);
                }
            }
        }
        catch
        {
            Print.Error("an error occured try restarting the client or wait for the server is available again!");
            Thread.Sleep(10000);
            Environment.Exit(0);
        }
    }
}