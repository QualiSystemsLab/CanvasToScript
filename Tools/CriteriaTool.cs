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
    static class CriteriaTool
    {
        public static void CriteriaToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            XmlNodeList inputs = step.StepXmlNode.SelectNodes(".//CriteriaLogic/InputProperty");
            XmlNodeList outputs = step.StepXmlNode.SelectNodes("//CriteriaLogic/OutputProperty");

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
            string[] outputNames = new string[outputs.Count];

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
                    outputNames[i] = output;
                }
                else
                {
                    outputNames[i] = "{" + displayName + "}";
                }
                if (createVariable == true) prototype += " /*[Create Variable]*/";
                if (saveResults == true) prototype += " /*[Save Results]*/";

                if (i < outputs.Count - 1) prototype += ", ";
            }
            prototype += ")";

            ThreadLog = prototype;

            ThreadFunctions = step.StepName.Replace("_x0020_", "_") + "()";
            ThreadFunctions += "\r\n{\r\n" + "/* ------ CriteriaTool ------ */" + "\r\n" + "\r\n";

            XmlNodeList criterias = step.StepXmlNode.SelectNodes(".//CriteriaLogic/*[@Key='Expression']");
            string assert = "Assert.IsTrue(";
            for (int i = 0; i < criterias.Count; i++)
            {
                string condition, dummy;
                ConditionXmlToTxt.ParseCondition(criterias[i], out condition, out dummy, out dummy, out dummy);

                ThreadFunctions += outputNames[i] + " = " + condition + "\r\n";
                assert += outputNames[i];
                if (i < criterias.Count - 1) assert += " && ";
            }
            assert += ")";
            ThreadFunctions += "\r\n" + assert + "\r\n";


            ThreadFunctions += "\r\n";
            ThreadFunctions += "}\r\n";
        }
    }
}
