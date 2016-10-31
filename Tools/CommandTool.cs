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
    static class CommandTool
    {
        public static void CommandToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            XmlNode timeoutxml = step.StepXmlNode.SelectSingleNode(".//*[@Key=\"ExecutionTimeout\"]");
            string timeout = "";
            if (timeoutxml!=null && timeoutxml.Name != "Null")
            {
                timeout = TextfieldToTxt.Parse(timeoutxml);                
            }

            XmlNodeList inputs = step.StepXmlNode.SelectNodes(".//List[@Key='StaticInputs' and  @ElementType='InputProperty']/*");
            XmlNodeList outputs = step.StepXmlNode.SelectNodes(".//List[@Key='StaticOutputs' and  @ElementType='OutputProperty']/*");

            ThreadFunctions = step.StepName.Replace("_x0020_", "_") + "()";
            ThreadFunctions += "\r\n{\r\n" + "/* ------ CommandTool ------ */" + "\r\n" + "\r\n";
            

            for (int i = 0; i < inputs.Count; i++)
            {
                XmlNode inputxml = inputs[i].SelectSingleNode("./*[@Key='Value']");
                string displayName = inputs[i].SelectSingleNode("./*[@Key='DisplayName']/@Value").Value;
                string input = "null";
                if (inputxml.Name!="Null")
                    input = TextfieldToTxt.Parse(inputxml);

                ThreadFunctions += "/* " + displayName + " */" + "\r\n";
                ThreadFunctions += input + "\r\n\r\n";
                
            }
            string killontimeout = step.StepXmlNode.SelectSingleNode(".//*[@Key='KillCommandOnTimeout']/@Value").Value;
            ThreadFunctions += "\r\n" + "/* Kill command on timeout: " + killontimeout + " */\r\n";
            ThreadFunctions += "}\r\n";

            
            string prototype = "(";
            for (int i = 0; i < outputs.Count; i++)
            {
                XmlNode outputxml = outputs[i].SelectSingleNode("./*[@Key='Value']");
                string displayName = outputs[i].SelectSingleNode("./*[@Key='DisplayName']/@Value").Value;
                bool createVariable = bool.Parse(outputs[i].SelectSingleNode("./*[@Key='CreateVariable']/@Value").Value);
                bool saveResults = bool.Parse(outputs[i].SelectSingleNode("./*[@Key='SaveResults']/@Value").Value);
                
                string output = "";
                if (outputxml.Name != "Null")
                    output = TextfieldToTxt.Parse(outputxml);

                prototype += "out " + displayName;
                if (output.Length > 0)
                {
                    prototype += ": " + output;
                }
                if (createVariable == true) prototype += " /*[Create Variable]*/";
                if (saveResults == true) prototype += " /*[Save Results]*/";

                if (i < outputs.Count - 1) prototype += ", ";
            }
            prototype += ")";

            if (timeout.Length > 0) prototype += " // Timeout: " + timeout + " seconds";
            ThreadLog = prototype;
        }
    }
}
