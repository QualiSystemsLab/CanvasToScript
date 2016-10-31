using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace CanvasToScript.Common
{
    class Step
    {
        public int[] Rows;
        public int[] Cols;
        public string ToolName;
        public string StepName;
        public string Description;
        public bool Enabled;
        public XmlNode StepXmlNode;

        public Step(XmlNode step)
        {
            StepXmlNode = step;
            Rows = ExtractStepRows(step);
            Cols = ExtractStepCols(step);
            ToolName = ExtractStepToolName(step);
            StepName = ExtractStepName(step);
            Enabled = bool.Parse(ExtractStepEnabled(step));
            Description = ExtractStepDescription(step);
        }

        private int[] ExtractStepRows(XmlNode step)
        {
            XmlNodeList list = step.SelectNodes("(List/StepBlock/Int32[@Key='Row'])");
            int[] rows = new int[list.Count];
            int i = 0;
            foreach (XmlNode row in list)
            {
                rows[i] = int.Parse(row.Attributes["Value"].Value)+500;
                i++;
            }
            return rows;
        }

        private int[] ExtractStepCols(XmlNode step)
        {
            XmlNodeList list = step.SelectNodes("(List/StepBlock/Int32[@Key='Column'])");
            int[] cols = new int[list.Count];
            int i = 0;
            foreach (XmlNode col in list)
            {
                cols[i] = int.Parse(col.Attributes["Value"].Value)+500;
                i++;
            }
            return cols;
        }

        private string ExtractStepToolName(XmlNode step)
        {
            if (step.Name == "Step" || step.Name=="ContainerStep")
            {
                string xml = step.InnerXml;
                Regex regex = new Regex("^<(.+?) Key=\"Tool\">");
                Match m = regex.Match(xml);
                return m.Groups[1].Value;
            }
            else
                return step.Name;

            
        }

        private string ExtractStepName(XmlNode step)
        {
            string name = step.SelectSingleNode(".//String[@Key='Name']/@Value").Value;
            return name;

        }

        private string ExtractStepEnabled(XmlNode step)
        {
            XmlNode enabledNode = step.SelectSingleNode("Boolean[@Key='IsEnabled']");
            if (enabledNode != null)
                return enabledNode.Attributes["Value"].Value;
            else
                return "False";
        }

        private string ExtractStepDescription(XmlNode step)
        {
            string xml = step.InnerXml;
            Regex regex = new Regex("Key=\"Description\" Value=\"(.*?)\"");
            Match m = regex.Match(xml);
            return m.Groups[1].Value;
        }
        
        public bool TopConnected { 
            get {
                XmlNode node = StepXmlNode.SelectSingleNode("List//*[@Key='InputConnectors']/BlockConnector/Boolean[@Key='Connected']/@Value");
                
                return bool.Parse(node.Value);
            }
        }
        
        public int StepWidth {
        	get {
                XmlNode node = StepXmlNode.SelectSingleNode("List[@Key='Blocks']/StepBlock/*[@Key='Width']/@Value");
                
                return int.Parse(node.Value);
            }
        }

		public int StepHeight {
        	get {
                XmlNodeList nodes = StepXmlNode.SelectNodes("List[@Key='Blocks']/StepBlock/*[@Key='Row']/@Value");
                
                if (nodes.Count==1)
                {
                	return int.Parse(nodes[0].Value);
                }
                else if (nodes.Count==2)
                {
                	return Math.Abs(int.Parse(nodes[1].Value)-int.Parse(nodes[0].Value));
                }
                else
                	return -1;
            }
        }    
    }
}
