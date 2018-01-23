using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KodiNfoX.Application.Code
{
    internal static class Util
    {
        public static string GetApplicationVersion() 
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static string RenderException(Exception x)
        {
            StringBuilder info = new StringBuilder();
            return RenderExceptionHelper(x, info);
        }

        private static string RenderExceptionHelper(Exception x, StringBuilder info)
        {
            info.AppendLine("## Exception Information ##\r\n");
            info.AppendLine(string.Format("Timestamp:\r\n{0:yyyy-MM-dd HH:mm}\r\n", DateTime.Now));
            info.AppendLine(string.Format("Type:\r\n{0}\r\n", x.GetType().FullName));
            info.AppendLine(string.Format("Target Site:\r\n{0}\r\n", x.TargetSite));
            info.AppendLine(string.Format("Message:\r\n{0}\r\n", x.Message));
            info.AppendLine(string.Format("Source:\r\n{0}\r\n", x.Source));
            info.AppendLine(string.Format("Stack Trace:\r\n{0}", x.StackTrace));

            if (x.Data != null && x.Data.Count > 0)
            {
                info.AppendLine("Data:\r\n");
                foreach (object key in x.Data.Keys)
                {
                    if (key != null)
                    {
                        object o = x.Data[key];
                        if (o != null)
                        {
                            info.AppendLine(key.ToString() + ": " + o.ToString());
                        }
                    }
                }
            }

            if (x.InnerException != null)
            {
                info.AppendLine("");
                return RenderExceptionHelper(x.InnerException, info);
            }
            return info.ToString();
        }

        public static double ToDouble(string number)
        {
            number = number.Replace(".", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            number = number.Replace(",", System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            return double.Parse(number);
        }

        public static string ToString(double number)
        {
            return number.ToString().Replace(",", ".");
        }

    }
}
