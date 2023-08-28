// UPDATER

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ChatUpdater
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "invictus chat | UPDATER";
            string text = "> invictus chat | UPDATER <";
            int x = (Console.WindowWidth - text.Length) / 2;
            Console.SetCursorPosition(x, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(text + Environment.NewLine);
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("not enough arguments provided!");
                Environment.Exit(0);
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[~] Createing webclient...");
            using (var webclient = new HttpClient())
            {
                Console.WriteLine("[+] Webclient created!\r\n[~] Getting latest version...");
                string latest = await webclient.GetStringAsync("https://www.cft-devs.xyz/invictus/chat/files/latest");
                Console.WriteLine("[+] Got latest version!\r\n[~] Getting latest file bytes...");
                byte[] fileBytes = await webclient.GetByteArrayAsync($"https://www.cft-devs.xyz/invictus/chat/files/ChatProgram{latest}.exe");
                Console.WriteLine("[+] Got latest file bytes!");
                string current = args[0];
                int processid = Convert.ToInt32(args[1]);
                Console.WriteLine("[~] Killing old process...");
                if ((int)processid > 0)
                {
                    Process.GetProcessById(processid).Kill();
                    Thread.Sleep(1300);
                }
                Console.WriteLine($"[+] Killed old process!\r\n[~] Updating \"ChatProgram.exe\" v{current}...");
                File.WriteAllBytes("ChatProgram.exe", fileBytes);
                Console.WriteLine($"[+] Updated \"ChatProgram.exe\" v{latest}!\r\n[~] Starting new version...");
                Process.Start("ChatProgram.exe");
                Console.WriteLine("[+] Started new version!\r\n[~] Killing this process...");
                Environment.Exit(0);
            }
        }
    }
}
