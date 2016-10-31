using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using CanvasToScript.Parsers;

namespace CanvasToScript.Common
{
    static class CanvasAnalyzer
    {
        public static void StepsListToTxt(XmlNodeList steps, bool isMiniCanvas, int indentLevel, XmlNode ehstep, out string ThreadLogs, out string ThreadFunctions)
        {
            ThreadLogs = "";
            int minrow, mincol, maxrow, maxcol;
            Step[,] canvasMatrix = new Step[2000, 2000];
            List<int[]> activeStarts = new List<int[]>();
            minrow = mincol = 100000;
            maxrow = maxcol = -1;
            int[] finalize = null;
            foreach (XmlNode astep in steps)
            {
                int row, col;
                XmlNode step;

                if (astep.Name == "List")
                    step = astep.FirstChild;
                else
                    step = astep;

                Step curStep = new Step(step);
                
                if (step.ToString().StartsWith("<ContainerStep"))
                {
                    row = curStep.Rows[curStep.Rows.Length - 1];
                    col = curStep.Cols[curStep.Cols.Length - 1];
                }
                else
                {
                    row = curStep.Rows[0];
                    col = curStep.Cols[0];
                }

                row += 500; col += 500;

                canvasMatrix[row, col] = curStep;

                if (row < minrow) minrow = row;
                if (col < mincol) mincol = col;
                if (row > maxrow) maxrow = row;
                if (col > maxcol) maxcol = col;
                
                if (curStep.Enabled)
                {
                    if (curStep.ToolName == "StartTool")
                    {
                        int[] start = new int[] { row, col, (curStep.ToolName == "StartTool") ? 0 : 1 };
                        activeStarts.Add(start);
                    }
                    else if (curStep.ToolName == "FinalizeTool")
                    {
                        finalize = new int[] { row, col, (curStep.ToolName == "StartTool") ? 0 : 1 };
                    }
                }
            }
            if (finalize != null) activeStarts.Add(finalize);

            canvasMatrix = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, minrow, mincol, maxrow, maxcol);

            for (int start = 0;start<activeStarts.Count;start++)
            {
	            activeStarts[start][0] -= minrow;
                activeStarts[start][1] -= mincol;
            }

            string eh = ErrorHandlingToTxt.Parse(ehstep, indentLevel+1);
            string[] logs = new string[activeStarts.Count];
            string[] functions = new string[activeStarts.Count];
            
            
            for (int start = 0; start < activeStarts.Count; start++)
            {
                ThreadCanvasToTxt.Analyze(ref canvasMatrix, activeStarts[start], mincol, minrow, indentLevel+1, out logs[start], out functions[start]);
                if (isMiniCanvas)
                {
                    logs[start] =  "\r\n" + logs[start] + "\r\n"; 
                }
                else
                {
                    logs[start] = "\r\n#region /* ------ Thread Start ------ */" + "\r\n" + logs[start] + "\r\n#endregion" + " /* ------- Thread End ------- */" + "\r\n"; 
                }
            }

            int frow=-1;
            for(int row=0; row<logs.Length; row++)
            {
                if (logs[row].Contains("(Finalize)"))
                    frow=row;
                else
                    ThreadLogs += logs[row] + (row < logs.Length-1 ? "\r\n\r\n" : "");
            }
            if (frow >= 0)
            {
                ThreadLogs += logs[frow].Replace("----- Thread ", "----- Finalize ") + "\r\n\r\n";
            }

            if (eh.Length > 0)
            {
                string indentString = new string(' ', indentLevel * 4);
                string indentStringIn = new string(' ', (indentLevel + 1) * 4);
                if (isMiniCanvas)
                {
                    ThreadLogs = "{" + "\r\n" + indentStringIn + "try" + "\r\n" + indentStringIn + "{" + ThreadLogs.Replace("\r\n", "\r\n    ") + "\r\n" + indentStringIn + "}" + "\r\n" + eh + indentString + "}\r\n";
                }
                else
                {
                    ThreadLogs = "{" + "\r\n" + indentStringIn + "try" + "\r\n" + indentStringIn + "{" + ThreadLogs.Replace("\r\n", "\r\n    ") + "\r\n" + indentStringIn + "}" + "\r\n" + eh + "}\r\n";
                }                
            }
            else
            {
                string indentString = new string(' ', indentLevel * 4);
                if (isMiniCanvas)
                {
                    ThreadLogs = "{" + ThreadLogs + indentString + "}\r\n";
                }
                else
                {
                    ThreadLogs = "{" + ThreadLogs + "}\r\n";
                }
            }
            
            ThreadFunctions = string.Join("\r\n\r\n", functions);

        }

        

        
    }
}
