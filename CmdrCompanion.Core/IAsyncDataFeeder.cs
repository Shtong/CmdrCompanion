using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    public interface IAsyncDataFeeder
    {
        int Start(EliteEnvironment environment);

        Task<bool> Update(EliteEnvironment environment);
    }
}
