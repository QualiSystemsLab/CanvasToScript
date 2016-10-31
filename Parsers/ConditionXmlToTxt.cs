/*
 * Created by SharpDevelop.
 * User: yaniv-k
 * Date: 7/1/2015
 * Time: 8:53 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace CanvasToScript.Parsers
{
	/// <summary>
	/// Description of ConditionXmlToTxt.
	/// </summary>
	public static class ConditionXmlToTxt
	{
		public static void ParseCondition(XmlNode condition, out string conditionTxt, out string left, out string operatorType, out string right)
		{
			left = "";
			right = "";
			operatorType = "";
			conditionTxt = "";
            string xml = condition.OuterXml;
			
			if (xml.StartsWith("<BinaryExpression Key=\"Left\">") ||
			    xml.StartsWith("<BinaryExpression Key=\"Right\">") ||
			    xml.StartsWith("<BinaryExpression"))
			{
				XmlNodeList operators = condition.SelectNodes("BinaryOperator");
                operatorType = ConvertOperator(operators[operators.Count - 1].Attributes["Value"].Value);
				
				XmlNode Left = condition.SelectSingleNode("*[@Key='Left']");
				XmlNode Right = condition.SelectSingleNode("*[@Key='Right']");
				string leftTxt, rightTxt, dummy;
				ParseCondition(Left, out leftTxt, out dummy, out dummy, out dummy);
				ParseCondition(Right, out rightTxt, out dummy, out dummy, out dummy);
				conditionTxt = "(" + leftTxt + " " + operatorType + " " + rightTxt + ")";
			}
            else if (xml.StartsWith("<CriteriaInputExpression"))
			{
                XmlNode child = condition.SelectSingleNode("*[@Key='InnerExpression']");
                string dummy;
                ConditionXmlToTxt.ParseCondition(child, out conditionTxt, out dummy, out dummy, out dummy);

			}
            else if (xml.StartsWith("<GroupExpression") ||
                 xml.StartsWith("<BinaryExpression Key=\"Expression\">"))
            {
                XmlNode child = condition.ChildNodes[0];
                string dummy;
                ConditionXmlToTxt.ParseCondition(child, out conditionTxt, out dummy, out dummy, out dummy);

            }
			else if (xml.StartsWith("<ConstantExpression"))
			{
                XmlNode DataType = condition.SelectSingleNode(".//DataType/@Value");
                string value = condition.SelectSingleNode(".//*[@Key='Value']/@Value").Value;
                if (DataType.Value == "String")
                    conditionTxt = "\"" + value + "\"";
                else
                    conditionTxt = value;
			}
            else if (xml.StartsWith("<StringConverterExpression"))
            {
                XmlNode child = condition.SelectSingleNode(".//*[@Key='Expression']");
                string value, dummy;
                ConditionXmlToTxt.ParseCondition(child, out value, out dummy, out dummy, out dummy);
                conditionTxt = value;
            }
            else if (xml.StartsWith("<VariableExpression"))
            {
                XmlNode varxml = condition.SelectSingleNode(".//String[@Key='Name']/@Value");
                conditionTxt = "{" + varxml.Value + "}";

            }
            else if (xml.StartsWith("<ExpressionDecorator"))
            {
                XmlNode decorator = condition.SelectSingleNode("./*");
                string dummy;
                ConditionXmlToTxt.ParseCondition(decorator, out conditionTxt, out dummy, out dummy, out dummy);
                if (conditionTxt.StartsWith("{") == false)
                    conditionTxt = "{" + conditionTxt;
                if (conditionTxt.EndsWith("}") == false)
                    conditionTxt = conditionTxt + "}";
            }
            else if (xml.StartsWith("<TrinaryExpression"))
            {

            }
            else if (xml.StartsWith("<StepResultExpression"))
            {
                XmlNode varxml = condition.SelectSingleNode(".//*[@Key='Result']/@Value");
                conditionTxt = "{" + varxml.Value + "}";
            }
            else if (xml.StartsWith("<FunctionCall"))
            {
                XmlNode function = condition.SelectSingleNode(".//*[@Key='FunctionName']");
                string funcname = function.Attributes["Value"].Value;
                XmlNode parameters = condition.SelectSingleNode(".//*[@Key='Parameters']/*");

                if (parameters != null)
                {
                    string paramsTxt, dummy;
                    ConditionXmlToTxt.ParseCondition(parameters, out paramsTxt, out dummy, out dummy, out dummy);

                    conditionTxt = funcname + "(" + paramsTxt + ")";
                }
                else
                {
                    conditionTxt = funcname;
                }
            }
            else if (xml.StartsWith("<IndexedVariableExpression") ||
                     xml.StartsWith("<IndexedMatrixVariableExpression") ||
                     xml.StartsWith("<MatrixSubColVariableExpression") ||
                     xml.StartsWith("<MatrixSubRowVariableExpression") ||
                     xml.StartsWith("<RangeIndexedMatrixVariableExpression") ||
                     xml.StartsWith("<RangeIndexedVariableExpression") ||                
                     xml.StartsWith("<RowRangeIndexedMatrixVariableExpression") ||
                     xml.StartsWith("<MatrixRowVariableExpressionLogicalTreeProvider") ||
                     xml.StartsWith("<UnaryExpression") ||
                     xml.StartsWith("<ConcatenationExpression") ||
                     xml.StartsWith("<VectorBuilder") ||
                     xml.StartsWith("<Null"))
            {
                conditionTxt = TextfieldToTxt.Parse(condition);
                return;
            }
            else if (xml.StartsWith("<ParameterExpressionBinder"))
            {

            }
            else if (xml.StartsWith("<CriteriaInputExpression"))
            {

            }
            else if (xml.StartsWith("<MatrixColVariableExpressionLogicalTreeProvider"))
            {
                conditionTxt = TextfieldToTxt.Parse(condition);
            }
            else if (xml.StartsWith("<MemberExpression"))
            {
                if (condition.FirstChild.Name=="MemberExpression")
                {
                    XmlNode Left = condition.SelectSingleNode(".//String[@Key='Name' or @Key='FunctionName']");
                    XmlNodeList Right = condition.SelectNodes(".//*[@Key='Right']");
                    conditionTxt = "{" + Left.Attributes["Value"].Value + "."; // +Right.Attributes["Value"].Value + "}";
                    int rc = 0;
                    foreach (XmlNode r in Right)
                    {
                        conditionTxt += r.Attributes["Value"].Value;
                        if (rc < Right.Count - 1)
                            conditionTxt += ".";
                        
                        rc++;
                    }
                    conditionTxt += "}";
                }
                else
                {
                    XmlNode Left = condition.SelectSingleNode(".//String[@Key='Name' or @Key='FunctionName']");
                    XmlNode Right = condition.SelectSingleNode("*[@Key='Right']");
                    conditionTxt = "{" + Left.Attributes["Value"].Value + "." + Right.Attributes["Value"].Value + "}";
                }
                
            }
            else
            {
                conditionTxt = "ConditionXmlToTxt.ParseCondition - unidentified condition: " + condition.Name;
            }

			if (conditionTxt.Length==0)
                conditionTxt = "ConditionXmlToTxt.ParseCondition - unhandled condition: " + condition.Name;
		}
		
		private static string ConvertOperator(string op)
		{
			switch (op)
			{
				case "Equals":
                case "EqualsCode":
					return "==";
				case "Add":
					return "+";
				case "NotEquals":
                case "NotEqualsCode":
					return "!=";
				case "GreaterThan":
                case "GreaterThanCode":
					return ">";
                case "GreaterOrEqual":
                case "GreaterOrEqualCode":
					return ">=";
				case "LessThan":
                case "LessThanCode":
					return "<";
				case "LessOrEqual":
                case "LessOrEqualCode":
                    return "<=";
				case "Subtract":
					return "-";
				case "Divide":
					return "/";
                case "Multiply":
                    return "*";
				case "And":
					return "&&";
				case "Or":
					return "||";
                case "GenerateVector":
                    return ":";
                case "Contains":
				case "DoesNotContain":
				case "StartsWith":
				case "EndsWith":
                case "Between":
                case "Not":
                case "BitAnd":
                case "BitOr":
                case "BitXor":
                case "ShiftRight":
                case "ShiftLeft":
					return op;
				default:
					return "unknown operator " + op;
			}

		}
	}
}
