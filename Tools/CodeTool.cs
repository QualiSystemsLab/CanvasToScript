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
    static class CodeTool
    {
        public static void CodeToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            
            string code = step.StepXmlNode.SelectSingleNode(".//*[@Key='Code']/@Value").Value;


            ThreadFunctions = step.StepName.Replace("_x0020_", "_") + "()";
            ThreadFunctions += "\r\n{\r\n" + "/* ------ Matshell code ------ */" + "\r\n";
            ThreadFunctions += "\r\n" + code + "\r\n";
            ThreadFunctions += "/* ---- Matshell code end ---- */" + "\r\n";
            ThreadFunctions += "}\r\n";

        }
    }
}
