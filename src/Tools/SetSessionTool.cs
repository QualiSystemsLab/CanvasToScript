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
    static class SetSessionTool
    {
        public static void SessionToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            XmlNodeList parameters = step.StepXmlNode.SelectNodes(".//List[@Key='StartSessionParameters']/*");
            XmlNode sessionType = step.StepXmlNode.SelectSingleNode(".//*[@Key='SessionType']/@Value");
            XmlNode connectAction = step.StepXmlNode.SelectSingleNode(".//*[@Key='ConnectAction']/@Value");
            XmlNode sessionName = step.StepXmlNode.SelectSingleNode(".//*[@Key='SessionName']");

            string sessionNameTxt = TextfieldToTxt.Parse(sessionName);
            string prototype = "(";
            prototype += "SessionName: " + sessionNameTxt + ", " +
                         "SessionType: " + sessionType.Value + ", " +
                         "ConnectAction: " + connectAction.Value;

            if (parameters.Count > 0)
            {
                prototype += ", ";
                foreach (XmlNode param in parameters)
                {
                    string conditionTxt, dummy;
                    ConditionXmlToTxt.ParseCondition(param, out conditionTxt, out dummy, out dummy, out dummy);
                    prototype += conditionTxt;
                }
            }

            XmlNodeList connectionInfoElements = step.StepXmlNode.SelectNodes(".//*[@Key='SessionConnectionInfo']/*");
            foreach (XmlNode element in connectionInfoElements)
            {
                string name = element.Attributes["Key"].Value;
                if (name == "NamedParameters")
                {
                    if (element.ChildNodes.Count > 0)
                    {
                        prototype += ", NamedParameters: [";
                        int e = 0;
                        foreach (XmlNode nparam in element.ChildNodes)
                        {
                            string value = TextfieldToTxt.Parse(nparam);
                            prototype += value;
                            
                            if (e < element.ChildNodes.Count - 1) prototype += ", ";

                            e++;
                        }
                        prototype += "]";
                    }
                }
                else
                {
                    string value = TextfieldToTxt.Parse(element);
                    prototype += ", " + name + ": " + value;
                }
            }

            prototype += ")";

            ThreadLog = prototype;

        }
    }
}
