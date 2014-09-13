using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// Provides utility extensions for XML loading
    /// </summary>
    public static class XmlReaderExtentions
    {
        /// <summary>
        /// Reads the current <see cref="System.Xml.XmlReader.Value"/> as a boolean
        /// </summary>
        /// <param name="reader">The reader to read the boolean from</param>
        /// <returns><c>true</c> if the reader contains the text <c>1</c>, <c>false</c> otherwise</returns>
        public static bool ReadBool(this XmlReader reader)
        {
            return reader.Value == "1";
        }

        /// <summary>
        /// Reads the current <see cref="System.Xml.XmlReader.Value"/> as a float
        /// </summary>
        /// <param name="reader">The reader to read the float from</param>
        /// <returns>The floating number that was found in the reader</returns>
        /// <exception cref="EnvironmentLoadException">The reader did not contain a valid number</exception>
        public static float ReadFloat(this XmlReader reader)
        {
            float result = 0;
            if (!Single.TryParse(reader.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out result))
                throw new EnvironmentLoadException("Invalid float value", reader);
            return result;
        }

        /// <summary>
        /// Reads the current <see cref="System.Xml.XmlReader.Value"/> as an integer
        /// </summary>
        /// <param name="reader">The reader to read the integer from</param>
        /// <returns>The integer that was found in the reader</returns>
        /// <exception cref="EnvironmentLoadException">The reader did not contain a valid number</exception>
        public static int ReadInt(this XmlReader reader)
        {
            int result = 0;
            if (!Int32.TryParse(reader.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
                throw new EnvironmentLoadException("Invalid integer value", reader);
            return result;
        }
    }
}
