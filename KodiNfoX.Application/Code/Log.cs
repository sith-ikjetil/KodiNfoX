using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KodiNfoX.Application.Code
{
    public static class Log
    {
        public static MainWindow MainWindow { get; set; }

        public static void WriteLine(string line,Brush brush) 
        {
            if (Log.MainWindow != null && line != null && brush != null)
            {
                Log.MainWindow.AppendTextToLog(line,brush);
            }
        }

        public static void WriteInformation(string information)
        {
            if (Log.MainWindow != null && information != null)
            {
                Log.MainWindow.AppendTextToLog(string.Format("(i): {0}\r\n", information),Brushes.Green);
            }
        }

        public static void WriteWarning(string warning)
        {
            if (Log.MainWindow != null && warning != null)
            {
                Log.MainWindow.AppendTextToLog(string.Format("(w): {0}\r\n", warning),Brushes.Blue);
            }
        }

        public static void WriteError(string error)
        {
            if (Log.MainWindow != null && error != null)
            {
                Log.MainWindow.AppendTextToLog(string.Format("(e): {0}\r\n", error),Brushes.Red);
            }
        }

        public static void WriteException(Exception x)
        {
            if (x != null)
            {
                Log.WriteError(Util.RenderException(x));
            }
        }

        public static void Clear()
        {
            if (Log.MainWindow != null)
            {
                Log.MainWindow.ClearTextToLog();
            }
        }
    }
}
