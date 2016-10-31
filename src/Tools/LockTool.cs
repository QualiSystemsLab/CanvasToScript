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
    static class LockTool
    {
        public static void LockToTxt(ref Step[,] canvasMatrix, int threadCol, int indentLevel, int lockHeight, int lockWidth, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            string indentString = new string(' ', indentLevel * 4);

            XmlNode expression = canvasMatrix[0, 0].StepXmlNode.SelectSingleNode(".//*[@Key='LockExpression']");
            string lockString, dummy;
            ConditionXmlToTxt.ParseCondition(expression, out lockString, out dummy, out dummy, out dummy);

            XmlNode timeoutxml = canvasMatrix[0, 0].StepXmlNode.SelectSingleNode(".//*[@Key='TimeoutExpression']");
            string timeout = "";
            if (timeoutxml != null && timeoutxml.Name != "Null")
            {
                timeout = TextfieldToTxt.Parse(timeoutxml);
            }

            Step[,] threadSteps = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, 1, 0, canvasMatrix.GetLength(0) - 1, canvasMatrix.GetLength(1) - 1);
            string log, functions;
            PartialCanvasToTxt.Analyze(ref threadSteps, threadCol, 0, 0, indentLevel + 1, out log, out functions);


            ThreadLog = "lock (" + lockString + ")";
            if (timeout.Length > 0) ThreadLog += " // Timeout: " + timeout + " seconds";
            ThreadLog += "\r\n" + indentString + "{" + "\r\n";
            ThreadLog += indentString + "    " + log.Trim();
            ThreadLog += "\r\n" + indentString + "}";
            ThreadFunctions = functions;

        }
    }
}
