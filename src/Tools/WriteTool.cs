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
    static class WriteTool
    {
        public static void WriteToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            XmlNode timeoutxml = step.StepXmlNode.SelectSingleNode(".//*[@Key='ExecutionTimeout']");
            string timeout = "";
            if (timeoutxml != null && timeoutxml.Name != "Null")
            {
                timeout = TextfieldToTxt.Parse(timeoutxml);                
            }

            XmlNodeList inputs = step.StepXmlNode.SelectNodes(".//List[@Key='StaticInputs' and  @ElementType='InputProperty']/*");
            XmlNodeList outputs = step.StepXmlNode.SelectNodes(".//List[@Key='StaticOutputs' and  @ElementType='OutputProperty']/*");

            ThreadFunctions = step.StepName.Replace("_x0020_", "_") + "()";
            ThreadFunctions += "\r\n{\r\n" + "/* ------ WriteTool ------ */" + "\r\n" + "\r\n";
            

            for (int i = 0; i < inputs.Count; i++)
            {
                if (bool.Parse(inputs[i].SelectSingleNode(".//Boolean[@Key='IsEnabled']/@Value").Value))
                {
                    XmlNode inputxml = inputs[i].SelectSingleNode("./*[@Key='Value']");
                    string displayName = inputs[i].SelectSingleNode("./*[@Key='DisplayName']/@Value").Value;
                    string input = "null";
                    if (inputxml.Name!="Null")
                        input = TextfieldToTxt.Parse(inputxml);

                    ThreadFunctions += "/* " + displayName + " */" + "\r\n";
                    ThreadFunctions += input + "\r\n\r\n";
                }
            }
            string terminationString = step.StepXmlNode.SelectSingleNode(".//*[@Key='TerminationString']/*[@Key='Value']/@Value").Value;
            ThreadFunctions += "\r\n" + "/* Termination String: " + ConvertTerminationString(terminationString) + " */\r\n";
            string clearReadBuffer = step.StepXmlNode.SelectSingleNode(".//*[@Key='ClearBuffer']/@Value").Value;
            ThreadFunctions += "\r\n" + "/* Clear Read Buffer On Write: " + clearReadBuffer + " */\r\n";
            ThreadFunctions += "}\r\n";

            ThreadLog += "()"; //prototype
            if (timeout.Length > 0) ThreadLog += " // Timeout: " + timeout + " seconds";            
        }

        private static string ConvertTerminationString(string terminationString)
        {
            switch (terminationString)
            {
                case "_x000D_":
                    return "Carriage Return (\\r)";
                case "_x000A_":
                    return "New Line (\\n)";
                case "_x000D__x000A_":
                    return "EOL (\\r\\n)";
                case "_x0009_":
                    return "Tab (\\t)";
                case "_x0020_":
                    return "Space (\\s)";
                case "CTRL_x002B_ENTER":
                    return "None (Ctrl+Enter)";
                default:
                    return "ConvertTerminationString - unhandled termination string: " + terminationString;
            }
        }
    }
}
