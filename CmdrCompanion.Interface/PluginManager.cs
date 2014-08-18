using CmdrCompanion.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface
{
    internal class PluginManager
    {
        public PluginManager()
        {
            AvailablePlugins = new List<PluginContainer>();
        }

        public List<PluginContainer> AvailablePlugins { get; private set; }

        public void ScanForPlugins()
        {
            string folder = Path.Combine(Environment.CurrentDirectory, "plugins");

            if (!Directory.Exists(folder))
                return;

            Type pluginType = typeof(IDataFeeder);
            Type asyncPlugintype = typeof(IAsyncDataFeeder);

            foreach(string s in Directory.EnumerateFiles(folder, "*.dll"))
            {
                try
                {
                    Assembly roAsm = Assembly.LoadFrom(s);

                    foreach(Type t in roAsm.GetExportedTypes().Where(t => pluginType.IsAssignableFrom(t) || asyncPlugintype.IsAssignableFrom(t)))
                    {
                        DataFeederAttribute feederAttribute = t.GetCustomAttribute<DataFeederAttribute>(false);
                        if (feederAttribute == null)
                            continue;

                        AvailablePlugins.Add(new PluginContainer()
                        {
                            AssemblyName = roAsm.FullName,
                            AssemblyPath = s,
                            TypeName = t.FullName,
                            Name = feederAttribute.Name,
                            Description = feederAttribute.Description,
                            IsAsync = asyncPlugintype.IsAssignableFrom(t),
                        });
                    }
                }
                catch(FileLoadException ex)
                {
                    Trace.TraceWarning("Could not load plugin assembly {0}: {1}", s, ex.ToString());
                }
                catch(BadImageFormatException ex)
                {
                    Trace.TraceWarning(ex.ToString());
                }
                catch(SecurityException ex)
                {
                    Trace.TraceWarning(ex.ToString());
                }
            }
        }
    }
}
