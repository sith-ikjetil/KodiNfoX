using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using ItSoftware.Core;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Linq;
using System.Globalization;

namespace KodiNfoX.Application.Code
{
    public class KodiNfoRootTreeItem
    {
        public KodiNfoElementType Type;
        public string DirectoryName;
        public string[] Files;
        public string[] Directories;
        public KodiNfoRootTreeItem Item;

        public KodiNfoRootTreeItem(string directoryPart, KodiNfoElementType type)
        {
            this.Type = type;
            this.DirectoryName = directoryPart;
        }

        public string[] LoadDirectories(string currentPathRoot)
        {
            List<string> directoriesFound = new List<string>();
            try
            {
                directoriesFound = Directory.GetDirectories(currentPathRoot).ToList();
                directoriesFound = directoriesFound.Where(f => { return (f != "." && f != ".."); }).ToList();
                foreach (string dir in directoriesFound)
                {
                    Log.WriteInformation(string.Format("FOUND directory {0}",dir));
                }
            }
            catch (Exception)
            {
                if (currentPathRoot.StartsWith("\\\\") && !string.IsNullOrEmpty(App.Username))
                {
                    Log.WriteInformation("Trying to connect to: " + currentPathRoot);
                    NetResource resource = new NetResource();
                    resource.dwType = ResourceType.RESOURCETYPE_DISK;
                    resource.lpLocalName = null;
                    resource.lpRemoteName = currentPathRoot;
                    resource.lpProvider = null;

                    int result = WNetAddConnection2(resource, App.Password, App.Username, CONNECT_TEMPORARY);
                    if (result == NO_ERROR)
                    {
                        Log.WriteInformation(string.Format("CONNECTION to share {0} SUCCEEDED", currentPathRoot));
                    }
                    else
                    {
                        Log.WriteError(string.Format("CONNECTION to share {0} FAILED", currentPathRoot));
                        Log.WriteException(new Win32Exception(Marshal.GetLastWin32Error()));
                    }
                }
                WIN32_FIND_DATA wfd;
                //IntPtr hFile = FindFirstFile(Path.Combine(currentPathRoot,"*"),out wfd);
                IntPtr hFile = FindFirstFileEx(Path.Combine(currentPathRoot, "*"), FINDEX_INFO_LEVELS.FindExInfoStandard, out wfd, FINDEX_SEARCH_OPS.FindExSearchLimitToDirectories, IntPtr.Zero, 0);
                if (hFile != INVALID_HANDLE_VALUE)
                {
                    if ((wfd.dwFileAttributes & 0x10) > 0 && wfd.cFileName != "." && wfd.cFileName != "..")
                    {
                        Log.WriteInformation(string.Format("FOUND directory {0}", wfd.cFileName));
                        directoriesFound.Add(wfd.cFileName);
                    }
                    while (FindNextFile(hFile, out wfd))
                    {
                        if ((wfd.dwFileAttributes & 0x10) > 0 && wfd.cFileName != "." && wfd.cFileName != "..")
                        {                            
                            Log.WriteInformation(string.Format("FOUND directory {0}", wfd.cFileName));
                            directoriesFound.Add(wfd.cFileName);
                        }
                    }
                    FindClose(hFile);
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            return directoriesFound.ToArray();
        }
        public const int CONNECT_TEMPORARY = 4;
        public const int NO_ERROR = 0;

        public enum ResourceScope
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
            RESOURCE_RECENT,
            RESOURCE_CONTEXT
        };

        public enum ResourceType
        {
            RESOURCETYPE_ANY = 0,
            RESOURCETYPE_DISK = 1,
            RESOURCETYPE_PRINT = 2,
            RESOURCETYPE_RESERVED = 8,
            RESOURCETYPE_UNKNOWN = -1,
        };

        public enum ResourceUsage
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010,
            RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
        };

        public enum ResourceDisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        };

        [StructLayout(LayoutKind.Sequential)]
        public class NetResource
        {
            public ResourceScope dwScope;
            public ResourceType dwType;
            public ResourceDisplayType dwDisplayType;
            public ResourceUsage dwUsage;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        }

        [DllImport("mpr.dll")]
        public static extern int WNetAddConnection2(NetResource netResource,
           string password, string username, uint flags);

        public IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public enum FINDEX_INFO_LEVELS
        {
            FindExInfoStandard = 0,
            FindExInfoBasic = 1
        }

        public enum FINDEX_SEARCH_OPS
        {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories = 1,
            FindExSearchLimitToDevices = 2
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr FindFirstFileEx(
            string lpFileName,
            FINDEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FIND_DATA lpFindFileData,
            FINDEX_SEARCH_OPS fSearchOp,
            IntPtr lpSearchFilter,
            int dwAdditionalFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA vlpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        public string[] LoadFiles(string currentPathRoot)
        {
            List<string> filesFound = new List<string>();

            foreach (var ext in App.Extensions)
            {
                string extension = ext.Trim();
                if (extension == null)
                {
                    extension = "";
                }
                if (extension.Length > 1)
                {
                    if (extension[0] != '.')
                    {
                        extension = "*." + extension;
                    }
                    else
                    {
                        extension = "*" + extension;
                    }
                }
                else
                {
                    string errorMessage = "Could not find an extension to file filter.";
                    Log.WriteError(errorMessage);
                    throw new Exception(errorMessage);
                }

                try
                {
                    string[] ff = Directory.GetFiles(currentPathRoot, extension);
                    foreach (string dir in ff)
                    {
                        Log.WriteInformation(string.Format("FOUND file {0}",dir));
                    }
                    filesFound.AddRange(ff);
                }
                catch (Exception)
                {
                    if (currentPathRoot.StartsWith("\\\\") && !string.IsNullOrEmpty(App.Username))
                    {
                        NetResource resource = new NetResource();
                        resource.dwType = ResourceType.RESOURCETYPE_DISK;
                        resource.lpLocalName = null;
                        resource.lpRemoteName = currentPathRoot;
                        resource.lpProvider = null;

                        int result = WNetAddConnection2(resource, App.Password, App.Username, CONNECT_TEMPORARY);
                        if (result == NO_ERROR)
                        {
                            Log.WriteInformation(string.Format("CONNECTION to share {0} SUCCEEDED", currentPathRoot));
                        }
                        else
                        {
                            Log.WriteError(string.Format("CONNECTION to share {0} FAILED", currentPathRoot));
                            Log.WriteException(new Win32Exception(Marshal.GetLastWin32Error()));
                        }
                    }
                    WIN32_FIND_DATA wfd;
                    //IntPtr hFile = FindFirstFile(Path.Combine(currentPathRoot,"*"),out wfd);
                    IntPtr hFile = FindFirstFileEx(Path.Combine(currentPathRoot, extension), FINDEX_INFO_LEVELS.FindExInfoStandard, out wfd, FINDEX_SEARCH_OPS.FindExSearchLimitToDirectories, IntPtr.Zero, 0);
                    if (hFile != INVALID_HANDLE_VALUE)
                    {
                        if ((wfd.dwFileAttributes & 0x10) == 0 && wfd.cFileName != "." && wfd.cFileName != "..")
                        {
                            Log.WriteInformation(string.Format("FOUND file {0} under {1}", wfd.cFileName, currentPathRoot));
                            filesFound.Add(wfd.cFileName);
                        }
                        while (FindNextFile(hFile, out wfd))
                        {
                            if ((wfd.dwFileAttributes & 0x10) == 0 && wfd.cFileName != "." && wfd.cFileName != "..")
                            {
                                Log.WriteInformation(string.Format("FOUND file {0} under {1}", wfd.cFileName, currentPathRoot));
                                filesFound.Add(wfd.cFileName);
                            }
                        }
                        FindClose(hFile);
                    }
                    else
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }// foreach ext
            return filesFound.ToArray();
        }

        internal void ProcessNfoFiles(string currentPathRoot)
        {
            string[] directories = null;
            if (this.Type != KodiNfoElementType.FileSystem)
            {
                directories = this.LoadDirectories(currentPathRoot);
                App.CountFoundDirectories += directories.Length;
            }
            else
            {
                directories = new string[] { this.DirectoryName };
                App.CountFoundDirectories += directories.Length;
            }

            foreach (var folder in directories)
            {
                this.SetInformation(Path.GetFileNameWithoutExtension(folder));
                string newPath = Path.Combine(currentPathRoot, folder);
                if (this.Item != null)
                {
                    this.Item.ProcessNfoFiles(newPath);
                }
                else
                {
                    this.ProcessFolder(newPath);
                }
            }
        }

        internal void SetInformation(string value)
        {
            switch (this.Type)
            {
                case KodiNfoElementType.Genre:
                    KodiNfoRootTree.Genre = value;
                    break;
                case KodiNfoElementType.Title:
                    KodiNfoRootTree.Title = value;
                    break;
                case KodiNfoElementType.Year:
                    KodiNfoRootTree.Year = value;
                    break;
                default:
                    break;
            }
        }

        internal void ProcessFolder(string currentPathRoot)
        {
            string[] files = this.LoadFiles(currentPathRoot);
            App.CountFoundFiles += files.Length;
            foreach (var file in files)
            {
                this.ProcessFile(Path.Combine(currentPathRoot, file));
            }

            string[] directories = this.LoadDirectories(currentPathRoot);
            App.CountFoundDirectories += directories.Length;
            foreach (var folder in directories)
            {
                this.ProcessFolder(Path.Combine(currentPathRoot, folder));
            }
        }

        private void ProcessFile(string pathAndFile)
        {
            string path = Path.GetDirectoryName(pathAndFile);
            string filename = Path.GetFileName(pathAndFile);
            string filenameNE = Path.GetFileNameWithoutExtension(pathAndFile);

            string nfoFilename = Path.Combine(path, filenameNE) + ".nfo";
            bool bExists = File.Exists(nfoFilename);
            if (bExists && !App.ReplaceExisting)
            {
                Log.WriteInformation(string.Format("FILE {0} already exists. Options is to NOT REPLACE", nfoFilename));
                return;
            }

            int indexLastDot = filename.LastIndexOf('.');
            if (indexLastDot >= 0)
            {
                filename = filename.Replace('.', ':');
                char[] cArray = filename.ToArray();
                cArray[indexLastDot] = '.';
                filename = new string(cArray);
            }

            var dicFileInfo = ItsString.ApplyTagTemplate(string.Format("-{0}-",filename), string.Format("-{0}-", App.FileFilter), "[[", "]]");

            if (indexLastDot >= 0)
            {
                filename = filename.Replace(':', '.');
            }

            if (dicFileInfo.Count == 0)
            {
                Log.WriteWarning(string.Format("Could not apply tag template to file {0}", filename) );
                return;
            }

            string genre = KodiNfoRootTree.Genre ?? string.Empty;
            string title = KodiNfoRootTree.Title ?? string.Empty;
            string year = KodiNfoRootTree.Year ?? string.Empty;
            foreach (var dict in dicFileInfo)
            {
                if (dict.ContainsKey("genre"))
                {
                    var g = dict["genre"];
                    if (g.Count == 1)
                    {
                        genre = g[0].Replace(':', '.');
                    }
                }
                if (dict.ContainsKey("title"))
                {
                    var t = dict["title"];
                    if (t.Count == 1)
                    {
                        title = t[0].Replace(':', '.'); ;
                    }
                }
                if (dict.ContainsKey("year"))
                {
                    var y = dict["year"];
                    if (y.Count == 1)
                    {
                        year = y[0].Replace(':', '.'); ;
                    }
                }
            }

            title = this.CleanTitle(title);

            int iYear = -1;
            if (!int.TryParse(year, out iYear))
            {
                Log.WriteWarning(string.Format("Could not identify year {0} for file {1}", year, pathAndFile ));
                return;
            }

            string thumb;
            string plot;
            string rating;
            Actor[] actor;
            string[] director;
            string[] genres;
            string[] writer;
            string[] producer;
            
            this.GetAdditional(pathAndFile, title, year, out thumb, out plot, out genres, out actor, out director, out writer, out producer, out rating);            
            
            if (!string.IsNullOrEmpty(genre) && !string.IsNullOrWhiteSpace(genre) &&
                genres.Length == 0)
            {
                genres = new string[] { genre };
            }
            KodiNfoXml knx = new KodiNfoXml(title, title, genres, year, thumb, plot, actor, director, writer, rating, producer);
            try
            {
                knx.ToXDocument().Save(nfoFilename);
                App.CountNfoFilesWritten++;
                Log.WriteInformation(string.Format("CREATION NFO file {0} SUCCEEDED", nfoFilename));
            }
            catch (Exception)
            {
                App.CountNfoFilesFailedWritten++;
                Log.WriteWarning(string.Format("CREATION NFO file {0} FAILED", nfoFilename));
            }
        }

        private void GetAdditional(string pathAndFile, string title, string year, out string thumb, out string plot, out string[] genres, out Actor[] actors, out string[] director, out string[] writer, out string[] producer, out string rating)
        {
            // Set out values.
            thumb = string.Empty;
            plot = string.Empty;
            rating = string.Empty;
            genres = new string[0];
            actors = new Actor[0];
            director = new string[0];
            producer = new string[0];
            writer = new string[0];

            int retryCount = 5;
            bool bRetry = false;
        retryPoint:
            try
            {                
                string apiKey = "a8051abec55b1993cc89184181b3d6a3";

                System.Net.TMDb.ServiceClient client = new System.Net.TMDb.ServiceClient(apiKey);
                var result = client.Movies.SearchAsync(title, "en", true, int.Parse(year), 1, System.Threading.CancellationToken.None);

                result.Wait();

                var searchResult = result.Result;

                if (searchResult.TotalCount >= 1)
                {
                    App.CountTmdbHits++;

                    int id = searchResult.Results.First().Id;

                    var result2 = client.Movies.GetAsync(id, "en", true, System.Threading.CancellationToken.None);

                    result2.Wait();

                    var searchResult2 = result2.Result;

                    // Get plot                                   
                    plot = searchResult2.Overview;

                    // Get Thumb                                   
                    thumb = "http://image.tmdb.org/t/p/w500" + searchResult2.Poster;

                    // Get Rating
                    rating = searchResult2.Popularity.ToString(new CultureInfo("en-US"));

                    // Get genres
                    if (searchResult2.Genres != null)
                    {
                        genres = searchResult2.Genres.Select(g => g.Name).ToArray();
                    }

                    // Get actors
                    if (searchResult2.Credits != null)
                    {
                        if (searchResult2.Credits.Cast != null)
                        {
                            actors = searchResult2.Credits.Cast.Select(c => new Actor() { Name = c.Name, Role = c.Character }).ToArray();
                        }
                    }

                    // Get director
                    if (searchResult2.Credits != null)
                    {
                        if (searchResult2.Credits.Crew != null)
                        {
                            director = searchResult2.Credits.Crew.Where(c => c.Job == "Director").Select(c => c.Name).ToArray();
                        }
                    }

                    // Get Writer
                    if (searchResult2.Credits != null)
                    {
                        if (searchResult2.Credits.Crew != null)
                        {
                            writer = searchResult2.Credits.Crew.Where(c => c.Department == "Writing").Select(c => c.Name).ToArray();
                        }
                    }

                    // Get Producer
                    if (searchResult2.Credits != null)
                    {
                        if (searchResult2.Credits.Crew != null)
                        {
                            producer = searchResult2.Credits.Crew.Where(c => c.Job == "Producer").Select(c => c.Name).ToArray();
                        }
                    }
                    Log.WriteInformation(string.Format("METADATA for file {0} FOUND", pathAndFile));
                }
                else
                {
                    App.CountTmdbMiss++;
                    Log.WriteWarning(string.Format("METADATA for file {0} NOT FOUND",pathAndFile));
                }
            }
            catch (System.AggregateException)
            {
                if (--retryCount >= 1)
                {
                    bRetry = true;
                    Log.WriteInformation(string.Format("The request limit was exceeded for '{0}', taking a pause and retrying...", pathAndFile));                    
                }
                else
                {
                    bRetry = false;
                    App.CountTooManyTmdbHits++;
                    Log.WriteWarning(string.Format("The request limit was exceeded for '{0}'", pathAndFile));
                }
            }

            if (bRetry)
            {
                bRetry = false;
                System.Threading.Thread.Sleep(1000);
                goto retryPoint;
            }
        }

        private string CleanTitle(string title)
        {
            title = title.Replace("-CD1", string.Empty);
            title = title.Replace("-CD2", string.Empty);
            title = title.Replace("-cd1", string.Empty);
            title = title.Replace("-cd2", string.Empty);
            title = title.Replace(" CD1", string.Empty);
            title = title.Replace(" CD2", string.Empty);
            title = title.Replace(" cd1", string.Empty);
            title = title.Replace(" cd2", string.Empty);
            title = title.Replace(".CD1", string.Empty);
            title = title.Replace(".CD2", string.Empty);
            title = title.Replace(".cd1", string.Empty);
            title = title.Replace(".cd2", string.Empty);
            return title;
        }

        public void ProcessDeleteNfoFiles(string currentPathRoot, DeleteNfoParams deleteNfoParams)
        {
            string[] directories = null;
            if (this.Type != KodiNfoElementType.FileSystem)
            {
                directories = this.LoadDirectories(currentPathRoot);
                App.CountFoundDirectories += directories.Length;
            }
            else
            {
                directories = new string[] { this.DirectoryName };
                App.CountFoundDirectories += directories.Length;
            }

            foreach (var folder in directories)
            {
                this.SetInformation(Path.GetFileNameWithoutExtension(folder));
                string newPath = Path.Combine(currentPathRoot, folder);
                if (this.Item != null)
                {
                    this.Item.ProcessDeleteNfoFiles(newPath, deleteNfoParams);
                }
                else
                {
                    this.ProcessDeleteFolder(newPath, deleteNfoParams);
                }
            }
        }

        internal void ProcessDeleteFolder(string currentPathRoot, DeleteNfoParams deleteNfoParams)
        {
            string[] files = this.LoadFiles(currentPathRoot);
            App.CountFoundFiles += files.Length;
            foreach (var file in files)
            {
                this.ProcessDeleteFile(Path.Combine(currentPathRoot, file), deleteNfoParams);
            }

            string[] directories = this.LoadDirectories(currentPathRoot);
            App.CountFoundDirectories += directories.Length;
            foreach (var folder in directories)
            {
                this.ProcessDeleteFolder(Path.Combine(currentPathRoot, folder), deleteNfoParams);
            }
        }

        private void ProcessDeleteFile(string pathAndFile, DeleteNfoParams deleteNfoParams)
        {
            try
            {
                XDocument xd = XDocument.Load(pathAndFile);

                if (KodiNfoXml.CheckForMissingInformation(xd, deleteNfoParams))
                {
                    File.Delete(pathAndFile);
                    Log.WriteInformation(string.Format("DELETION of NFO file {0} SUCCEEDED", pathAndFile));
                    App.CountNfoFilesDeleted++;
                }
            }
            catch (Exception x)
            {
                Log.WriteError(string.Format("DELETION of NFO file {0} FAILED", pathAndFile));
                Log.WriteException(x);
                App.CountNfoFilesFailedDeleted++;
            }
        }
    }
}
