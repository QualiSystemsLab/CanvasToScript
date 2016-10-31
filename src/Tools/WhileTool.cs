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
    static class WhileTool
    {
        public static void WhileToTxt(ref Step[,] canvasMatrix, int threadCol, int indentLevel, int loopHeight, int loopWidth, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            string indentString = new string(' ', indentLevel * 4);

            XmlNode conditions = canvasMatrix[0, 0].StepXmlNode.SelectSingleNode(".//BinaryExpression[@Key='Condition']/*");
            string conditionString, dummy;
            ConditionXmlToTxt.ParseCondition(conditions, out conditionString, out dummy, out dummy, out dummy);

            Step[,] threadSteps = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, 1, 0, canvasMatrix.GetLength(0) - 1, canvasMatrix.GetLength(1) - 1);
            string log, functions;
            PartialCanvasToTxt.Analyze(ref threadSteps, threadCol, 0, 0, indentLevel + 1, out log, out functions);

            bool isDoWhile = bool.Parse(canvasMatrix[0, 0].StepXmlNode.SelectSingleNode(".//Boolean[@Key='IsDoWhile']/@Value").Value);

            if (isDoWhile)
            {
                ThreadLog = "do";
            }
            else
            {
                ThreadLog = "while (" + conditionString + ")";
            }
            
            ThreadLog += "\r\n" + indentString + "{" + "\r\n";
            ThreadLog += indentString + "    " + log.Trim();
            ThreadLog += "\r\n" + indentString + "}";

            if (isDoWhile)
            {
                ThreadLog += " while (" + conditionString + ")";
            }
            
            ThreadFunctions = functions;

        }
    }
}
