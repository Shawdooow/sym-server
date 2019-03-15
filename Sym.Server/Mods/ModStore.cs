using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Logging;

namespace Symcol.Server.Mods
{
    public static class ModStore
    {
        public static Modset GetModset(string name)
        {
            Dictionary<Assembly, Type> loadedAssemblies = new Dictionary<Assembly, Type>();

            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, $"Symcol.Server.Mod.{name}.dll"))
            {
                var filename = Path.GetFileNameWithoutExtension(file);

                if (loadedAssemblies.Values.Any(t => t.Namespace == filename)) return null;

                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    loadedAssemblies[assembly] = assembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(Modset)));
                }
                catch (Exception)
                {
                    Logger.Log("Error loading a modset from a mod file! [filename = " + filename + "]", LoggingTarget.Runtime, LogLevel.Error);
                }
            }

            var instances = loadedAssemblies.Values.Select(g => (Modset)Activator.CreateInstance(g)).ToList();

            try
            {
                return instances.First();
            }
            catch
            {
                return null;
            }
        }
    }
}
