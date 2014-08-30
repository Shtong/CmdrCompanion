using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// The interface that plugin writers should implement for asynchronous plugins
    /// </summary>
    /// <seealso cref="IDataFeeder"/>
    public interface IAsyncDataFeeder
    {
        /// <summary>
        /// Starts the plugin
        /// </summary>
        /// <param name="environment">The environment the running instance is working with</param>
        /// <returns>The amount of seconds to wait between each call to <see cref="Update"/>. Return zero if calling the update function is not necessary.</returns>
        int Start(EliteEnvironment environment);

        /// <summary>
        /// Method called periodically to asynchronously update the envorinment data.
        /// </summary>
        /// <param name="environment">The current environment</param>
        /// <returns>false if you want to stop regularily calling the Update method, true otherwise.</returns>
        Task<bool> Update(EliteEnvironment environment);
    }
}
