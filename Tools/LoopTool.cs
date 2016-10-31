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
    static class LoopTool
    {
        public static void LoopToTxt(ref Step[,] canvasMatrix, int threadCol, int indentLevel, int loopHeight, int loopWidth, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            string indentString = new string(' ', indentLevel * 4);

            XmlNode iterations = canvasMatrix[0, 0].StepXmlNode.SelectSingleNode(".//*[@Key='Iterations']");
            string iterationsCount, dummy;
            ConditionXmlToTxt.ParseCondition(iterations, out iterationsCount, out dummy, out dummy, out dummy);

            Step[,] threadSteps = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, 1, 0, canvasMatrix.GetLength(0) - 1, canvasMatrix.GetLength(1) - 1);
            string log, functions;
            PartialCanvasToTxt.Analyze(ref threadSteps, threadCol, 0, 0, indentLevel + 1, out log, out functions);

            if (canvasMatrix[0, 0].ToolName == "LoopTool")
            {
                ThreadLog = "Loop " + iterationsCount + " times";
                
            }
            else if (canvasMatrix[0, 0].ToolName == "ParaLoopTool")
            {
                ThreadLog = "ParaLoop " + iterationsCount + " branches";
            }
            else
            { 
                ThreadLog = "LoopToTxt - unrecognized tool name: " + canvasMatrix[0, 0].ToolName;
                return;
            }

            ThreadLog += "\r\n" + indentString + "{" + "\r\n";
            ThreadLog += indentString + "    " + log.Trim();
            ThreadLog += "\r\n" + indentString + "}";
            ThreadFunctions = functions;

        }
    }
}
