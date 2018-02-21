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
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            new Engine().RunAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

        }
    }
}
