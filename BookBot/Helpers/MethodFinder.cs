using BookBot.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Helpers
{
    public class MethodFinder
    {
        public static MethodInfo[] FindMessageMenuHandlers()
        {
            return FindMethods(typeof(MessageMenuHandlerAttribute));
        }

        public static MethodInfo[] FindInlineMenuHandlers()
        {
            return FindMethods(typeof(InlineCallbackHandlerAttribute));
        }

        public static MethodInfo[] FindSlashCommandHandlers()
        {
            return FindMethods(typeof(SlashCommandAttribute));
        }

        public static MethodInfo[] FindMethods(Type type)
        {
            string thisAssemblyName = Assembly.GetExecutingAssembly().GetName().FullName;
            var result = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => x.FullName.ToLower() == thisAssemblyName.ToLower())
                .GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                .Where(m => m.GetCustomAttributes(type, false).Length > 0)
                .ToArray();

            return result;
        }
    }
}
