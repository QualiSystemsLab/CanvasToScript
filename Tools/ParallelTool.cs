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
    static class ParallelTool
    {
        public static void ParallelToTxt(ref Step[,] canvasMatrix, int threadCol, int indentLevel, int pHeight, int pWidth, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            string indentString = new string(' ', indentLevel * 4);

            XmlNode col = canvasMatrix[0, 0].StepXmlNode.SelectSingleNode("List[@Key='Blocks']/StepBlock");
            XmlNodeList cols = col.SelectNodes("Array[@Key='OutputConnectors']//*[@Key='Offset']/@Value");

            var logs = new string[cols.Count];
            for (int i = 0; i < cols.Count; i++)
            {
                Step[,] threadSteps = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, 1, 0, canvasMatrix.GetLength(0) - 1, canvasMatrix.GetLength(1) - 1);
                string functions;
                int column = int.Parse(cols[i].Value);
                PartialCanvasToTxt.Analyze(ref threadSteps, column, 0, 0, indentLevel + 1, out logs[i], out functions);
                ThreadFunctions += "\r\n" + functions;
            }

            for (int i = 0; i < logs.Length; i++)
            {
                ThreadLog += indentString + "Thread " + i.ToString();
                ThreadLog += "\r\n" + indentString + "{" + "\r\n";
                ThreadLog += indentString + "    " + logs[i].Trim();
                ThreadLog += "\r\n" + indentString + "}\r\n";
            }
        }
    }
}
