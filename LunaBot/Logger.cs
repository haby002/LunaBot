using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaBot
{
    public static class Logger
    {
        public static Task Log(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;

            System.IO.File.WriteAllText(@"log.txt", $"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}\n");

            return Task.CompletedTask;
        }

        public static void Critical(string source, string message, Exception ex = null)
        {
            Log(new LogMessage(LogSeverity.Critical, source, message, ex));
        }

        public static void Error(string source, string message, Exception ex = null)
        {
            Log(new LogMessage(LogSeverity.Error, source, message, ex));
        }

        public static void Warning(string source, string message)
        {
            Log(new LogMessage(LogSeverity.Warning, source, message));
        }

        public static void Verbose(string source, string message)
        {
            Log(new LogMessage(LogSeverity.Verbose, source, message));
        }

        public static void Info(string source, string message)
        {
            Log(new LogMessage(LogSeverity.Info, source, message));
        }

        public static void Debug(string source, string message)
        {
            Log(new LogMessage(LogSeverity.Debug, source, message));
        }
    }
}