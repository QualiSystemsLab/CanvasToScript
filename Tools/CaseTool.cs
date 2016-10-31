
using System;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using CanvasToScript.Common;
using CanvasToScript.Parsers;

namespace CanvasToScript.Tools
{
	static class CaseTool
	{
		public static void CaseToTxt(ref Step[,] canvasMatrix, int threadCol, int indentLevel, int caseHeight, int caseWidth, out string ThreadLog, out string ThreadFunctions)
		{
			ThreadLog = "";
			ThreadFunctions = "";
			string indentString = new string(' ', indentLevel * 4);
			
			XmlNodeList conditions = GetConditions(canvasMatrix[0,0]);
			string left, right, operatorTxt;
			var conditionTxt = new string[conditions.Count];
			int curcon=0;
			foreach (XmlNode condition in conditions) {
				
				ConditionXmlToTxt.ParseCondition(condition, out conditionTxt[curcon], out left, out operatorTxt, out right);
				curcon++;
			}

            XmlNode col = canvasMatrix[0,0].StepXmlNode.SelectSingleNode("List[@Key='Blocks']/StepBlock");
            XmlNodeList cols = col.SelectNodes("Array[@Key='OutputConnectors']//*[@Key='Offset']/@Value");

            XmlNode hasDefaultXml = canvasMatrix[0, 0].StepXmlNode.SelectSingleNode(".//*[@Key=\"HasDefaultBranch\"]");
            int branches = conditions.Count;
            if (hasDefaultXml != null)
            {
                if (hasDefaultXml.Attributes["Value"].Value == "True")
                    branches++;
            }
            else
            {
                //before the case tool could had 1 column
                branches++;
            }
			var logs = new string[conditions.Count+1];
            for (curcon = 0; curcon < branches; curcon++)
			{
				Step[,] threadSteps = MatrixTransformations.ResizeArray<Step>(ref canvasMatrix, 1, 0, canvasMatrix.GetLength(0)-1, canvasMatrix.GetLength(1)-1);
				string functions;
                int column = int.Parse(cols[curcon].Value);
                PartialCanvasToTxt.Analyze(ref threadSteps, column, 0, 0, indentLevel + 1, out logs[curcon], out functions);
				ThreadFunctions += "\r\n" + functions;
			}
            for (curcon = 0; curcon < branches; curcon++)
			{
                if (curcon==0)
				{
					ThreadLog += "if " + conditionTxt[curcon];
				}
				else if (curcon<logs.Length-1)
				{
					ThreadLog += "\r\n" + indentString + "else if " + conditionTxt[curcon];
				}
				else
				{
					ThreadLog += "\r\n" + indentString + "else";
				}
				
				ThreadLog += "\r\n" + indentString + "{" + "\r\n";
                ThreadLog += indentString + "    " + logs[curcon].Trim();
				ThreadLog += "\r\n" + indentString + "}";
			}
		}
		
		private static XmlNodeList GetConditions(Step step)
		{
			XmlNodeList nodes = step.StepXmlNode.SelectNodes("CaseTool/Array[@Key='Conditions']/*");
                
			return nodes;
		} 
	}
}
