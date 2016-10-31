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
    static class SetEventTool
    {
        public static void SetEventToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            
            XmlNode varXml = step.StepXmlNode.SelectSingleNode(".//*[@Key='EventExpression']");
            string value = TextfieldToTxt.Parse(varXml);

            bool setValue = bool.Parse(step.StepXmlNode.SelectSingleNode(".//Boolean[@Key='Set']/@Value").Value);
            
            ThreadLog = "(Event: " + value + ", Turn: " + (setValue? "On":"Off");
            
            ThreadLog += ")";

        }
    }
}
