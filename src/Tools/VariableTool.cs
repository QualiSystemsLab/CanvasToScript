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
    static class VariableTool
    {
        public static void SetVariableToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            string indentString = new string(' ', indentLevel * 4 * 0);
            bool assignFromValue = bool.Parse(step.StepXmlNode.SelectSingleNode(".//Boolean[@Key='AssignFromValue']/@Value").Value);

            XmlNode variablexml = step.StepXmlNode.SelectSingleNode(".//*[@Key='VariableNameTextSource']");
            
            string varname = TextfieldToTxt.Parse(variablexml);

            if (assignFromValue)
            {
                XmlNode valuexml = step.StepXmlNode.SelectSingleNode(".//*[@Key='VariableValue']");
                string datatype = step.StepXmlNode.SelectSingleNode(".//*[@Key='VariableType']/@Value").Value;
                string dimension = step.StepXmlNode.SelectSingleNode(".//*[@Key='VariableDimension']/@Value").Value;
                string value = TextfieldToTxt.Parse(valuexml);

                if (datatype=="String" && dimension=="Scalar")
                    ThreadLog = indentString + varname + " = \"" + value + "\"";
                else
                    ThreadLog = indentString + varname + " = " + value;
                
            }
            else
            {
                XmlNode expression = step.StepXmlNode.SelectSingleNode(".//*[@Key='Expression']");            
                string value = TextfieldToTxt.Parse(expression);
                if (value.Contains("_x000D__x000A_"))
                {
                    ThreadLog = indentString + varname + " = " + step.StepName.Replace("_x0020_", "_") + "()";
                    ThreadFunctions = step.StepName.Replace("_x0020_", "_") + "()" + "\r\n" + "{\r\n" + value + "\r\n}\r\n";
                }
                else
                {
                    ThreadLog = indentString + varname + " = " + value;
                }
            }

        }
    }
}
