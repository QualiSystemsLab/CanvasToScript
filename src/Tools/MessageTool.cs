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
    static class MessageTool
    {
        public static void MessageToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
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
            
            ThreadFunctions = step.StepName.Replace("_x0020_", "_") + "()";
            ThreadFunctions += "\r\n{\r\n" + "/* ------ MessageTool ------ */" + "\r\n" + "\r\n";
            
            //message
            XmlNode messageXml = step.StepXmlNode.SelectSingleNode(".//List[@Key='StaticInputs' and  @ElementType='InputProperty']/InputProperty/String[@Key='Name' and @Value='Message']/../*[@Key='Value']");
            ThreadFunctions += "/* " + "Message" + " */" + "\r\n";
            ThreadFunctions += TextfieldToTxt.Parse(messageXml) + "\r\n";

            //timeout
            if (bool.Parse(step.StepXmlNode.SelectSingleNode(".//Boolean[@Key='UseTimeout']/@Value").Value))
            {
                XmlNode timeoutXml = step.StepXmlNode.SelectSingleNode(".//List[@Key='StaticInputs' and  @ElementType='InputProperty']/InputProperty/String[@Key='Name' and @Value='Timeout']/../*[@Key='Value']");
                ThreadFunctions += "/* " + "Duration: " + TextfieldToTxt.Parse(timeoutXml) + " seconds */" + "\r\n";                
            }
            
            ThreadFunctions += "}\r\n";

            ThreadLog += "()"; //prototype
            if (timeout.Length > 0) ThreadLog += " // Timeout: " + timeout + " seconds";            
        }

    }
}
