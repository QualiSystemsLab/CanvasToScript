﻿using System;
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
    static class ReadTool
    {
        public static void ReadToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            XmlNode timeoutxml = step.StepXmlNode.SelectSingleNode(".//*[@Key='ExecutionTimeout']");
            string timeout = "";
            if (timeoutxml != null && timeoutxml.Name != "Null")
            {
                timeout = TextfieldToTxt.Parse(timeoutxml);                
            }

            //XmlNodeList inputs = step.StepXmlNode.SelectNodes(".//List[@Key='StaticInputs' and  @ElementType='InputProperty']/*");
            XmlNodeList outputs = step.StepXmlNode.SelectNodes(".//List[@Key='StaticOutputs' and  @ElementType='OutputProperty']/*");

            ThreadFunctions = step.StepName.Replace("_x0020_", "_") + "()";
            ThreadFunctions += "\r\n{\r\n" + "/* ------ ReadTool ------ */" + "\r\n" + "\r\n";

            XmlNode readToolInfo = step.StepXmlNode.SelectSingleNode(".//ReadToolInfo");
            string terminationType = readToolInfo.SelectSingleNode(".//TerminationType[@Key='TerminationType']/@Value").Value;
            XmlNode expression;
            switch (terminationType)
            {
                case "StepDuration":
                    expression = readToolInfo.SelectSingleNode(".//*[@Key='StepDuration']");
                    ThreadFunctions += "/* Step Duration: " + TextfieldToTxt.Parse(expression) + " seconds */" + "\r\n";
                    break;
                case "FixedLength":
                    expression = readToolInfo.SelectSingleNode(".//*[@Key='FixedLength']");
                    ThreadFunctions += "/* Fixed Length: " + TextfieldToTxt.Parse(expression) + " chars */" + "\r\n";
                    break;
                case "Text":
                    expression = readToolInfo.SelectSingleNode(".//*[@Key='TerminationString']");
                    ThreadFunctions += "/* Termination Text */" + "\r\n" + TextfieldToTxt.Parse(expression) + "\r\n";
                    ThreadFunctions += "/* Remove match from output: " + readToolInfo.SelectSingleNode(".//Boolean[@Key='RemoveMatchFromOutput']/@Value").Value + " */" + "\r\n";
                    break;
                case "RegularExpression":
                    expression = readToolInfo.SelectSingleNode(".//*[@Key='TerminationString']");
                    ThreadFunctions += "/* Termination Regex */" + "\r\n" + TextfieldToTxt.Parse(expression) + "\r\n";
                    ThreadFunctions += "/* Remove match from output: " + readToolInfo.SelectSingleNode(".//Boolean[@Key='RemoveMatchFromOutput']/@Value").Value + " */" + "\r\n";
                    break;
                case "PatternSet":
                    expression = readToolInfo.SelectSingleNode(".//*[@Key='PatternName']");
                    ThreadFunctions += "/* Pattern Name: " + TextfieldToTxt.Parse(expression) + " */" + "\r\n";
                    ThreadFunctions += "/* Remove match from output: " + readToolInfo.SelectSingleNode(".//Boolean[@Key='RemoveMatchFromOutput']/@Value").Value + " */" + "\r\n";
                    break;
                case "TimeoutAfterLastInput":
                    expression = readToolInfo.SelectSingleNode(".//*[@Key='TimeoutAfterLastInput']");
                    ThreadFunctions += "/* Idle Timeout: " + TextfieldToTxt.Parse(expression) + " seconds */" + "\r\n";
                    break;
            }

            
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
