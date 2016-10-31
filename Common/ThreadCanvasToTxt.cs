using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;


namespace CanvasToScript.Common
{
    static class ThreadCanvasToTxt
    {

        public static void Analyze(ref Step[,] canvasMatrix, int[] activeStarts, int mincol, int minrow, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";

            string indentString = new string(' ', indentLevel*4);
            
            //analyze start step
            Step start = canvasMatrix[activeStarts[0],activeStarts[1]];
            
            if (start.Description.Length>0)
                ThreadLog += indentString + "/*" + start.Description + "*/" + "\r\n";

            if (start.ToolName=="StartTool")
                ThreadLog += indentString + "/* " + start.StepName + " (Start) */";
            else
                ThreadLog += indentString + "/* " + start.StepName + " (Finalize) */";

            ThreadLog += "\r\n";

            int curRow = activeStarts[0] + 1;

            Step[,] threadMatrix = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, curRow, activeStarts[1], canvasMatrix.GetLength(0) - 1, canvasMatrix.GetLength(1) - 1);

            string log, functions;
            PartialCanvasToTxt.Analyze(ref threadMatrix, 0, mincol, minrow, indentLevel, out log, out functions);

            ThreadLog += log;
            ThreadFunctions = functions;
        }
    }
}
