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
    static class ScripterTool
    {
        public static void ScripterToTxt(XmlNode step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            XmlNode timeoutxml = step.SelectSingleNode(".//*[@Key='ExecutionTimeout']");
            string timeout = "";
            if (timeoutxml != null && timeoutxml.Name != "Null")
            {
                timeout = TextfieldToTxt.Parse(timeoutxml);                
            }

            XmlNodeList inputs = step.SelectNodes(".//Dictionary[@Key='Parameters']//*[@ElementType='InputProperty']/*");
            XmlNodeList outputs = step.SelectNodes(".//List[@Key='StaticOutputs' and  @ElementType='OutputProperty']/*");

            string prototype = "(";
            for (int i = 0; i < inputs.Count; i++)
            {
                XmlNode inputxml = inputs[i].SelectSingleNode("./*[@Key='Value']");
                string displayName = inputs[i].SelectSingleNode("./*[@Key='DisplayName']/@Value").Value;
                string input = "null";
                if (inputxml.Name!="Null")
                    input = TextfieldToTxt.Parse(inputxml);

                prototype += displayName + ": " + input;
                if (i < inputs.Count - 1) prototype += ", ";
            }

            if (inputs.Count>0 && outputs.Count > 0) prototype += ", ";

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

            if (timeout.Length > 0) prototype += " // Timeout: " + timeout;
            ThreadLog = prototype;
        }
    }
}
