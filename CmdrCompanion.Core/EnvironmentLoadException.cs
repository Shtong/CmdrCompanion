using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// Exception thrown when a problem occurs while loading an <see cref="EliteEnvironment"/> save file
    /// </summary>
    public class EnvironmentLoadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentLoadException"/>
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="reader">The reader positionned at the place the error occured</param>
        public EnvironmentLoadException(string message, XmlReader reader)
            : this(message, (IXmlLineInfo)reader)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentLoadException"/>
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="lineInfo">information about the position of the error in the save file</param>
        public EnvironmentLoadException(string message, IXmlLineInfo lineInfo) 
            : base(String.Format("{0} (line {1}, char {2})", message, lineInfo.LineNumber, lineInfo.LinePosition))
        {
            LineNumber = lineInfo.LineNumber;
            LinePosition = lineInfo.LinePosition;
        }

        /// <summary>
        /// Gets the line number where the problem occured in the save file
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Gets the column number where the problem occured in the save file
        /// </summary>
        public int LinePosition { get; private set; }
    }
}
