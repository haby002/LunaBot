using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

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
            new Engine().RunAsync().GetAwaiter().GetResult();

        }
    }
}
