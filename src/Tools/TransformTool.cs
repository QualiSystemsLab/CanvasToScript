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
    static class TransformTool
    {
        public static void TransformToTxt(Step step, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            XmlNodeList inputs = step.StepXmlNode.SelectNodes(".//List[@Key='Inputs' and  @ElementType='InputProperty']/*");
            XmlNodeList outputs = step.StepXmlNode.SelectNodes(".//List[@Key='Outputs' and  @ElementType='OutputProperty']/*");

            string prototype = "(";
            for (int i = 0; i < inputs.Count; i++)
            {
                XmlNode inputxml = inputs[i].SelectSingleNode("./*[@Key='Value']");
                string displayName = inputs[i].SelectSingleNode("./*[@Key='DisplayName']/@Value").Value;
                string input = "null";
                if (inputxml.Name != "Null")
                    input = TextfieldToTxt.Parse(inputxml);

                prototype += displayName + ": " + input;
                if (i < inputs.Count - 1) prototype += ", ";
            }

            if (outputs.Count > 0) prototype += ", ";

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
            ThreadLog = prototype;

            ThreadFunctions = step.StepName.Replace("_x0020_", "_") + "()" + "\r\n" + "{";
            ThreadFunctions += "\r\n" + "/* ------ Transformation Flows ------ */" + "\r\n";

            XmlNodeList transformFlows = step.StepXmlNode.SelectNodes(".//TransformationFlow");
            int f=0;
            foreach (XmlNode flow in transformFlows)
            {
                string flowTxt;
                int inputIdx, outputIdx;
                TransformationFlowToTxt(flow, out inputIdx, out outputIdx, out flowTxt);
                ThreadFunctions += "/* Flow " + f.ToString() + " (";
                ThreadFunctions += "Input: " + inputs[inputIdx].SelectSingleNode("./*[@Key='DisplayName']/@Value").Value + ", ";
                ThreadFunctions += "Output: " + outputs[outputIdx].SelectSingleNode("./*[@Key='DisplayName']/@Value").Value + ") */" + "\r\n";
                ThreadFunctions += flowTxt;
                ThreadFunctions += "\r\n" + "\r\n";


                f++;
            }

            ThreadFunctions += "\r\n" + "/*------ Transformation Flows End ------ */" + "\r\n" + "}" + "\r\n\r\n";

        }

        private static void TransformationFlowToTxt(XmlNode flow, out int inputIdx, out int outputIdx, out string flowTxt)
        {
            flowTxt = "";

            inputIdx = int.Parse(flow.SelectSingleNode(".//Int32[@Key='InputIndex']/@Value").Value);
            outputIdx = int.Parse(flow.SelectSingleNode(".//Int32[@Key='OutputIndex']/@Value").Value);

            XmlNodeList transforms = flow.SelectNodes(".//Array[@Key='Transformations']/*");
            int step = 0;
            foreach (XmlNode transform in transforms)
            {
                flowTxt += "/* Step " + step.ToString() + " */" + "\r\n";
                XmlNodeList parameters = transform.SelectNodes(".//List[@Key='Parameters']/*");
                string transformName = transform.SelectSingleNode(".//*[@Key='Name']/@Value").Value;
                if (parameters.Count > 0)
                {
                    flowTxt += transformName + "(";
                    int p=0;
                    foreach (XmlNode parameter in parameters)
                    {
                        string pName = parameter.SelectSingleNode(".//*[@Key='Name']/@Value").Value;
                        string ptxt = TextfieldToTxt.Parse(parameter.SelectSingleNode(".//*[@Key='Value']"));
                        flowTxt += pName + ": " + ptxt + (p < parameters.Count - 1 ? ", " : "");
                        p++;
                    }
                    flowTxt += ")\r\n";
                }
                else
                {
                    flowTxt += transformName + "\r\n";
                }
                step++;
            }
        }
    }
}
