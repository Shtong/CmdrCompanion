using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface.Modules
{
    public sealed class NewLocationEventArgs : EventArgs
    {
        public NewLocationEventArgs(
            string starName,
            string positionDescription,
            bool isInDeepSpace)
        {
            StarName = starName;
            PositionDescription = positionDescription;
            IsInDeepSpace = isInDeepSpace;
        }

        public string StarName { get; private set; }
        public string PositionDescription { get; private set; }

        public bool IsInDeepSpace { get; private set; }
    }
}
