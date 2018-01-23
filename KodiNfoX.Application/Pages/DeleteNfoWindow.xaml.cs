using KodiNfoX.Application.Code;
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
using System.Windows.Shapes;

namespace KodiNfoX.Application.Pages
{
    /// <summary>
    /// Interaction logic for DeleteNfoWindow.xaml
    /// </summary>
    public partial class DeleteNfoWindow : Window
    {
        public DeleteNfoWindow()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {            
            this.DialogResult = true;
            this.Close();
        }

        public DeleteNfoParams GetDeleteParams()
        {
            return new DeleteNfoParams()
            {
                Actor = (this.checkBoxActor.IsChecked.HasValue) ? this.checkBoxActor.IsChecked.Value : false,
                Director = (this.checkBoxDirector.IsChecked.HasValue) ? this.checkBoxDirector.IsChecked.Value : false,
                Genre = (this.checkBoxGenre.IsChecked.HasValue) ? this.checkBoxGenre.IsChecked.Value : false,
                PlotOutline = (this.checkBoxPlotOutline.IsChecked.HasValue) ? this.checkBoxPlotOutline.IsChecked.Value : false,
                Producer = (this.checkBoxProducer.IsChecked.HasValue) ? this.checkBoxProducer.IsChecked.Value : false,
                Rating = (this.checkBoxRating.IsChecked.HasValue) ? this.checkBoxRating.IsChecked.Value : false,
                ThumbPoster = (this.checkBoxThumbPoster.IsChecked.HasValue) ? this.checkBoxThumbPoster.IsChecked.Value : false,
                TitleSortTitle = (this.checkBoxTitleSortTitle.IsChecked.HasValue) ? this.checkBoxTitleSortTitle.IsChecked.Value : false,
                Writer = (this.checkBoxWriter.IsChecked.HasValue) ? this.checkBoxWriter.IsChecked.Value : false,
            };
        }
    }
}
