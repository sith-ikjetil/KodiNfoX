using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KodiNfoX
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string FileFilter { get; set; }
        public static string[] Extensions { get; set; }
        public static bool ReplaceExisting { get; set; }
        public static long CountFoundDirectories = 0;
        public static long CountFoundFiles = 0;
        public static long CountNfoFilesWritten = 0;
        public static long CountTmdbHits = 0;
        public static long CountTmdbMiss = 0;
        public static long CountNfoFilesFailedWritten = 0;
        public static long CountTooManyTmdbHits = 0;
        public static long CountNfoFilesFailedDeleted = 0;
        public static long CountNfoFilesDeleted = 0;
    }
}
