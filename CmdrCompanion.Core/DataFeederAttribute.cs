using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// An attribute used as a marker for plugin classes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This should be used by plugin writers on classes that implement the <see cref="IDataFeeder"/> 
    /// or <see cref="IAsyncDataFeeder"/> interfaces so that they are recognized and loaded by the
    /// interface when scanning for plugins.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DataFeederAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>
        /// Creates a new instance of the <see cref="DataFeederAttribute"/> class.
        /// </summary>
        /// <param name="name">The unique name of the plugin</param>
        public DataFeederAttribute(string name)
        {
            this._name = name;
        }

        /// <summary>
        /// Gets the unique name of this plugin
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets or sets a short description of what this plugin does.
        /// </summary>
        public string Description { get; set; }
    }
}
