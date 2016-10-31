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
    static class PassFailTool
    {
        public static void PassFailToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            ThreadFunctions = step.StepName.Replace("_x0020_", "_") + "()";

            string tooltype = step.StepXmlNode.SelectSingleNode(".//*[@Key='NotificationType']/@Value").Value;
            string toolname = "PassFailTool";
            switch (tooltype)
            {
                case "0":
                    toolname = "Pass";
                    break;
                case "1":
                    toolname = "Fail";
                    break;
                case "2":
                    toolname = "Text To Report";
                    break;
                default:
                    break;
            }

            ThreadFunctions += "\r\n{\r\n" + "/* ------ " + toolname + " Tool ------ */" + "\r\n" + "\r\n";
            
            //text
            XmlNode messageXml = step.StepXmlNode.SelectSingleNode(".//*[@Key='DescriptionTextSource']");
            ThreadFunctions += "/* " + "Text" + " */" + "\r\n";
            ThreadFunctions += TextfieldToTxt.Parse(messageXml) + "\r\n";

            //attachment
            if (bool.Parse(step.StepXmlNode.SelectSingleNode(".//FileAttachment[@Key='Attachment']/Boolean[@Key='IsAttached']/@Value").Value))
            {
                XmlNode fileCaption = step.StepXmlNode.SelectSingleNode(".//FileAttachment[@Key='Attachment']/*[@Key='FileCaption']");
                ThreadFunctions += "/* " + "File Caption: " + " */" + "\r\n" + TextfieldToTxt.Parse(fileCaption) + "\r\n";
                XmlNode filePath = step.StepXmlNode.SelectSingleNode(".//FileAttachment[@Key='Attachment']/*[@Key='FilePath']");
                ThreadFunctions += "/* " + "File Path: " + " */" + "\r\n" + TextfieldToTxt.Parse(filePath) + "\r\n";                
            }
            
            ThreadFunctions += "}\r\n";

            ThreadLog += "()"; //prototype
                       
        }

    }
}
