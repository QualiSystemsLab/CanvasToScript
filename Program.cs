using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CanvasToScript.Drivers;
using System.IO;
using System.Xml;
using System.Xml.XPath;


namespace CanvasToScript
{
    class Program
    {
        
        static void Main(string[] args)
        {
            if (args.Length>0)
            {

                try
                {
                    if (args[0] == "-f")
                    {
                        if (args[1].EndsWith("tstest"))
                        {
                            //if a test file was provided
                            MainCanvasToTxt.Analyze(args[1]);
                            Console.WriteLine("complete");
                        }
                        else if (args[1].EndsWith("tsdrv"))
                        {
                            //if a driver file was provided
                            MainCanvasToTxt.Analyze(args[1]);
                            Console.WriteLine("complete");
                        }
                        else
                        {
                            Console.WriteLine("Unknown file type: " + args[1]);
                            Console.WriteLine("Only .tsdrv and .tstest are supported when using the -f switch");
                        }
                    }
                    else if (args[0] == "-ff" || args[0]=="-fff")
                    {
                        if (Directory.Exists(args[1]))
                        {
                            var files = Directory.GetFiles(args[1], "*.tsdrv", ((args[0] == "-fff") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                            foreach (string file in files)
                            {
                                if (file.Contains("..svnbridge\\") == false)
                                {
                                    MainCanvasToTxt.Analyze(file);
                                    Console.WriteLine(file);
                                }
                            }
                            files = Directory.GetFiles(args[1], "*.tstest", ((args[0] == "-fff") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                            foreach (string file in files)
                            {
                                if (file.Contains("..svnbridge\\") == false)
                                {
                                    MainCanvasToTxt.Analyze(file);
                                    Console.WriteLine(file);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Folder not found: " + args[1]);
                        }
                    }
                    else if (args[0] == "-p")
                    {
                        //if a project was provided
                        if (args[1].EndsWith("tsdrvproj"))
                        {
                            

                            //get files from the root folder
                            List<string> tsdrvFiles = new List<string>();
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(File.ReadAllText(args[1]).Replace(" xmlns=\"http://www.qualisystems.com/\"", ""));

                            XmlNodeList files = doc.SelectNodes(".//ContainerManifest/Dictionary/KeyValuePair/String[@Key='HashKey']/@Value");
                            string projRoot = Path.GetDirectoryName(args[1]);
                            foreach (XmlNode f in files)
                            {
                                string fname = f.Value;
                                if (fname.ToLower().EndsWith(".tsdrv"))
                                    tsdrvFiles.Add(projRoot + "\\" + fname);
                            }

                            //get files from the sub folders
                            var manifests = Directory.GetFiles(Path.GetDirectoryName(args[1]), ".manifest", SearchOption.AllDirectories);
                            foreach (string m in manifests)
                            {
                                doc.LoadXml(File.ReadAllText(m).Replace(" xmlns=\"http://www.qualisystems.com/\"", ""));

                                files = doc.SelectNodes(".//ContainerManifest/Dictionary/KeyValuePair/String[@Key='HashKey']/@Value");
                                string folder = Path.GetDirectoryName(m);
                                foreach (XmlNode f in files)
                                {
                                    string fname = f.Value;
                                    if (fname.ToLower().EndsWith(".tsdrv"))
                                        tsdrvFiles.Add(folder + "\\" + fname);
                                }
                            }

                            //analyze files
                            foreach (string file in tsdrvFiles)
                            {

                                MainCanvasToTxt.Analyze(XmlConvert.DecodeName(file));
                                Console.WriteLine(file);
                            }

                            Console.WriteLine("complete");
                        }
                        else
                        {
                            Console.WriteLine("Unknown file type: " + args[1]);
                            Console.WriteLine("Only .tsdrvproj is supported when using the -p switch");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unknown switch: " + args[0]);
                        ShowUsage();
                    }

                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Missing file path");
                ShowUsage();
            }

            
            
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("-f filepath [this can be only a tstest file or a tsdrv file]\r\n");
            Console.WriteLine("-p filepath [this can be only a tsdrvproj file]\r\n");
            Console.WriteLine("-ff folderpath [this can be only a folder, just tstest and tsdrv files in that folder will be processed]\r\n");
            Console.WriteLine("-fff folderpath [this can be only a folder, tsdrv and tstest files in that folder and sub-folders will be processed]\r\n");
            Console.WriteLine("Results will be created at the same folder with a .script extension");
        }
    }
}
