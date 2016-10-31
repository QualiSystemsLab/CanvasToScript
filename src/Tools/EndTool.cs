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
    static class EndTool
    {
        public static void EndToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            string indentString = new string(' ', indentLevel * 4 * 0);
            string action = step.StepXmlNode.SelectSingleNode(".//*[@Key='EndKey']/@Value").Value;

            ThreadLog = "(Action: " + action;
            
            ThreadLog += ")";


        }
    }
}
