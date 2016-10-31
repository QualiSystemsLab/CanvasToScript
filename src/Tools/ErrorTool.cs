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
    static class ErrorTool
    {
        public static void ErrorToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            string indentString = new string(' ', indentLevel * 4 * 0);

            string name = TextfieldToTxt.Parse(step.StepXmlNode.SelectSingleNode(".//ErrorInfo/*[@Key='ErrorName']"));
            string description = TextfieldToTxt.Parse(step.StepXmlNode.SelectSingleNode(".//ErrorInfo/*[@Key='ErrorDescription']"));

            XmlNodeList parameters = step.StepXmlNode.SelectNodes(".//List[@Key='ErrorParameters']/*");
            ThreadLog = "(Name: " + name + ", Description: " + description;

            foreach (XmlNode param in parameters)
            {
                string pname = param.SelectSingleNode(".//*[@Key='Name']/@Value").Value;
                string pvalue = TextfieldToTxt.Parse(param.SelectSingleNode(".//*[@Key='Value']"));
                ThreadLog += ", " + pname + ": " + pvalue;
            }

            string action = step.StepXmlNode.SelectSingleNode(".//Scope/@Value").Value;
            ThreadLog += ", End: " + action;
            
            ThreadLog += ")";

        }
    }
}
