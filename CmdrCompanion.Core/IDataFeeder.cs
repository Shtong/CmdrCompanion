using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    public interface IDataFeeder
    {
        int Start(EliteEnvironment environment);

        bool Update(EliteEnvironment environment);
    }
}
