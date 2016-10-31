using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using CanvasToScript.Common;
using CanvasToScript.Parsers;

namespace CanvasToScript.Tools
{
    static class AttributeTool
    {
        public static void AttributeToTxt(XmlNode step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            XmlNodeList attributes = step.SelectNodes(".//RuntimeAttributeRecordCollection//RuntimeAttributeRecord");
            
            string prototype = "(";
            for (int i = 0; i < attributes.Count; i++)
            {
                string attname = attributes[i].SelectSingleNode(".//*[@Key='AttributeName']/@Value").Value;
                XmlNode valuexml = attributes[i].SelectSingleNode(".//*[@Key='Expression']");
                string input = "null";
                if (valuexml.Name != "Null")
                    input = TextfieldToTxt.Parse(valuexml);

                prototype += attname + ": " + input;
                if (i < attributes.Count - 1) prototype += ", ";
            }

            prototype += ")";

            ThreadLog = prototype;
        }
    }
}
