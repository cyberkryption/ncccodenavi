﻿/*
Released as open source by NCC Group Plc - http://www.nccgroup.com/

Developed by Ollie Whitehouse, ollie dot whitehouse at nccgroup dot com

http://www.github.com/nccgroup/ncccodenavi

Released under AGPL see LICENSE for more information
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Win.CodeNavi
{

    /// <summary>
    /// Class for each check
    /// </summary>
    public class Grepifyv2Check
    {
        public string strName = null;
        public string strDescription = null;
        public string strRegex = null;
        public string strExts = null;

        public Grepifyv2Check(string strExts){
            this.strExts = strExts;
        }
    }

    /// <summary>
    /// Class for each file
    /// </summary>
    class Grepifyv2File 
    {
        public string strExts = null;
        public List<Grepifyv2Check> myChecks = new List<Grepifyv2Check>();

        // Constructor
        public Grepifyv2File(String strFile) 
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(strFile);
            XmlNodeList xmlExts = xmlDoc.GetElementsByTagName("Extensions");
            if (xmlExts.Count == 1)
            {
                strExts = xmlExts[0].InnerText;
            }
            else
            {
                return;
            }
            

            XmlNodeList xmlChecks = xmlDoc.GetElementsByTagName("Check");

            foreach(XmlNode xmlNode in xmlChecks){
                Grepifyv2Check myCheck = new Grepifyv2Check(strExts);

                foreach (XmlNode xmlSubNode in xmlNode.ChildNodes)
                {

                    if(xmlSubNode.Name.Equals("Regex")){
                        myCheck.strRegex = xmlSubNode.InnerText;
                    }

                    if(xmlSubNode.Name.Equals("Friendly")){
                        myCheck.strName = xmlSubNode.InnerText;
                    }

                    if(xmlSubNode.Name.Equals("Description")){
                        myCheck.strDescription = xmlSubNode.InnerText;
                    }

                }

                // We need them all
                if (myCheck.strRegex != null && myCheck.strName != null && myCheck.strDescription != null)
                {
                    // Check the regex is valid before adding it
                    try
                    {
                        Match regexMatch = Regex.Match("Mooo", myCheck.strRegex);
                        myChecks.Add(myCheck);
                    }
                    catch (ArgumentException rExcp)
                    {
                        MessageBox.Show("Regex looks broken, Regex is '" + myCheck.strRegex + "'. Error is '" + rExcp.Message + "' in file " + strFile + ".", "Regex error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }
        }

    }

    /// <summary>
    /// The main class
    /// </summary>
    class Grepifyv2
    {
        public List<Grepifyv2File> myFiles = new List<Grepifyv2File>();

        public int CheckCount()
        {
            int intCount = 0;
            try{
                foreach (Grepifyv2File gv2File in myFiles)
                {
                    intCount += gv2File.myChecks.Count();
                }
            
            } catch(Exception){

            }

            return intCount;
        }

        public string GetExts()
        {
            StringBuilder strExts = new StringBuilder();
            
            try
            {
                foreach (Grepifyv2File gv2File in myFiles)
                {
                    if(strExts.Length == 0){
                        strExts.Append(gv2File.strExts);
                    } else {
                        strExts.Append(";" + gv2File.strExts);
                    }
                }
            }
            catch (Exception)
            {

            }

            if (strExts.Length > 0)
            {
                return strExts.ToString();
            }
            else
            {
                return null;
            }
        }

        public List<Grepifyv2Check> GetChecks()
        {

            if (CheckCount() == 0) return null;

            List<Grepifyv2Check> lstMaster = new List<Grepifyv2Check>();

            try
            {
                foreach (Grepifyv2File gv2File in myFiles)
                {
                    foreach (Grepifyv2Check gCheck in gv2File.myChecks)
                    {
                        lstMaster.Add(gCheck);
                    }
                }

            }
            catch (Exception)
            {
                return null;
            }

            return lstMaster;
        }

        // Constructor
        public Grepifyv2(String strDirectory)
        {
            Console.WriteLine("Grepify v2");

            foreach (String fileXML in Directory.GetFiles(strDirectory,"*.grepifyv2"))
            {
                try
                {
                    Grepifyv2File grepv2File = new Grepifyv2File(fileXML);
                    myFiles.Add(grepv2File);
                }
                catch (Exception)
                {

                }
            }
        }

        public Grepifyv2(List<String> strFilenames)
        {

            foreach (String fileXML in strFilenames)
            {
                try
                {
                    Grepifyv2File grepv2File = new Grepifyv2File(fileXML);
                    myFiles.Add(grepv2File);
                }
                catch (Exception)
                {

                }
            }
        }

    }
}
