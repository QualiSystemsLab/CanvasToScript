using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using CanvasToScript.Tools;
using CanvasToScript.Parsers;

namespace CanvasToScript.Common
{
    static class PartialCanvasToTxt
    {
        public static void Analyze(ref Step[,] canvasMatrix, int threadCol, int mincol, int minrow, int indentLevel, out string ThreadLog, out string ThreadFunctions)
        {
            ThreadLog = "";
            ThreadFunctions = "";
            //string indentString = new string(' ', indentLevel * 4);

            int curRow = 0;
            while (curRow < canvasMatrix.GetLength(0))
            {
                if (canvasMatrix[curRow, threadCol] != null)
                {
                    if (canvasMatrix[curRow, threadCol].TopConnected)
                    {
                        string log, functions;
                        curRow = LogStep(ref canvasMatrix, canvasMatrix[curRow, threadCol], curRow, threadCol, indentLevel, out log, out functions);
                        ThreadLog += log;
                        ThreadFunctions += functions;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                    break;

                curRow += 1;
            }

        }

        private static int LogStep(ref Step[,] canvasMatrix, Step step, int curRow, int threadCol, int indentLevel, out string log, out string functions)
        {
            int nextRow = curRow;
            log="";
            functions="";
            string indentString = new string(' ', indentLevel * 4);

            if (step.Description.Length > 0)
                log = "\r\n\r\n" + indentString + "/*" + step.Description + "*/";// + ((log.Length > 0) ? ("\r\n" + log) : (""));
            else
                log = "";//"\r\n";// +log;

            switch(step.ToolName)
            {
                case "NullTool":
                    { 
                        log = ""; 
                        return nextRow; 
                    }
                case "NoOpTool":
                    {
                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "// " + "/*" + step.StepName.Replace("_x0020_", "_") + "*/";
                        else
                            log += "\r\n" + indentString + "/*NOP: " + step.StepName.Replace("_x0020_", "_") + "*/";

                        return nextRow;
                    }
                case "CaseTool":
                    {
                        int width = step.StepWidth;
                        int height = step.StepHeight;
                        log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";
                        string caselog;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        Step[,] caseMatrix = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, curRow, threadCol, Math.Min(curRow + height - 1, canvasMatrix.GetLength(0) - 1), canvasMatrix.GetLength(1) - 1);
                        CaseTool.CaseToTxt(ref caseMatrix, 0, indentLevel + ((eh.Length > 0) ? 1 : 0), height, width, out caselog, out functions);
                        nextRow = curRow + height;
                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "// " + caselog.Replace("\r\n", "\r\n// ");
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + caselog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + caselog;
                        }
                        return nextRow;
                    }
                case "VariableTool":
                    {
                        string varlog, varfunction;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        VariableTool.SetVariableToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out varlog, out varfunction);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + varlog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + varlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + varlog;
                        } 

                        if (varfunction.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + varfunction.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + varfunction;
                        }

                        return nextRow;
                    }
                case "PaWTestTool":
                case "MethodTool":
                case "TestTool":
                case "Mouse":
                case "Keyboard":
                case "Inspect":
                    {
                        string testlog, dummy;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        TestTool.TestToTxt(step.StepXmlNode, indentLevel + ((eh.Length > 0) ? 1 : 0), out testlog, out dummy);

                        if (step.ToolName == "TestTool" || step.ToolName=="PaWTestTool")
                        {
                            XmlNode path = step.StepXmlNode.SelectSingleNode(".//*[@Key='ResourceFullName']");
                            if (path != null)
                            {
                                string p;
                                if (path.Attributes["Value"] != null)
                                    p = path.Attributes["Value"].Value;
                                else
                                {
                                    p = path.SelectSingleNode(".//*[@Key='Value']/@Value").Value;
                                }

                                log += "\r\n" + indentString + "/*" + "Path: " + p.Replace(".tsdrv", "") + " */";
                            }
                            
                        }

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + testlog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + testlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + testlog;
                        } 

                        return nextRow;
                    }
                case "AttributeTool":
                    {
                        string alog, dummy;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        AttributeTool.AttributeToTxt(step.StepXmlNode, indentLevel + ((eh.Length > 0) ? 1 : 0), out alog, out dummy);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + alog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + alog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + alog;
                        }

                        return nextRow;
                    }
                case "CaptureImageTool":
                    {
                        string cilog, dummy;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        CaptureImageTool.CaptureImageToTxt(step.StepXmlNode, indentLevel + ((eh.Length > 0) ? 1 : 0), out cilog, out dummy);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + cilog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + cilog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + cilog;
                        }

                        return nextRow;
                    }
                case "CriteriaTool":
                    {
                        string clog, cfunction;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        CriteriaTool.CriteriaToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out clog, out cfunction);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + clog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + clog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + clog;
                        }

                        if (cfunction.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + cfunction.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + cfunction;
                        }

                        return nextRow;
                    }
                case "ScripterTool":
                case "PaWScripterTool":
                    {
                        string slog, dummy;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        ScripterTool.ScripterToTxt(step.StepXmlNode, indentLevel + ((eh.Length > 0) ? 1 : 0), out slog, out dummy);

                        XmlNode path = step.StepXmlNode.SelectSingleNode(".//*[@Key='ResourceFullName']");
                        if (path != null)
                        {
                            string p;
                            if (path.Attributes["Value"] != null)
                                p = path.Attributes["Value"].Value;
                            else
                            {
                                p = path.SelectSingleNode(".//*[@Key='Value']/@Value").Value;
                            }

                            log += "\r\n" + indentString + "/* " + "Path: " + p.Replace(".tsscript", "") + " */";
                        }

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + slog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + slog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + slog;
                        }

                        return nextRow;
                    }
                case "CommandTool":
                    {
                        string clog, cfunction;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        CommandTool.CommandToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out clog, out cfunction);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + clog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + clog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + clog;
                        } 

                        if (cfunction.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + cfunction.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + cfunction;
                        }

                        return nextRow;
                    }
                case "TerminalCommandTool":
                    {
                        string clog, cfunction;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        TerminalCommandTool.CommandToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out clog, out cfunction);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + clog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + clog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + clog;
                        }

                        if (cfunction.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + cfunction.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + cfunction;
                        }

                        return nextRow;
                    }
                case "WriteTool":
                    {
                        string clog, cfunction;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        WriteTool.WriteToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out clog, out cfunction);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + clog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + clog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + clog;
                        }

                        if (cfunction.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + cfunction.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + cfunction;
                        }

                        return nextRow;
                    }
                case "ReadTool":
                    {
                        string clog, cfunction;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        ReadTool.ReadToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out clog, out cfunction);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + clog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + clog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + clog;
                        }

                        if (cfunction.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + cfunction.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + cfunction;
                        }

                        return nextRow;
                    }
                case "MessageTool":
                    {
                        string clog, cfunction;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        MessageTool.MessageToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out clog, out cfunction);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + clog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + clog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + clog;
                        }

                        if (cfunction.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + cfunction.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + cfunction;
                        }

                        return nextRow;
                    }
                case "TransformationTool":
                    {
                        string tlog, tfunctions;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        TransformTool.TransformToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out tlog, out tfunctions);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + tlog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + tlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + tlog;
                        } 

                        if (tfunctions.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + tfunctions.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + tfunctions;
                        }

                        return nextRow;
                    }
                case "SetSessionTool":
                    {
                        string sessionlog, dummy;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        SetSessionTool.SessionToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out sessionlog, out dummy);
                        log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + sessionlog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + sessionlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + sessionlog;
                        }

                        return nextRow;
                    }
                case "DelayTool":
                    {
                        string dlog, dummy;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        DelayTool.DelayToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out dlog, out dummy);
                        
                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + dlog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + dlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + dlog;
                        } 

                        return nextRow;
                    }
                case "SetEventTool":
                    {
                        string dlog, dummy;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        SetEventTool.SetEventToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out dlog, out dummy);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + dlog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + dlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + dlog;
                        }

                        return nextRow;
                    }
                case "WaitForEventTool":
                    {
                        string dlog, dummy;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        WaitForEventTool.WaitForEventToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out dlog, out dummy);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + dlog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + dlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + dlog;
                        }

                        return nextRow;
                    }
                case "PassFailTool":
                    {
                        string dlog, dfunc;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        PassFailTool.PassFailToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out dlog, out dfunc);
                        string tooltype = step.StepXmlNode.SelectSingleNode(".//*[@Key='NotificationType']/@Value").Value;
                        string toolname = "PassFailTool";
                        switch (tooltype)
                        {
                            case "0":
                                toolname = "Pass";
                                break;
                            case "1":
                                toolname = "Fail";
                                break;
                            case "2":
                                toolname = "Text To Report";
                                break;
                            default:
                                break;
                        }
                        log += "\r\n" + indentString + "/*" + toolname + ": " + step.StepName + "*/";

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + dlog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + dlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + dlog;
                        }

                        if (dfunc.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + dfunc.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + dfunc;
                        }

                        return nextRow;
                    }
                case "ErrorTool":
                    {
                        string dlog, dummy;
                        ErrorTool.ErrorToTxt(step, indentLevel, out dlog, out dummy);
                        log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "// throw" + step.StepName.Replace("_x0020_", "_") + dlog;
                        else
                            log += "\r\n" + indentString + "throw " + step.StepName.Replace("_x0020_", "_") + dlog;

                        return nextRow;
                    }
                case "EndSessionTool":
                    {
                        string dlog, dummy;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        EndSessionTool.EndSessionToTxt(step, indentLevel + ((eh.Length > 0) ? 1 : 0), out dlog, out dummy);
                        log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + dlog;
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + dlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + dlog;
                        } 

                        return nextRow;
                    }
                case "EndTool":
                    {
                        string dlog, dummy;
                        EndTool.EndToTxt(step, indentLevel, out dlog, out dummy);
                        log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + dlog;
                        else
                            log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + dlog;

                        return nextRow;
                    }
                case "CodeTool":
                    {
                        string dummy, codefunction;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        CodeTool.CodeToTxt(step, indentLevel + ((eh.Length>0)?1:0), out dummy, out codefunction);

                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "//" + step.StepName.Replace("_x0020_", "_") + "()";
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + step.StepName.Replace("_x0020_", "_") + "()";
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + step.StepName.Replace("_x0020_", "_") + "()";
                        }

                        if (codefunction.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + codefunction.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + codefunction;
                        }

                        return nextRow;
                    }
                case "LoopTool":
                case "ParaLoopTool":
                    {
                        int width = step.StepWidth;
                        int height = step.StepHeight;
                        log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";
                        string looplog, loopfunctions;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        Step[,] loopMatrix = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, curRow, threadCol, Math.Min(curRow + height -1,canvasMatrix.GetLength(0) - 1), canvasMatrix.GetLength(1) - 1);
                        LoopTool.LoopToTxt(ref loopMatrix, 0, indentLevel + ((eh.Length > 0) ? 1 : 0), height, width, out looplog, out loopfunctions);

                        nextRow = curRow + height;
                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "// " + looplog.Replace("\r\n", "\r\n// ");
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + looplog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + looplog;
                        } 

                        if (loopfunctions.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + loopfunctions.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + loopfunctions;
                        }

                        return nextRow;
                    }
                case "WhileTool":
                    {
                        int width = step.StepWidth;
                        int height = step.StepHeight;
                        log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";
                        string wlog, wfunctions;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        Step[,] whileMatrix = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, curRow, threadCol, Math.Min(curRow + height - 1, canvasMatrix.GetLength(0) - 1), canvasMatrix.GetLength(1) - 1);
                        WhileTool.WhileToTxt(ref whileMatrix, 0, indentLevel + ((eh.Length > 0) ? 1 : 0), height, width, out wlog, out wfunctions);

                        nextRow = curRow + height;
                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "// " + wlog.Replace("\r\n", "\r\n// ");
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + wlog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + wlog;
                        } 

                        if (wfunctions.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + wfunctions.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + wfunctions;
                        }

                        return nextRow;
                    }
                case "LockTool":
                    {
                        int width = step.StepWidth;
                        int height = step.StepHeight;
                        log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";
                        string llog, lfunctions;
                        string eh = ErrorHandlingToTxt.Parse(step.StepXmlNode, indentLevel);
                        Step[,] lockMatrix = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, curRow, threadCol, Math.Min(curRow + height - 1, canvasMatrix.GetLength(0) - 1), canvasMatrix.GetLength(1) - 1);
                        LockTool.LockToTxt(ref lockMatrix, 0, indentLevel + ((eh.Length > 0) ? 1 : 0), height, width, out llog, out lfunctions);

                        nextRow = curRow + height;
                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "// " + llog.Replace("\r\n", "\r\n// ");
                        else
                        {
                            if (eh.Length > 0)
                            {
                                log += "\r\n" + indentString + "try" + "\r\n" + indentString + "{";
                                log += "\r\n" + indentString + "    " + llog;
                                log += "\r\n" + indentString + "}" + "\r\n" + eh;
                            }
                            else
                                log += "\r\n" + indentString + llog;
                        } 

                        if (lfunctions.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + lfunctions.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + lfunctions;
                        }

                        return nextRow;
                    }
                case "ParallelTool":
                    {
                        int width = step.StepWidth;
                        int height = step.StepHeight;
                        log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";
                        string plog, pfunctions;
                        Step[,] pMatrix = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, curRow, 0, Math.Min(curRow + height - 1, canvasMatrix.GetLength(0) - 1), canvasMatrix.GetLength(1) - 1);
                        ParallelTool.ParallelToTxt(ref pMatrix, threadCol, indentLevel, height, width, out plog, out pfunctions);
                        nextRow = curRow + height;
                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "// " + plog.Replace("\r\n", "\r\n// ");
                        else
                            log += "\r\n" + indentString + plog;

                        if (pfunctions.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + pfunctions.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + pfunctions;
                        }

                        return nextRow;
                    }
                case "GroupTool":
                case "PaWDllTool":
                case "DllTool":
                case "CommandShellTool":
                case "SequenceTool":
                case "TerminalTool":
                case "WebServiceTool":
                case "NetworkClientTool":
                    {
                        string glog, gfunctions;
                        if (step.ToolName=="DllTool" || step.ToolName=="PaWDllTool" || step.ToolName=="WebServiceTool")
                        {
                            XmlNode path = step.StepXmlNode.SelectSingleNode(".//*[@Key='ResourceFullName']");
                            if (path != null)
                            {
                                string p;
                                if (path.Attributes["Value"] != null)
                                    p = path.Attributes["Value"].Value;
                                else
                                {
                                    p = path.SelectSingleNode(".//*[@Key='Value']/@Value").Value;
                                }

                                log += "\r\n" + indentString + "/*" + "Path: " + p.Replace(".tsdll","") + " */";
                            }

                            log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";
                        }
                        else if (step.ToolName == "SequenceTool")
                        {
                            XmlNode path = step.StepXmlNode.SelectSingleNode(".//*[@Key='LibraryFullName']");
                            if (path != null)
                            {
                                string p;
                                if (path.Attributes["Value"] != null)
                                    p = path.Attributes["Value"].Value;
                                else
                                {
                                    p = path.SelectSingleNode(".//*[@Key='Value']/@Value").Value;
                                }

                                log += "\r\n" + indentString + "/*" + "Path: " + p.Replace(".tslib", "") + " */";
                            }

                            log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";
                        }
                        else
                        {
                            log += "\r\n" + indentString + "/*" + step.ToolName + ": " + step.StepName + "*/";
                        }
                        XmlNode groupSteps = step.StepXmlNode.SelectSingleNode(".//List[@Key='Steps']");
                        XmlNode eh = step.StepXmlNode.SelectSingleNode(".//ErrorHandlingBehavior");
                        CanvasAnalyzer.StepsListToTxt(groupSteps.ChildNodes, true, indentLevel, eh, out glog, out gfunctions);
                        if (step.Enabled == false)
                            log += "\r\n" + indentString + "// " + glog.Replace("\r\n", "\r\n// ");
                        else
                            log += "\r\n" + indentString + glog; 

                        if (gfunctions.Length > 0)
                        {
                            if (step.Enabled == false)
                                functions += "\r\n" + "// " + gfunctions.Replace("\r\n", "\r\n// ");
                            else
                                functions += "\r\n" + gfunctions;
                        }                        

                        return nextRow;
                    }
                case "AnalyzableCompositeTool":
                    {
                        string aclog, acfunctions;
                        XmlNodeList acTools = step.StepXmlNode.SelectNodes(".//Array[@Key='ChildTools']/*");
                        foreach (XmlNode tool in acTools)
                        {
                            Step curstep = new Step(tool);
                            curstep.Enabled = step.Enabled;
                            LogStep(ref canvasMatrix, curstep, curRow, threadCol, indentLevel, out aclog, out acfunctions);
                            
                            if (step.Enabled == false)
                                log += "\r\n" + indentString + "// " + aclog.Replace("\r\n", "\r\n// ");
                            else
                                log += "\r\n" + indentString + aclog;

                            if (acfunctions.Length > 0)
                            {
                                if (step.Enabled == false)
                                    functions += "\r\n" + "// " + acfunctions.Replace("\r\n", "\r\n// ");
                                else
                                    functions += "\r\n" + acfunctions;
                            }  

                        }

                        return nextRow + acTools.Count-1;
                    }
            }

            if (step.Enabled==false)
	            log += "\r\n" + indentString + "//";
            else
	            log += "\r\n" + indentString;

            log += step.StepName.Replace("_x0020_","_") + " // (" + step.ToolName + ")";

            return nextRow;
        }
    }
}
