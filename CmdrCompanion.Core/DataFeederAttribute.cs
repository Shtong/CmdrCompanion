using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DataFeederAttribute : Attribute
    {
        private readonly string _name;

        public DataFeederAttribute(string name)
        {
            this._name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Description { get; set; }
    }
}
