using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using CanvasToScript.Tools;

namespace CanvasToScript.Parsers
{
    public static class ErrorHandlingToTxt
    {
        public static string Parse(XmlNode node, int indentLevel)
        {
            if (node == null) return "";

            string output = "";
            string indentString = new string(' ', indentLevel * 4);
                        
            XmlNodeList handlers = node.SelectNodes(".//*[@Key='ErrorHandlers']/ErrorHandler/Boolean[@Key='IsEnabled' and @Value='True']/..");
            foreach (XmlNode handler in handlers)
            {
                string conditionType = handler.SelectSingleNode(".//ErrorConditionType/@Value").Value;
                string errorName = "";
                if (conditionType == "Simple")
                {
                    errorName = handler.SelectSingleNode(".//*[@Key=\"SimpleConditionErrorName\"]/@Value").Value;
                }
                else if (conditionType == "Complex")
                {
                    string dummy;
                    ConditionXmlToTxt.ParseCondition(handler.SelectSingleNode(".//*[@Key=\"ComplexCondition\"]"), out errorName, out dummy, out dummy, out dummy);
                }
                else
                {
                    errorName = "ErrorHandlingToTxt - unknown condition type: " + conditionType;
                }
                string handlerName = handler.SelectSingleNode(".//*[@Key=\"Name\"]/@Value").Value;
                output += indentString + "catch (" + errorName + ")" + " // " + handlerName + "\r\n" + indentString + "{" + "\r\n";

                XmlNodeList actions = handler.SelectNodes(".//*[@Key=\"Actions\"]/*/Boolean[@Key=\"IsEnabled\" and @Value=\"True\"]/..");
                foreach (XmlNode action in actions)
                {
                    string actionStr = ParseAction(action, indentLevel+1);
                    output += actionStr + "\r\n";
                }

                XmlNode terminationAction = handler.SelectSingleNode(".//*[@Key=\"TerminationAction\"]");
                if (terminationAction.Name != "Null")
                {
                    string isEnabled = terminationAction.SelectSingleNode(".//*[@Key=\"IsEnabled\"]/@Value").Value;
                    if (isEnabled == "True")
                    {
                        output += "\r\n" + indentString + "    " + ParseTermination(terminationAction);

                    }
                }
                output += indentString + "}\r\n";
            }

            return output;
        }

        private static string ParseTermination(XmlNode termination)
        {
            string output = "";
            string scope = termination.SelectSingleNode(".//*[@Key=\"Scope\"]/@Value").Value;
            string name = termination.SelectSingleNode(".//*[@Key=\"Name\"]/@Value").Value;

            output = "If step still fails: ";
            if (name == "Return")
            {
                string returnError = termination.SelectSingleNode(".//*[@Key=\"ReturnError\"]/@Value").Value;
        
                output += "End " + scope;
                if (returnError == "True")
                    output += " with error and continue";
                else if (scope != "All")
                    output += " and continue";
            }
            else if (name.StartsWith("Terminate"))
            {
                output += "Terminate all";
            }

            return output + "\r\n";
        }

        private static string ParseAction(XmlNode action, int indentLevel)
        {
            string output = "";
            string indentString = new string(' ', indentLevel * 4);

            string name = action.SelectSingleNode(".//*[@Key=\"Name\"]/@Value").Value;
            string scope = action.SelectSingleNode(".//*[@Key=\"Scope\"]/@Value").Value;
            string retryCount = TextfieldToTxt.Parse(action.SelectSingleNode(".//*[@Key=\"RetryCount\"]"));
            string retryInterval = TextfieldToTxt.Parse(action.SelectSingleNode(".//*[@Key=\"RetryInterval\"]"));
            string isRetryEnabled = action.SelectSingleNode(".//*[@Key=\"IsRetryEnabled\"]/@Value").Value;

            output = indentString + "/* Action Type: " + action.Name +  " */" + "\r\n";
            switch (action.Name)
            {
                case "RetryAction":
                    output += indentString + name + "(Retry " + scope + " in " + retryInterval + " seconds, a maximum of " + retryCount + " times)";
                    break;
                case "RestartSessionAction":
                    string session = TextfieldToTxt.Parse(action.SelectSingleNode(".//*[@Key=\"Session\"]"));
                    string delayBeforeRestart = TextfieldToTxt.Parse(action.SelectSingleNode(".//*[@Key=\"DelayBeforeRestart\"]"));
                    output += indentString + name + "(Wait " + delayBeforeRestart + " seconds and restart " + session + ".";
                    if (isRetryEnabled == "True")
                        output += " If action succeeds, retry " + scope + " in " + retryInterval + " seconds, a maximum of " + retryCount + " times)";
                    else
                        output += ")";
                    break;
                case "RunCodeAction":
                    string code = action.SelectSingleNode(".//*[@Key=\"Code\"]/@Value").Value;
                    output += indentString + name + "(Execute the code below.";
                    if (isRetryEnabled == "True")
                        output += " If action succeeds, retry " + scope + " in " + retryInterval + " seconds, a maximum of " + retryCount + " times)";
                    else
                        output += ")";
                    output += "\r\n" + indentString + "/***** Matshell code *****/" + "\r\n";
                    output += indentString + code.Replace("_x000D__x000A_", "\r\n" + indentString);
                    output += "\r\n" + indentString + "/*** Matshell code end ***/" + "\r\n";
                    
                    break;
                case "PaWRunFunction":
                case "RunFunction":
                    XmlNode testTool = action.SelectSingleNode(".//PaWTestTool");
                    string testlog, dummy;
                    string testname = testTool.SelectSingleNode(".//*[@Key=\"Name\"]/@Value").Value.Replace(".tsdrv","").Replace(".tstest","");
                    TestTool.TestToTxt(testTool, indentLevel, out testlog, out dummy);
                    output += indentString + testname.Replace("_x0020_", "_") + testlog;
                    if (isRetryEnabled == "True")
                        output += "\r\n" + indentString + "If action succeeds, retry " + scope + " in " + retryInterval + " seconds, a maximum of " + retryCount + " times";
                    
                    break;
                default:
                    break;
            }

            return output;
        }
    }
}
