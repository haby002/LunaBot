using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections;
using System.Collections.Generic;

namespace LunaBot
{
    public class Program
    {
        // Used for commands
        private readonly IServiceCollection _map = new ServiceCollection();
        private readonly CommandService _commands = new CommandService();

        // Start in an async context
        static void Main(string[] args)
        {
            new Engine().Run().GetAwaiter().GetResult();
        }
    }
}
