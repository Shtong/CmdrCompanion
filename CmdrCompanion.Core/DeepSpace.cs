using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// This astronomical object is used as a marker for an arbitrary position in space,
    /// which cannot be located by a physical entity (except for player and AI ships)
    /// </summary>
    public sealed class DeepSpace: AstronomicalObject
    {
        internal DeepSpace(string name, Star s, bool isPositionned, bool isPersistent)
            : base(name, s.Environment, s)
        {
            IsPositionned = isPositionned;
            IsPersistent = isPersistent;
        }

        /// <summary>
        /// When <c>true</c>, means that this represents a precise location in space.
        /// Otherwise, the instance is only used to represent "somewhere" (typically
        /// to represent the position of a player during Hyper Drive)
        /// </summary>
        public bool IsPositionned { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating is this position will stay relevant after the player will leave it.
        /// </summary>
        /// <remarks>
        /// If false, the instance will be removed from the environment shortly after the player leaves it
        /// </remarks>
        public bool IsPersistent { get; private set; }

    }
}
