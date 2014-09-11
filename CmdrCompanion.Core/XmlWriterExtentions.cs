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
    /// Provides utilities for the <see cref="XmlWriter"/> class
    /// </summary>
    public static class XmlWriterExtentions
    {
        /// <summary>
        /// Writes a boolean value in an attribute.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> instance</param>
        /// <param name="localName">Local name of the attribute</param>
        /// <param name="value">The value to write into the attribute</param>
        public static void WriteAttributeBool(this XmlWriter writer, string localName, bool value)
        {
            writer.WriteAttributeString(localName, value ? "1" : "0");
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value into an attribute.
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> instance</param>
        /// <param name="localName">Local name of the attribute</param>
        /// <param name="value">The value to write into the attribute</param>
        public static void WriteAttributeFloat(this XmlWriter writer, string localName, float value)
        {
            writer.WriteAttributeString(localName, value.ToString(NumberFormatInfo.InvariantInfo));
        }
        /// <summary>
        /// Writes an integer into an attribute
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> instance</param>
        /// <param name="localName">Local name of the attribute</param>
        /// <param name="value">The value to write into the attribute</param>
        public static void WriteAttributeInt(this XmlWriter writer, string localName, int value)
        {
            writer.WriteAttributeString(localName, value.ToString(NumberFormatInfo.InvariantInfo));
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> into an attribute
        /// </summary>
        /// <param name="writer">A <see cref="XmlWriter"/> instance</param>
        /// <param name="localName">Local name of the attribute</param>
        /// <param name="value">The value to write into the attribute</param>
        public static void WriteAttributeDate(this XmlWriter writer, string localName, DateTime value)
        {
            writer.WriteAttributeString(localName, value.ToString("O"));
        }
    }
}
