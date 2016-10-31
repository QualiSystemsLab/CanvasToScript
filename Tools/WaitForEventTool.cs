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
    static class WaitForEventTool
    {
        public static void WaitForEventToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            XmlNode varXml = step.StepXmlNode.SelectSingleNode(".//*[@Key='EventExpression']");
            string value = TextfieldToTxt.Parse(varXml);

            ThreadLog = "(Event: " + value + ")";
            


        }
    }
}
