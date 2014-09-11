using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CmdrCompanion.Core
{
    public static class XmlWriterExtentions
    {
        public static void WriteAttributeBool(this XmlWriter writer, string localName, bool value)
        {
            writer.WriteAttributeString(localName, value ? "1" : "0");
        }

        public static void WriteAttributeFloat(this XmlWriter writer, string localName, float value)
        {
            writer.WriteAttributeString(localName, value.ToString(NumberFormatInfo.InvariantInfo));
        }
        public static void WriteAttributeInt(this XmlWriter writer, string localName, int value)
        {
            writer.WriteAttributeString(localName, value.ToString(NumberFormatInfo.InvariantInfo));
        }
    }
}
