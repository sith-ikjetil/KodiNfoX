using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
//using System.Windows.Forms;
using System.IO;
using KodiNfoX.Application.Code;
using Telerik.Windows.Controls;
using KodiNfoX.Application.Pages;
using System.Net.Mail;
using System.Diagnostics;
using System.Web;
using System.Windows.Threading;

namespace KodiNfoX.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            this.ShowBusyIndicatorMethod = new Action<string>(this.ShowBusyIndicator);
            this.HideBusyIndicatorMethod = new NoArgumentDelegate(this.HideBusyIndicator);
            this.AppendTextMethod = new Action<string,Brush>(this.AppendText);
            this.ClearTextMethod = new NoArgumentDelegate(this.ClearText);

            Log.MainWindow = this;
            
            this.radRibbonViewMain.SetValue(StyleManager.ThemeProperty, GetRandomTheme());

            this.richTextBoxStatus.Background = Brushes.White;
            this.richTextBoxStatus.Document.PageWidth = System.Windows.SystemParameters.PrimaryScreenWidth * 2;

            this.richTextBoxStatus.FontFamily = new FontFamily("Lucida Console");
            this.richTextBoxStatus.Document.LineHeight = 2;

            this.radRibbonViewMain.ApplicationName = string.Format("by Kjetil Kristoffer Solberg (post@ikjetil.no) | version: {0}", Util.GetApplicationVersion());
        }

        private Theme GetRandomTheme()
        {
            Random rnd = new Random();
            int iTheme = rnd.Next(1,8);

            SetThemeControlColors(Brushes.Black);            

            Theme theme = new Office_BlueTheme();
            
            switch ( iTheme) {
                case 1:
                    theme = new Office_BlueTheme();
                    break;
                case 2:
                    theme = new Office_BlackTheme();
                    break;
                case 3:
                    theme = new Office_SilverTheme();
                    break;
                case 4:
                    theme = new VistaTheme();
                    break;
                case 5:
                    theme = new Windows7Theme();
                    break;
                case 6:
                    theme = new Windows8Theme();
                    break;
                case 7:
                    //SetThemeControlColors(Brushes.White);
                    //theme = new Expression_DarkTheme();
                    break;
                case 8:
                    theme  = new Windows8TouchTheme();
                    break;
                default:
                    break;
            }
            return theme;
        }

        private void SetThemeControlColors(Brush brush)
        {
            this.labelExtensions.Foreground = brush;
            this.labelFilenameFilter.Foreground = brush;
            this.labelPassword.Foreground = brush;
            this.labelRootFolder.Foreground = brush;
            this.labelUsername.Foreground = brush;
            this.checkBoxReplaceExisting.Foreground = brush;
        }

        private void radRibbonButtonCreateNfoFiles_Click(object sender, RoutedEventArgs e)
        {
            Log.Clear();
            Log.WriteInformation("Action: Create NFO Files");

            WriteParametersToLog();

            string rpath = this.textBoxRootFolder.Text;            
            if (string.IsNullOrEmpty(rpath) || string.IsNullOrWhiteSpace(rpath))
            {
                Log.WriteError("Invalid root path");                
                return;
            }

            if (string.IsNullOrEmpty(this.textBoxFilter.Text) || string.IsNullOrWhiteSpace(this.textBoxFilter.Text))
            {
                Log.WriteError("Invalid filter");
                return;
            }

            if (!this.textBoxFilter.Text.EndsWith(".[[ext]]"))
            {
                Log.WriteError("Invalid filter. No .[[ext]] present");
                return;
            }

            if (string.IsNullOrEmpty(this.textBoxExtensions.Text) || string.IsNullOrWhiteSpace(this.textBoxExtensions.Text))
            {
                Log.WriteError("Invalid extensions.");
                return;
            }


            App.ReplaceExisting = (this.checkBoxReplaceExisting.IsChecked.HasValue) ? this.checkBoxReplaceExisting.IsChecked.Value : false;
            App.Username = this.textBoxUserName.Text;
            App.Password = this.textBoxPassword.Password;
            App.FileFilter = this.textBoxFilter.Text;
            App.Extensions = this.textBoxExtensions.Text.Split(new char[] { ';', '|', ',' });            
            if (App.Extensions.Length == 0)
            {
                Log.WriteError("No extensions present.");
                return;
            }

            this.ClearStats();

            foreach (var ext in App.Extensions)
            {
                if (ext.Length == 0)
                {
                    Log.WriteError("(e) Extension with length 0 found. Invalid.");
                    return;
                }
                foreach (var c in ext)
                {
                    foreach (var c2 in System.IO.Path.GetInvalidPathChars())
                    {
                        if (c == c2)
                        {
                            Log.WriteError("Extension with invalid character '" + c.ToString() + "' found.");
                            return;
                        }
                    }
                    foreach (var c2 in System.IO.Path.GetInvalidFileNameChars())
                    {
                        if (c == c2)
                        {
                            Log.WriteError("Extension with invalid character '" + c.ToString() + "' found.");
                            return;
                        }
                    }
                }
            }

            Task.Run( () => { this.ExecuteCreateNfoFiles(rpath); });
        }

        private void ClearStats()
        {
            App.CountFoundDirectories = 0;
            App.CountFoundFiles = 0;
            App.CountNfoFilesWritten = 0;
            App.CountTmdbHits = 0;
            App.CountTmdbMiss = 0;
            App.CountNfoFilesFailedWritten = 0;
            App.CountTooManyTmdbHits = 0;
            App.CountNfoFilesFailedDeleted = 0;
            App.CountNfoFilesDeleted = 0;
        }

        #region Delegate Implementation Methods
        private void AppendText(string text, Brush brush)
        {           
            TextRange range = new TextRange(this.richTextBoxStatus.Document.ContentEnd, this.richTextBoxStatus.Document.ContentEnd);
            range.Text = text;
            range = new TextRange(range.Start, this.richTextBoxStatus.Document.ContentEnd);
            range.ApplyPropertyValue(RichTextBox.ForegroundProperty, brush);                        
            
            this.richTextBoxStatus.ScrollToEnd();
        }

        private void ClearText()
        {
            TextRange range = new TextRange(this.richTextBoxStatus.Document.ContentStart, this.richTextBoxStatus.Document.ContentEnd);
            range.Text = string.Empty;
            
            this.richTextBoxStatus.ScrollToEnd();
        }

        private void HideBusyIndicator()
        {
            this.radBusyIndicator.IsBusy = false;
            this.radBusyIndicator.IsEnabled = false;
            this.radBusyIndicator.Visibility = System.Windows.Visibility.Collapsed;
            this.radRibbonButtonCreateNfoFiles.IsEnabled = true;
            this.radRibbonButtonDeleteNfoFiles.IsEnabled = true;
            this.radRibbonButtonRandomView.IsEnabled = true;
        }

        private void ShowBusyIndicator(string msg)
        {
            this.radBusyIndicator.BusyContent = msg;
            this.radBusyIndicator.IsBusy = true;
            this.radBusyIndicator.IsEnabled = true;
            this.radBusyIndicator.Visibility = System.Windows.Visibility.Visible;
            this.radRibbonButtonCreateNfoFiles.IsEnabled = false;
            this.radRibbonButtonDeleteNfoFiles.IsEnabled = false;
            this.radRibbonButtonRandomView.IsEnabled = false;
        }
        #endregion

        #region Public Methods
        public void AppendTextToLog(string text,Brush brush)
        {
            this.Dispatcher.BeginInvoke(this.AppendTextMethod,text,brush);
        }

        public void ClearTextToLog()
        {            
            this.Dispatcher.BeginInvoke(this.ClearTextMethod);
        }
        #endregion

        #region Delegates
        private delegate void NoArgumentDelegate();                        
        private NoArgumentDelegate HideBusyIndicatorMethod;
        private Action<string> ShowBusyIndicatorMethod;
        private Action<string,Brush> AppendTextMethod;
        private NoArgumentDelegate ClearTextMethod;
        #endregion

        private void ExecuteCreateNfoFiles(string rpath)
        {            
            try
            {
                this.Dispatcher.BeginInvoke(this.ShowBusyIndicatorMethod,new object[] { "Creating NFO Files..." });
                KodiNfoRootTree krt = new KodiNfoRootTree(rpath);                
                krt.CreateNfoFiles();

                Log.WriteInformation(string.Format("# Directories Found: {0}", App.CountFoundDirectories.ToString()));
                Log.WriteInformation(string.Format("# Files Found: {0}", App.CountFoundFiles.ToString()));
                Log.WriteInformation(string.Format("# Nfo Files Written: {0}", App.CountNfoFilesWritten.ToString()));
                Log.WriteInformation(string.Format("# Nfo Files Failed Written: {0}", App.CountNfoFilesFailedWritten.ToString()));
                Log.WriteInformation(string.Format("# Tmdb Hits: {0}", App.CountTmdbHits.ToString()));
                Log.WriteInformation(string.Format("# Tmdb Miss: {0}", App.CountTmdbMiss.ToString()));
                Log.WriteInformation(string.Format("# Too Many Tmdb Hits: {0}", App.CountTooManyTmdbHits.ToString()));

                Log.WriteInformation("Operation completed");
            }
            catch (Exception x)
            {
                Log.WriteException(x);                
            }
            this.Dispatcher.BeginInvoke(this.HideBusyIndicatorMethod);
        }

        

        private void buttonSelectRootFolder_Click(object sender, RoutedEventArgs e)
        {
            string startupPath = App.Current.StartupUri.AbsolutePath;
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Open Root Folder under which all movie files are loacated.";
                dialog.ShowNewFolderButton = false;
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.textBoxRootFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void radRibbonButtonRandomView_Click(object sender, RoutedEventArgs e)
        {
            this.radRibbonViewMain.SetValue(StyleManager.ThemeProperty, GetRandomTheme());
        }

        private void WriteParametersToLog()
        {
            Log.WriteInformation(string.Format("Root Folder: {0}", this.textBoxRootFolder.Text));
            Log.WriteInformation(string.Format("Filter: {0}", this.textBoxFilter.Text));
            Log.WriteInformation(string.Format("UserName: {0}", this.textBoxUserName.Text));
            Log.WriteInformation(string.Format("Extensions: {0}", this.textBoxExtensions.Text));
            Log.WriteInformation(string.Format("ReplaceExisting: {0}",(this.checkBoxReplaceExisting.IsChecked.HasValue) ? this.checkBoxReplaceExisting.IsChecked.Value.ToString() : false.ToString()));
        }

        private void radRibbonButtonDeleteNfoFiles_Click(object sender, RoutedEventArgs e)
        {            
            DeleteNfoWindow dw = new DeleteNfoWindow();
            bool? bResult = dw.ShowDialog();
            if (bResult.HasValue && bResult.Value == true)
            {
                Log.Clear();
                Log.WriteInformation("Action: Delete NFO Files");
                
                WriteParametersToLog();

                string rpath = this.textBoxRootFolder.Text;
                Log.WriteInformation("Root Path: " + rpath);
                if (string.IsNullOrEmpty(rpath) || string.IsNullOrWhiteSpace(rpath))
                {
                    Log.WriteError("Invalid root path");
                    return;
                }

                this.ClearStats();
                
                App.Username = this.textBoxUserName.Text;
                App.Password = this.textBoxPassword.Password;
                App.Extensions = new string[] { "nfo" };
                
                var dp = dw.GetDeleteParams();

                Task.Run(() => { this.ExecuteDeleteNfoFiles(rpath, dp); });
            }
        }

        private void ExecuteDeleteNfoFiles(string rpath, DeleteNfoParams deleteNfoParams)
        {
            try
            {
                this.Dispatcher.BeginInvoke(this.ShowBusyIndicatorMethod, new object[] { "Deleting NFO Files..." });
                KodiNfoRootTree krt = new KodiNfoRootTree(rpath);
                krt.DeleteNfoFiles(deleteNfoParams);

                Log.WriteInformation(string.Format("# Directories Found: {0}", App.CountFoundDirectories.ToString()));
                Log.WriteInformation(string.Format("# Files Found: {0}", App.CountFoundFiles.ToString()));
                //Log.WriteInformation("# Nfo Files Written: " + App.CountNfoFilesWritten.ToString());
                //Log.WriteInformation("# Nfo Files Failed: " + App.CountNfoFilesFailedWritten.ToString());
                //Log.WriteInformation("# Tmdb Hits: " + App.CountTmdbHits.ToString());
                //Log.WriteInformation("# Tmdb Miss: " + App.CountTmdbMiss.ToString());
                //Log.WriteInformation("# Too Many Tmdb Hits: " + App.CountTooManyTmdbHits.ToString());
                Log.WriteInformation(string.Format("# Nfo Files Deleted: {0}",App.CountNfoFilesDeleted.ToString()));
                Log.WriteInformation(string.Format("# Nfo Files Failed to Delete: {0}",App.CountNfoFilesFailedDeleted.ToString()));

                Log.WriteInformation("Operation completed");
            }
            catch (Exception x)
            {
                Log.WriteException(x);
            }
            this.Dispatcher.BeginInvoke(this.HideBusyIndicatorMethod);
        }        

        private void menuItemStatusSelectAll_Click(object sender, RoutedEventArgs e)
        {
            this.contextMenuRichTextBoxStatus.IsOpen = false;

            this.richTextBoxStatus.SelectAll();
        }

        private void SendStatusLog()
        {
            try
            {
                string text = new TextRange(this.richTextBoxStatus.Document.ContentStart, this.richTextBoxStatus.Document.ContentEnd).Text;
                string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "KodiNfoXStatusLog.txt");
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(filename))
                {
                    sw.Write(text);
                }                

                MAPI mapi = new MAPI();
                mapi.AddAttachment(filename);
                mapi.AddRecipientTo("post@ikjetil.no");
                mapi.SendMailPopup("Kodi Nfo X - Status Log", "Add any comments here.");
            }
            catch (Exception)
            {

            }
        }

        private void SendStatusLogDispatcher()
        {
            System.Threading.Thread.Sleep(1000);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {                
                this.SendStatusLog();
            }));            
        }

        private void menuItemStatusSendAsMail_Click(object sender, RoutedEventArgs e)
        {            
            Task.Run(() => this.SendStatusLogDispatcher()); //this.SendStatusLog();            
        }        
    }
}
