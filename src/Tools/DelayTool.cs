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
    static class DelayTool
    {
        public static void DelayToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            string indentString = new string(' ', indentLevel * 4 * 0);
            bool showProgress = bool.Parse(step.StepXmlNode.SelectSingleNode(".//*[@Key='IsShow']/@Value").Value);

            XmlNode valuexml = step.StepXmlNode.SelectSingleNode(".//*[@Key='Delay']");
            string value = TextfieldToTxt.Parse(valuexml);

            ThreadLog = "(Delay: " + value + " seconds";
            if (showProgress)
                ThreadLog += ", ShowProgress: True";
            
            ThreadLog += ")";


        }
    }
}
