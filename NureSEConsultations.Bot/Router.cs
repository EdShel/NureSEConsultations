using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NureSEConsultations.Bot
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    internal sealed class CommandAttribute : Attribute
    {
        private readonly string command;

        public CommandAttribute(string positionalString)
        {
            this.command = positionalString;
        }

        public string Command
        {
            get { return this.command; }
        }
    }

    public class Router
    {
        private readonly IList<Route> routesActions = new List<Route>();

        private readonly IServiceProvider services;

        private readonly string defaultCommand;

        public string DefaultCommand => this.defaultCommand;

        public Router(IServiceProvider services, string defaultCommand)
        {
            var assembly = GetType().Assembly;
            var controllers = assembly.GetTypes()
                .Where(t => t.IsClass && t.Name.EndsWith("Controller"));
            this.routesActions = controllers.SelectMany(controller => controller.GetMethods()
                .Where(m => Attribute.IsDefined(m, typeof(CommandAttribute)))
                .Select(m =>
                {
                    var commandAttribute = m.GetCustomAttribute<CommandAttribute>(true);
                    return new Route(commandAttribute.Command, m);
                }))
                .OrderByDescending(r => r.Command.Length)
                .ToList();

            this.services = services;
            this.defaultCommand = defaultCommand;
            if (!this.routesActions.Any(r => r.Command == defaultCommand))
            {
                throw new ArgumentException("No default handler found.", nameof(defaultCommand));
            }
        }

        public async Task HandleAsync(string command, object param)
        {
            var handler = this.routesActions.FirstOrDefault(r => r.Command.StartsWith(command));
            if (handler == null)
            {
                handler = this.routesActions.First(r => r.Command == this.defaultCommand);
            }

            var controller = this.services.GetService(handler.Action.DeclaringType);
            await (Task)handler.Action.Invoke(controller, new object[] { param });
        }

        private record Route(string Command, MethodInfo Action);
    }
}
