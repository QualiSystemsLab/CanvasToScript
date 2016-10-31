/*
 * Created by SharpDevelop.
 * User: yaniv-k
 * Date: 7/1/2015
 * Time: 10:13 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml;
using System.Xml.XPath;

namespace CanvasToScript.Parsers
{
	/// <summary>
	/// Description of TextfieldToTxt.
	/// </summary>
	public static class TextfieldToTxt
	{
		public static string Parse(XmlNode node)
		{
			string output = null;
			string xml = node.OuterXml;
            
			
			if (xml.StartsWith("<ConstantExpression"))
			{
				output = ConstantExpressionToTxt(node);
			}
			else if (xml.StartsWith("<ConcatenationExpression"))
			{
                XmlNode concatType = node.SelectSingleNode(".//StringExpressionsConcator/@Key");
                if (concatType==null) 
                    concatType = node.SelectSingleNode(".//PasswordExpressionsConcator/@Key");
                if (concatType == null)
                    concatType = node.SelectSingleNode(".//Null/@Key");

                if (concatType.Value == "Concator")
                {
                    XmlNodeList parts = node.SelectNodes(".//List/*");
                    string dummy;
                    string curpart;
                    output = "";
                    foreach (XmlNode part in parts)
	                {
                        ConditionXmlToTxt.ParseCondition(part, out curpart, out dummy, out dummy, out dummy);
                        output += curpart;
	                }                    
                }
                else
                {
                    output = "TextfieldToTxt - ConcatenationExpression - unknown concatenation type: " + concatType.Value;
                }
			}
            else if (xml.StartsWith("<StringConcatenationExpression"))
            {
                XmlNodeList parts = node.SelectNodes(".//List/*");
                string dummy;
                string curpart;
                output = "";
                foreach (XmlNode part in parts)
                {
                    ConditionXmlToTxt.ParseCondition(part, out curpart, out dummy, out dummy, out dummy);
                    output += curpart;
                }
            }
			else if (xml.StartsWith("<BinaryExpression") ||
                     xml.StartsWith("<GroupExpression") ||
			         xml.StartsWith("<MemberExpression") ||
                     xml.StartsWith("<ExpressionDecorator"))
			{
				string dummy;
				ConditionXmlToTxt.ParseCondition(node, out output, out dummy, out dummy, out dummy);
			}
			else if (xml.StartsWith("<VariableExpression"))
			{
				output = VariableExpressionToTxt(node);
			}
			else if (xml.StartsWith("<Null"))
			{
				output = "";
			}
            else if (xml.StartsWith("<RangeIndexedVariableExpression"))
            {
                XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Vector")
                {
                    string variable = TextfieldToTxt.Parse(node.SelectSingleNode("*"));
                    XmlNode rowstart = node.SelectSingleNode(".//Range/*[@Key='Start']");
                    XmlNode rowend = node.SelectSingleNode(".//Range/*[@Key='End']");

                    output = variable.Replace("}", "(" + TextfieldToTxt.Parse(rowstart) + ":" + TextfieldToTxt.Parse(rowend) + ")}");
                }
                else
                {
                    output = "TextfieldToTxt - RangeIndexedVariableExpression - unknown dimension: " + dimension.Value;
                }
            }
			else if (xml.StartsWith("<AssignmentStatement"))
			{
				string source = TextfieldToTxt.Parse(node.SelectSingleNode(".//*[@Key='Source']"));
                string target = TextfieldToTxt.Parse(node.SelectSingleNode(".//*[@Key='Target']"));
                string operatorType = node.SelectSingleNode(".//AssignmentOperator/@Value").Value;

                output = target;
                switch (operatorType)
                {
                    case "Assignment":
                        output += " = " + source;
                        break;
                    case "PlusAssignment":
                        output += " += " + source;
                        break;
                    case "MinusAssignment":
                        output += " -= " + source;
                        break;
                    case "MultiplyAssignment":
                        output += " *= " + source;
                        break;
                    case "DivideAssignment":
                        output += " /= " + source;
                        break;
                    default:
                        output += " TextfieldToTxt - AssignmentStatement - unknown operator: " + operatorType + " " + source;
                        break;
                }
                
			}
			else if (xml.StartsWith("<IndexedVariableExpression"))
			{
                XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Vector")
                {
                    XmlNode variable = node.SelectSingleNode("*");
                    XmlNode col = node.SelectSingleNode("*[@Key='Index']");

                    output = TextfieldToTxt.Parse(variable).Replace("}", "(" + TextfieldToTxt.Parse(col) + ")}");
                }
                else
                {
                    output = "TextfieldToTxt - IndexedVariableExpression - unknown dimension: " + dimension.Value;
                }
			}
            else if (xml.StartsWith("<FunctionCall"))
            {
                string dummy;
                ConditionXmlToTxt.ParseCondition(node, out output, out dummy, out dummy, out dummy);
            }
			else if (xml.StartsWith("<CodeBlock"))
			{
                output = "";
                XmlNodeList innerList = node.SelectSingleNode(".//List[@Key='Statements']").ChildNodes;

                foreach (XmlNode statement in innerList)
                {
                    output += TextfieldToTxt.Parse(statement) + "\r\n";
                }

			}
			else if (xml.StartsWith("<IndexedMatrixVariableExpression"))
			{
				XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Matrix")
                {
                    XmlNode variable = node.SelectSingleNode("*");
                    XmlNode row = node.SelectSingleNode("*[@Key='IndexRow']");
                    XmlNode col = node.SelectSingleNode("*[@Key='IndexCol']");

                    output = TextfieldToTxt.Parse(variable).Replace("}", "(" + TextfieldToTxt.Parse(row) + "," + TextfieldToTxt.Parse(col) + ")}");
                }
                else
                {
                    output = "TextfieldToTxt - IndexedMatrixVariableExpression - unknown dimension: " + dimension.Value;
                }
			}
			else if (xml.StartsWith("<MatrixSubColVariableExpressionLogicalTreeProvider"))
			{
                XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Matrix")
                {
                    XmlNode variable = node.SelectSingleNode("*");
                    XmlNode rowstart = node.SelectSingleNode(".//Range[@Key='RowRange']/*[@Key='Start']");
                    XmlNode rowend = node.SelectSingleNode(".//Range[@Key='RowRange']/*[@Key='End']");
                    XmlNode col = node.SelectSingleNode("*[@Key='ColIndex']");

                    output = TextfieldToTxt.Parse(variable).Replace("}", "(" + TextfieldToTxt.Parse(rowstart) + ":" + TextfieldToTxt.Parse(rowend) + "," + TextfieldToTxt.Parse(col) + ")}");
                }
                else
                {
                    output = "TextfieldToTxt - MatrixSubColVariableExpressionLogicalTreeProvider - unknown dimension: " + dimension.Value;
                }
			}
			else if (xml.StartsWith("<MatrixSubRowVariableExpressionLogicalTreeProvider"))
			{
                XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Matrix")
                {
                    XmlNode variable = node.SelectSingleNode("*");
                    XmlNode colstart = node.SelectSingleNode(".//Range[@Key='ColRange']/*[@Key='Start']");
                    XmlNode colend = node.SelectSingleNode(".//Range[@Key='ColRange']/*[@Key='End']");
                    XmlNode row = node.SelectSingleNode("*[@Key='RowIndex']");

                    output = TextfieldToTxt.Parse(variable).Replace("}", "(" + TextfieldToTxt.Parse(row) + "," + TextfieldToTxt.Parse(colstart) + ":" + TextfieldToTxt.Parse(colend) + ")}");
                }
                else
                {
                    output = "TextfieldToTxt - MatrixSubRowVariableExpressionLogicalTreeProvider - unknown dimension: " + dimension.Value;
                }
			}
            else if (xml.StartsWith("<RangeIndexedMatrixVariableExpression"))
            {
                XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Matrix")
                {
                    XmlNode variable = node.SelectSingleNode("*");
                    XmlNode rowstart = node.SelectSingleNode(".//Range[@Key='RowRange']/*[@Key='Start']");
                    XmlNode rowend = node.SelectSingleNode(".//Range[@Key='RowRange']/*[@Key='End']");
                    XmlNode colstart = node.SelectSingleNode(".//Range[@Key='ColRange']/*[@Key='Start']");
                    XmlNode colend = node.SelectSingleNode(".//Range[@Key='ColRange']/*[@Key='End']");

                    output = TextfieldToTxt.Parse(variable).Replace("}", "(" + TextfieldToTxt.Parse(rowstart) + ":" + TextfieldToTxt.Parse(rowend) + "," + TextfieldToTxt.Parse(colstart) + ":" + TextfieldToTxt.Parse(colend) + ")}");
                }
                else
                {
                    output = "TextfieldToTxt - MatrixSubColVariableExpressionLogicalTreeProvider - unknown dimension: " + dimension.Value;
                }
            }
            else if (xml.StartsWith("<MatrixColVariableExpressionLogicalTreeProvider"))
            {
                XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Matrix")
                {
                    XmlNode variable = node.SelectSingleNode("*");
                    XmlNode col = node.SelectSingleNode("*[@Key='ColIndex']");

                    output = TextfieldToTxt.Parse(variable).Replace("}", "(:," + TextfieldToTxt.Parse(col) + ")}");
                }
                else
                {
                    output = "TextfieldToTxt - MatrixColVariableExpressionLogicalTreeProvider - unknown dimension: " + dimension.Value;
                }
            }
            else if (xml.StartsWith("<MatrixRowVariableExpressionLogicalTreeProvider"))
            {
                XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Matrix")
                {
                    XmlNode variable = node.SelectSingleNode("*");
                    XmlNode row = node.SelectSingleNode("*[@Key='RowIndex']");

                    output = TextfieldToTxt.Parse(variable).Replace("}", "(" + TextfieldToTxt.Parse(row) + ",:)}");
                }
                else
                {
                    output = "TextfieldToTxt - MatrixRowVariableExpressionLogicalTreeProvider - unknown dimension: " + dimension.Value;
                }
            }
            else if (xml.StartsWith("<RowRangeIndexedMatrixVariableExpression"))
            {
                XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Matrix")
                {
                    XmlNode variable = node.SelectSingleNode("*");
                    XmlNode rowstart = node.SelectSingleNode(".//Range[@Key='RowRange']/*[@Key='Start']");
                    XmlNode rowend = node.SelectSingleNode(".//Range[@Key='RowRange']/*[@Key='End']");
                    
                    output = TextfieldToTxt.Parse(variable).Replace("}", "(" + TextfieldToTxt.Parse(rowstart) + ":" + TextfieldToTxt.Parse(rowend) + ",:)}");
                }
                else
                {
                    output = "TextfieldToTxt - RowRangeIndexedMatrixVariableExpression - unknown dimension: " + dimension.Value;
                }
            }
            else if (xml.StartsWith("<ColRangeIndexedMatrixVariableExpression"))
            {
                XmlNode dimension = node.SelectSingleNode("*/@Key");

                if (dimension.Value == "Matrix")
                {
                    XmlNode variable = node.SelectSingleNode("*");
                    XmlNode colstart = node.SelectSingleNode(".//Range[@Key='ColRange']/*[@Key='Start']");
                    XmlNode colend = node.SelectSingleNode(".//Range[@Key='ColRange']/*[@Key='End']");
                    
                    output = TextfieldToTxt.Parse(variable).Replace("}", "(:," + TextfieldToTxt.Parse(colstart) + ":" + TextfieldToTxt.Parse(colend) + ")}");
                }
                else
                {
                    output = "TextfieldToTxt - ColRangeIndexedMatrixVariableExpression - unknown dimension: " + dimension.Value;
                }
            }
            else if (xml.StartsWith("<Boolean ") ||
                     xml.StartsWith("<Double ") ||
                     xml.StartsWith("<DataDimension ") ||
                     xml.StartsWith("<String ") ||
                     xml.StartsWith("<ReadBufferType ") ||
                     xml.StartsWith("<DllHostType ") ||
                     xml.StartsWith("<PaddingOptions ") ||
                     xml.StartsWith("<LaunchMode ") ||
                     xml.StartsWith("<Int32 "))
            {
                output = node.Attributes["Value"].Value;
            }
            else if (xml.StartsWith("<TrimWhitespaceOptions"))
            {
                output = node.Attributes["Value"].Value;
            }
            else if (xml.StartsWith("<Password "))
            {
                output = "\"" + node.SelectSingleNode(".//*[@Key='Password']/@Value").Value + "\"" + " // (encrypted value)";
            }
            else if (xml.StartsWith("<VectorString"))
            {
                XmlNodeList elements = node.SelectNodes(".//*[@Key='Elements']/KeyValuePair");
                int count = int.Parse(node.SelectSingleNode(".//*[@Key='Size']/@Value").Value);
                output = "";
                string[] values = new string[count];
                for (int i = 0; i < count; i++){ values[i] = "\"\""; }

                foreach (XmlNode element in elements)
                {
                    int index = int.Parse(element.SelectSingleNode(".//*[@Key='HashKey']/@Value").Value);
                    string value = element.SelectSingleNode(".//*[@Key='HashValue']/@Value").Value;
                    values[index] = "\"" + value + "\"";
                }
                output = "[" + String.Join(", ", values) + "]";
            }
            else if (xml.StartsWith("<VectorNumeric"))
            {
                XmlNodeList elements = node.SelectNodes(".//*[@Key='Elements']/KeyValuePair");
                int count = int.Parse(node.SelectSingleNode(".//*[@Key='ElementCount']/@Value").Value);
                output = "";
                int[] numbers = new int[count];
                Array.Clear(numbers, 0, numbers.Length);
                foreach (XmlNode element in elements)
                {
                    int index = int.Parse(element.SelectSingleNode(".//*[@Key='HashKey']/@Value").Value);
                    int value = int.Parse(element.SelectSingleNode(".//*[@Key='HashValue']/@Value").Value);
                    numbers[index] = value;
                }
                output = "[" + String.Join(", ", numbers) + "]";
            }
            else if (xml.StartsWith("<MatrixString") ||
                     xml.StartsWith("<MatrixNumeric"))
            {
                XmlNodeList vectors = node.SelectNodes(".//List[@Key='Vectors']/*");
                string curpart;
                output = "";
                int c = 0;
                foreach (XmlNode vect in vectors)
                {
                    curpart = TextfieldToTxt.Parse(vect);
                    curpart = curpart.Substring(1, curpart.Length - 2);
                    output += curpart +  "; " ;
                    c++;
                }
                if (output.Length == 0)
                {
                    output = "[;]"; 
                }
                else
                    output = "[" + output + "]";
            }
            else if (xml.StartsWith("<RangeEndExpression"))
            {
                output = "end";
            }
            else if (xml.StartsWith("<SessionInfo"))
            {
                if (node.SelectSingleNode(".//Guid/@Value").Value == "00000000-0000-0000-0000-000000000000")
                    output = "null";
                else
                    output = "TextfieldToTxt - SessionInfo - unhandled session with id";
            }
            else if (xml.StartsWith("<NamedParameterInfo"))
            {
                string name = node.SelectSingleNode(".//*[@Key='Name']/@Value").Value;
                string value = TextfieldToTxt.Parse(node.SelectSingleNode(".//*[@Key='Value']"));
                output = name + " = " + value;
            }
            else if (xml.StartsWith("<VectorBuilder"))
            {
                XmlNodeList cells = node.SelectNodes(".//*[@Key='Cells']/*");
                string dummy;
                string curpart;
                output = "";
                int c = 0;
                foreach (XmlNode cell in cells)
                {
                    ConditionXmlToTxt.ParseCondition(cell, out curpart, out dummy, out dummy, out dummy);
                    output += curpart + (c<cells.Count-1 ? ", ":"");
                    c++;
                }
                output = "[" + output + "]";
            }
            else if (xml.StartsWith("<WhileStatement"))
            {
                XmlNode codeblock = node.SelectSingleNode(".//*[@Key='CodeBlock']");
                XmlNode condition = node.SelectSingleNode(".//*[@Key='Condition']");
                bool isDoWhile = bool.Parse(node.SelectSingleNode(".//*[@Key='IsVerifyAfter']/@Value").Value);

                if (isDoWhile)
                {
                    output = "do" + "\r\n";
                    output += TextfieldToTxt.Parse(codeblock);
                    output += "while" + TextfieldToTxt.Parse(condition) + "\r\n";
                }
                else
                {
                    output = "while " + TextfieldToTxt.Parse(condition) + "\r\n";
                    output += TextfieldToTxt.Parse(codeblock) + "end" + "\r\n";
                    
                }
            }
            else if (xml.StartsWith("<ForStatement"))
            {
                XmlNode codeblock = node.SelectSingleNode(".//*[@Key='CodeBlock']");
                XmlNode index = node.SelectSingleNode(".//*[@Key='Index']");
                XmlNode binary = node.SelectSingleNode(".//BinaryExpression");

                output = "for (" + TextfieldToTxt.Parse(index) + " = " + TextfieldToTxt.Parse(binary) + ")" + "\r\n";
                output += TextfieldToTxt.Parse(codeblock) + "end" + "\r\n";
            }
            else if (xml.StartsWith("<IfStatement") || xml.StartsWith("<ConditionedCodeBlock"))
            {
                XmlNode codeblock = node.SelectSingleNode(".//*[@Key='CodeBlock']");
                XmlNode condition = node.SelectSingleNode(".//*[@Key='Condition']");
                XmlNode elseblock = node.SelectSingleNode(".//*[@Key='Else']");
                XmlNode elseifs = node.SelectSingleNode(".//*[@Key='ElseIfs']");
                
                output = "if " + TextfieldToTxt.Parse(condition) + "\r\n";
                output += TextfieldToTxt.Parse(codeblock);

                if (elseifs!=null && elseifs.ChildNodes.Count>0)
                {
                    foreach (XmlNode item in elseifs.ChildNodes)
                    {
                        output += "else" + TextfieldToTxt.Parse(item);
                    }
                }

                if (elseblock!=null && elseblock.Name != "Null")
                {
                    output += "else" + "\r\n";
                    output += TextfieldToTxt.Parse(elseblock);                    
                }

                if (xml.StartsWith("<IfStatement"))
                    output += "end";

            }
            else if (xml.StartsWith("<UnaryExpression"))
            {
                string target = TextfieldToTxt.Parse(node.SelectSingleNode(".//*[@Key='Expression']"));
                string operatorType = node.SelectSingleNode(".//UnaryOperator/@Value").Value;
                if (operatorType == "AutoIncrement")
                {
                    output = target + "++";
                }
                else if (operatorType == "AutoDecrement")
                {
                    output = target + "--";
                }
                else if (operatorType == "Negate")
                {
                    output = "-" + target;
                }
                else
                {
                    output = "TextfieldToTxt - UnaryExpression - unknown operator: " + operatorType;
                }
            }
            else if (xml.StartsWith("<MatrixBuilder"))
            {
                XmlNodeList lists = node.SelectNodes(".//*[@Key='Cells']/*");
                string dummy;
                string curpart;
                output = "";
                int c = 0;
                foreach (XmlNode list in lists)
                {
                    XmlNodeList cells = list.SelectNodes("./*");
                    foreach (XmlNode cell in cells)
                    {
                        ConditionXmlToTxt.ParseCondition(cell, out curpart, out dummy, out dummy, out dummy);
                        output += curpart + (c < cells.Count - 1 ? ", " : "");
                        c++;
                    }
                    output += ";";
                }
                output = "[" + output + "]";
            }
            else
            {
                return "TextfieldToTxt - unidentified xml: " + xml;
            }
			
			if (output==null) 
				return "TextfieldToTxt - not implemented xml: " + node.Name;
			
			return output;
			
		}
		
		private static string ConstantExpressionToTxt(XmlNode xml)
		{
			XmlNode valuexml = xml.SelectSingleNode("./*[@Key='Value']");

            string value = TextfieldToTxt.Parse(valuexml);
            string type = xml.SelectSingleNode("./*[@Key='Value']").Name;
			
			if (type=="String") value = "\"" + value + "\"";
			
			return value;
		}
		
		private static string VariableExpressionToTxt(XmlNode xml)
		{
			string value = xml.SelectSingleNode("*[@Key='Name']/@Value").Value;
			
			value = "{" + value + "}";
			
			return value;
		}
	}
}
