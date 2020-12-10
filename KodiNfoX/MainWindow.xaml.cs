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
using KodiNfoX.Code;
namespace KodiNfoX
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
            Log.MainWindow = this;
		}

		#region Private Methods
        /// <summary>
        /// Select root path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void ButtonSelectRootFolder_Click(object sender, RoutedEventArgs e)
		{
            string startupPath = App.Current.StartupUri.AbsolutePath;
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Open Root Folder under which all movie files are loacated.";
                dialog.ShowNewFolderButton = false;
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.TextBoxRootFolder.Text = dialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Create Nfo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void ButtonCreateNfo_Click(object sender, RoutedEventArgs e)
		{
            Log.Clear();
            Log.WriteInformation("Action: Create NFO Files");

            WriteParametersToLog();

            string rpath = this.TextBoxRootFolder.Text;
            if (string.IsNullOrEmpty(rpath) || string.IsNullOrWhiteSpace(rpath))
            {
                Log.WriteError("Invalid root path");
                return;
            }

            if (string.IsNullOrEmpty(this.ComboBoxFilter.Text) || string.IsNullOrWhiteSpace(this.ComboBoxFilter.Text))
            {
                Log.WriteError("Invalid filter");
                return;
            }

            if (!this.ComboBoxFilter.Text.EndsWith(".[[ext]]"))
            {
                Log.WriteError("Invalid filter. No .[[ext]] present");
                return;
            }

            if (string.IsNullOrEmpty(this.TextBoxExtensions.Text) || string.IsNullOrWhiteSpace(this.TextBoxExtensions.Text))
            {
                Log.WriteError("Invalid extensions.");
                return;
            }


            App.ReplaceExisting = (this.CheckBoxReplaceExisting.IsChecked.HasValue) ? this.CheckBoxReplaceExisting.IsChecked.Value : false;
            App.Username = this.TextBoxUserName.Text;
            App.Password = this.TextBoxPassword.Password;
            App.FileFilter = this.ComboBoxFilter.Text;
            App.Extensions = this.TextBoxExtensions.Text.Split(new char[] { ';', '|', ',' });
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

            Task.Run(() => { this.ExecuteCreateNfoFiles(rpath); });
        }

        /// <summary>
        /// Creates Nfo Task.
        /// </summary>
        /// <param name="rpath"></param>
        private void ExecuteCreateNfoFiles(string rpath)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.ShowBusyIndicator("Creating NFO Files...");
                });
                
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
            finally
			{
                this.Dispatcher.Invoke(() =>
                {
                    this.HideBusyIndicator();
                });
            }
        }

        /// <summary>
        /// Delete Nfo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDeleteNfo_Click(object sender, RoutedEventArgs e)
		{
            try
            {
                this.Opacity = 0.3d;

                var dw = new DeleteNfoWindow();
                dw.Owner = this;
                bool? bResult = dw.ShowDialog();
                if (bResult.HasValue && bResult.Value == true)
                {
                    Log.Clear();
                    Log.WriteInformation("Action: Delete NFO Files");

                    WriteParametersToLog();

                    string rpath = this.TextBoxRootFolder.Text;
                    Log.WriteInformation("Root Path: " + rpath);
                    if (string.IsNullOrEmpty(rpath) || string.IsNullOrWhiteSpace(rpath))
                    {
                        Log.WriteError("Invalid root path");
                        return;
                    }

                    this.ClearStats();

                    App.Username = this.TextBoxUserName.Text;
                    App.Password = this.TextBoxPassword.Password;
                    App.Extensions = new string[] { "nfo" };

                    var dp = dw.GetDeleteParams();

                    Task.Run(() => { this.ExecuteDeleteNfoFiles(rpath, dp); });
                }
            }
            finally
			{
                this.Opacity = 1.0d;
			}
        }

        /// <summary>
        /// Deletes NFO files implementation Task method.
        /// </summary>
        /// <param name="rpath"></param>
        /// <param name="deleteNfoParams"></param>
        private void ExecuteDeleteNfoFiles(string rpath, DeleteNfoParams deleteNfoParams)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                   this.ShowBusyIndicator("Deleting NFO Files...");
                });

                KodiNfoRootTree krt = new KodiNfoRootTree(rpath);
                krt.DeleteNfoFiles(deleteNfoParams);

                Log.WriteInformation(string.Format("# Directories Found: {0}", App.CountFoundDirectories.ToString()));
                Log.WriteInformation(string.Format("# Files Found: {0}", App.CountFoundFiles.ToString()));
                //Log.WriteInformation("# Nfo Files Written: " + App.CountNfoFilesWritten.ToString());
                //Log.WriteInformation("# Nfo Files Failed: " + App.CountNfoFilesFailedWritten.ToString());
                //Log.WriteInformation("# Tmdb Hits: " + App.CountTmdbHits.ToString());
                //Log.WriteInformation("# Tmdb Miss: " + App.CountTmdbMiss.ToString());
                //Log.WriteInformation("# Too Many Tmdb Hits: " + App.CountTooManyTmdbHits.ToString());
                Log.WriteInformation(string.Format("# Nfo Files Deleted: {0}", App.CountNfoFilesDeleted.ToString()));
                Log.WriteInformation(string.Format("# Nfo Files Failed to Delete: {0}", App.CountNfoFilesFailedDeleted.ToString()));

                Log.WriteInformation("Operation completed");
            }
            catch (Exception x)
            {
                Log.WriteException(x);
            }
            finally
			{
                this.Dispatcher.Invoke(() =>
                {
                    this.HideBusyIndicator();
                });
            }
        }

        /// <summary>
        /// Clears statistics.
        /// </summary>
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

        /// <summary>
        /// Appends text to status log.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="brush"></param>
        private void AppendText(string text, Brush brush)
        {
            TextRange range = new TextRange(this.RichTextBoxStatus.Document.ContentEnd, this.RichTextBoxStatus.Document.ContentEnd);
            range.Text = text;
            range = new TextRange(range.Start, this.RichTextBoxStatus.Document.ContentEnd);
            range.ApplyPropertyValue(RichTextBox.ForegroundProperty, brush);

            this.RichTextBoxStatus.ScrollToEnd();
        }

        /// <summary>
        /// Clears status log.
        /// </summary>
        private void ClearText()
        {
            TextRange range = new TextRange(this.RichTextBoxStatus.Document.ContentStart, this.RichTextBoxStatus.Document.ContentEnd);
            range.Text = string.Empty;

            this.RichTextBoxStatus.ScrollToEnd();
        }

        /// <summary>
        /// Hides the busy indicator.
        /// </summary>
        private void HideBusyIndicator()
        {
            this.LabelBusyIndicator.Content = string.Empty;
            this.BusyIndicator.Visibility = Visibility.Collapsed;
            this.BusyIndicator.IsEnabled = false;
        }

        /// <summary>
        /// Shows the busy indicator.
        /// </summary>
        /// <param name="msg"></param>
        private void ShowBusyIndicator(string msg)
        {
            this.LabelBusyIndicator.Content = msg;
            this.BusyIndicator.Visibility = Visibility.Visible;
            this.BusyIndicator.IsEnabled = true;
        }

        /// <summary>
        /// Writes parameters to log.
        /// </summary>
        private void WriteParametersToLog()
        {
            Log.WriteInformation(string.Format("Root Folder: {0}", this.TextBoxRootFolder.Text));
            Log.WriteInformation(string.Format("Filter: {0}", this.ComboBoxFilter.Text));
            Log.WriteInformation(string.Format("UserName: {0}", this.TextBoxUserName.Text));
            Log.WriteInformation(string.Format("Extensions: {0}", this.TextBoxExtensions.Text));
            Log.WriteInformation(string.Format("ReplaceExisting: {0}", (this.CheckBoxReplaceExisting.IsChecked.HasValue) ? this.CheckBoxReplaceExisting.IsChecked.Value.ToString() : false.ToString()));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemStatusSelectAll_Click(object sender, RoutedEventArgs e)
        {
            this.ContextMenuRichTextBoxStatus.IsOpen = false;

            this.RichTextBoxStatus.SelectAll();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Appends message to log.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="brush"></param>
        public void AppendTextToLog(string text, Brush brush)
        {
            this.Dispatcher.Invoke(() =>
            {
               this.AppendText(text, brush);
            });
        }

        /// <summary>
        /// Clears log.
        /// </summary>
        public void ClearTextToLog()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.ClearText();
            });
        }
        #endregion
 
    }// class
}// namespace
