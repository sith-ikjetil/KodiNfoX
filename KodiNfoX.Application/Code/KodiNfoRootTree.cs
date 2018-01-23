using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace KodiNfoX.Application.Code
{
    public class KodiNfoRootTree  
    {
        public static string Genre;
        public static string Title;
        public static string Year;
        
        private string m_rootPath;        
        public string RootPath { get { return m_rootPath;  } }
        
        public KodiNfoRootTreeItem Item { get; set; }

        public KodiNfoRootTree(string rpath)
        {
            this.m_rootPath = rpath;

            string[] parts = rpath.Split(new char[]{ '\\','/' } );

            KodiNfoRootTreeItem lastItem = null;
            int index = 0;
            if (rpath.StartsWith("\\"))
            {
                index = 2;
                Log.WriteInformation("Root Path is UNC path.");
                this.Item = new KodiNfoRootTreeItem(parts[2], KodiNfoElementType.FileSystem);
                lastItem = this.Item;
            }
            
            index++;

            for (; index < parts.Length; index++)
            {
                KodiNfoElementType type = GetKodiNfoElementType(parts[index]);
                if (type == KodiNfoElementType.FileSystem)
                {
                    if (this.Item == null)
                    {
                        this.Item = new KodiNfoRootTreeItem(parts[index], KodiNfoElementType.FileSystem);
                        lastItem = this.Item;
                    }
                    else
                    {
                        lastItem.Item = new KodiNfoRootTreeItem(parts[index], KodiNfoElementType.FileSystem);
                        lastItem = lastItem.Item;                        
                    }
                }
                else
                {
                    if (this.Item == null)
                    {
                        this.Item = new KodiNfoRootTreeItem(parts[index], type);
                        lastItem = this.Item;
                    }
                    else
                    {
                        lastItem.Item = new KodiNfoRootTreeItem(parts[index], type);
                        lastItem = lastItem.Item;
                    }
                }
            }            
        }

        private KodiNfoElementType GetKodiNfoElementType(string p)
        {
            if (p == "[[genre]]")
            {
                Log.WriteInformation("Found path part genre.");
                return KodiNfoElementType.Genre;
            }
            else if (p == "[[year]]")
            {
                Log.WriteInformation("Found path part year.");
                return KodiNfoElementType.Year;
            }
            else if (p == "[[title]]")
            {
                Log.WriteInformation("Found path part title.");
                return KodiNfoElementType.Title;
            }
            return KodiNfoElementType.FileSystem;
        }

       

        public void CreateNfoFiles() 
        {
            if ( this.Item == null ) {
                throw new Exception("Item is NULL.");
            }            
            
            // Load all directories
            string currentPathRoot = string.Empty;            
            
            if (this.Item.Type != KodiNfoElementType.FileSystem)
            {
                string errorMessage = "Invalid Path Part '" + this.Item.DirectoryName + "'.";                        
                throw new Exception(errorMessage);
            }
            else 
            {
                if (this.m_rootPath.StartsWith("\\\\"))
                {
                    currentPathRoot = "\\\\";
                }
                else
                {
                    currentPathRoot = this.m_rootPath.Split(new char[] { '\\', '/' })[0] + "\\";
                }
            }

            this.Item.ProcessNfoFiles(currentPathRoot);            
        }

        public void DeleteNfoFiles(DeleteNfoParams deleteNfoParams)
        {
            if (this.Item == null)
            {
                throw new Exception("Item is NULL.");
            }            

            // Load all directories
            string currentPathRoot = string.Empty;

            if (this.Item.Type != KodiNfoElementType.FileSystem)
            {
                string errorMessage = "Invalid Path Part '" + this.Item.DirectoryName + "'.";
                throw new Exception(errorMessage);
            }
            else
            {
                if (this.m_rootPath.StartsWith("\\\\"))
                {
                    currentPathRoot = "\\\\";
                }
                else
                {
                    currentPathRoot = this.m_rootPath.Split(new char[] { '\\', '/' })[0] + "\\";
                }
            }

            this.Item.ProcessDeleteNfoFiles(currentPathRoot, deleteNfoParams);            
        }
    }
}
