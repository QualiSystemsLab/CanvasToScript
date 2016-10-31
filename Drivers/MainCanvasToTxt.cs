using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using CanvasToScript.Common;
using CanvasToScript.Parsers;


namespace CanvasToScript.Drivers
{
    public static class MainCanvasToTxt
    {
        public static void Analyze(string xmlFilePath)
        {
            XmlDocument xmlFile;
            
            try
            {
                xmlFile = new XmlDocument();
                xmlFile.LoadXml(File.ReadAllText(xmlFilePath).Replace(" xmlns=\"http://www.qualisystems.com/\"",""));

            }
            catch
            {
                throw new Exception("Can't read source file");
            }

            string xmlroot = "";
            string modificationDate="";
            string description="";
            if (xmlFilePath.ToLower().EndsWith(".tstest"))
            {
                xmlroot = "TestInfo";
                //modificationDate = xmlFile.SelectSingleNode("//TestInfo/DateTime[@Key='ModificationDate']/@Value").Value;
                description = xmlFile.SelectSingleNode("//Header/*[@Key='Description']/@Value").Value;
            }
            else if (xmlFilePath.ToLower().EndsWith(".tsdrv"))
            {
                xmlroot = "DriverFunctionInfoData";
                //modificationDate = xmlFile.SelectSingleNode("//DriverFunctionInfoMetadata/DateTime[@Key='ModificationDate']/@Value").Value;
                description = xmlFile.SelectSingleNode("//DriverFunctionInfoMetadata/*[@Key='Description']/@Value").Value;
            }

            XmlNodeList steps = xmlFile.SelectNodes("//" + xmlroot + "/List[@Key='Steps']/*");
            XmlNodeList variables = xmlFile.SelectNodes("//" + xmlroot + "/Array[@Key='Variables']/*");
            XmlNodeList interfaceXmls = xmlFile.SelectNodes("//TestPrototype[@Key='Prototype']/List[@Key='Parameters']/*");
            
            string functionInterface = AnalyzeInterface(Path.GetFileNameWithoutExtension(xmlFilePath), description, modificationDate, interfaceXmls);
            string userVariables = AnalyzeVariables(variables);

            string ThreadLogs, ThreadFunctions;
            XmlNode eh = xmlFile.SelectSingleNode("//" + xmlroot + "/ErrorHandlingBehavior");
            CanvasAnalyzer.StepsListToTxt(steps, false, 0, eh, out ThreadLogs, out ThreadFunctions);

            ThreadLogs = functionInterface + "\r\n" +
                        ((userVariables.Length > 0) ? ("#region /* User defined variables */" + "\r\n" + userVariables + "\r\n" + "#endregion" + "\r\n\r\r") : "\r\n") +
                        "/* Main flow/s */\r\n" + ThreadLogs;

            string script = ConvertScriptTexts(ThreadLogs) + ConvertScriptTexts(ThreadFunctions.Trim().Length > 0 ? ("\r\n\r\n" + "/* Functions */\r\n" + ThreadFunctions) : "");
            if (xmlFilePath.EndsWith("tsdrv"))
            {
                script = script.Replace(" /*[Save Results]*/", "");
            }

            File.WriteAllText(xmlFilePath + ".script", script);            
        }

        private static string AnalyzeInterface(string funcName, string description, string modificationDate, XmlNodeList interfaceXmls)
        {
            /// <summary>
            /// Connects to the database and attempts to apply 
            /// all adds, updates and deletes
            /// </summary>
            /// <param name="data">a dataset, passed by reference, 
            /// that contains all the 
            /// data for updating</param>

            string prototype = "";
            string header = "";

            if (description.Length > 0)
            {
                header = "/// <summary>" + "\r\n";
                header += "/// " + description.Replace("_x000D__x000A_", "\r\n/// ") + "\r\n";
                header += "/// </summary>" + "\r\n";
            }
            if (modificationDate.Length > 0)
            {
                header = "/// <remarks>" + "\r\n";
                header += "/// Modification date: " + modificationDate + "\r\n";
                header += "/// </remarks>" + "\r\n";
            }

            int i = 0;
            foreach (XmlNode item in interfaceXmls)
            {
                string name = item.SelectSingleNode("String[@Key='Name']/@Value").Value;
                string pdesc = item.SelectSingleNode("*[@Key='Description']/@Value").Value;
                string publish = item.SelectSingleNode("*[@Key='PublishType']/@Value").Value;
                header += "/// <param name=\"" + name + "\">\r\n";
                if (pdesc.Length > 0)
                {
                    header += "/// " + pdesc.Replace("_x000D__x000A_", "\r\n/// ") + "\r\n";
                }
                header += "/// </param>" + "\r\n";

                if (publish == "InOut") prototype += "ref ";
                if (publish == "Out") prototype += "out ";

                string ptype;
                string pdimension;
                string penum="";
                if (item.SelectSingleNode("*[@Key='DataType']").Name == "IOStructureDataType")
                {
                    ptype = "Structure";
                    pdimension = "Scalar";                    
                }
                else
                {
                    ptype = item.SelectSingleNode("*[@Key='DataType']/@Value").Value;
                    pdimension = item.SelectSingleNode("*[@Key='DataDimension']/@Value").Value;
                    if (item.SelectSingleNode("*[@Key='EnumTypeName']")!=null)
                        penum = item.SelectSingleNode("*[@Key='EnumTypeName']/@Value").Value;

                    if (penum.Length > 0) ptype = penum;
                    if (ptype == "String") ptype = "string";
                    if (ptype == "Numeric") ptype = "double";
                    if (pdimension == "Matrix") pdimension = "[,]";
                    if (pdimension == "Vector") pdimension = "[]";
                }
                

                prototype += ptype + " " + ((pdimension=="Scalar")?"":(pdimension + " ")) + name + ((i<interfaceXmls.Count-1)? ", ":"");
                i++;
            }

            return header + "\r\n" + funcName + "(" + prototype + ")";
        }

        private static string AnalyzeVariables(XmlNodeList variables)
        {
            string output = "";

            foreach (XmlNode varxml in variables)
            {
                XmlNode dtxml = varxml.SelectSingleNode(".//*[@Key='Type']");
                string datatype="";
                if (dtxml.Name == "DataType")
                    datatype = dtxml.SelectSingleNode("@Value").Value;
                else if (dtxml.Name == "EventDataType")
                    datatype = "Event";

                string name = varxml.SelectSingleNode(".//String[@Key='Name']/@Value").Value;
                string description = varxml.SelectSingleNode(".//*[@Key='Description']/@Value").Value;
                string dimension = varxml.Name;
                switch (dimension)
                {
                    case "MatrixVariable":
                        dimension = "[,]";
                        break;
                    case "VectorVariable":
                        dimension = "[]";
                        break;
                    case "SingleVariable":
                    case "EventVariable":
                    case "SessionVariable":
                    case "LockVariable":
                        dimension = "";
                        break;
                    default:
                        break;
                }

                string initialValue = "";
                switch (datatype)
                {
                    case "String":                        
                    case "Numeric":
                        initialValue = TextfieldToTxt.Parse(varxml.SelectSingleNode(".//*[@Key='InitialValue']"));
                        if (datatype == "String") datatype = "string";
                        if (datatype == "Numeric") datatype = "double";
                        break;
                    case "Event":
                        initialValue = "\"Off\"";
                        break;
                    default:
                        initialValue = "";
                        break;
                }
                
                if (datatype == "string")
                    initialValue = "\"" + initialValue + "\"";

                if (description.Length > 0)
                    output += "/*" + description + "*/" + "\r\n";

                output += datatype + ((dimension.Length>0) ? (dimension + " "):" ") + name + ((initialValue.Length>0)?(" = " + initialValue):"") + "\r\n";
                
            }

            return output;
        }

        private static string ConvertScriptTexts(string input)
        {
            string output = input;
            
            output = output.Replace("\r\n\r\n","\r\n");
            output = output.Replace("\r\r", "\r\n");
            output = output.Replace("NoOpTool","NopTool");
            output = output.Replace("PaWDllTool", "DllTool");
            output = output.Replace("Tool: ", ": ");

            output = output.Replace("Output_x0020__x003D__x0020_", "Output");
            
            output = Regex.Replace(output, "}\r\n\r\n","}\r\n");
            
            output = XmlConvert.DecodeName(output);

            return output;
        }
    }
}
