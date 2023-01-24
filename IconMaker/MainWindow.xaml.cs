using System.Windows.Input;
using Microsoft.Win32;

namespace IconMaker
{
    /// <summary>
    /// Main application window for IconMaker.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private OpenFileDialog? importDialog;
        private SaveFileDialog? saveDialog;
        private IconFile currentIcon = new();
        private bool modified;

        public MainWindow() => this.InitializeComponent();

        protected override void OnInitialized(EventArgs e)
        {
            this.importDialog = (OpenFileDialog)this.Resources["importDialog"];
            this.saveDialog = (SaveFileDialog)this.Resources["saveDialog"];
            this.imageList.ItemsSource = this.currentIcon.Images;
            base.OnInitialized(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.Data.GetData("FileDrop") is string[] fileNames && fileNames.Length > 0)
            {
                foreach (var fileName in fileNames)
                    this.AddImageFile(fileName);
            }

            base.OnDrop(e);
        }

        private void AddImageFile(string fileName)
        {
            try
            {
                var decoder = BitmapDecoder.Create(new Uri(fileName), BitmapCreateOptions.None, BitmapCacheOption.None);
                if (decoder.Frames.Count > 0)
                {
                    var image = decoder.Frames[0];
                    this.currentIcon.Images.Set(image);
                    this.modified = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CommandAlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
        private void CanExecuteIfSelected(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = this.imageList.SelectedItem != null;
        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = this.currentIcon != null && this.currentIcon.Images.Count > 0;
        private void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                var image = Clipboard.GetImage();
                e.CanExecute = image != null && image.PixelWidth == image.PixelHeight && image.PixelWidth <= 256;
            }
            catch
            {
                // I don't like this, but Clipboard.GetImage can throw random exceptions if the clipboard data is invalid.
                e.CanExecute = false;
            }
        }
        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.currentIcon != null && this.currentIcon.Images.Count > 0 && this.modified)
            {
                MessageBoxResult result;
                if ((result = MessageBox.Show(this, "Icon has not been saved. Create a new icon?", "Icon Maker", MessageBoxButton.YesNo, MessageBoxImage.Question)) == MessageBoxResult.Yes)
                {
                    this.modified = false;
                }
                else if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            this.currentIcon = new IconFile();
            this.imageList.ItemsSource = this.currentIcon.Images;
        }
        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.importDialog?.ShowDialog(this) == true)
            {
                foreach (var fileName in this.importDialog.FileNames)
                    this.AddImageFile(fileName);
            }
        }
        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.currentIcon != null && this.currentIcon.Images.Count > 0 && this.saveDialog?.ShowDialog(this) == true)
            {
                this.currentIcon.Save(this.saveDialog.FileName);
                this.modified = false;
            }
        }
        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e) => this.Close();
        private void DeleteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.imageList.SelectedItem is BitmapSource image)
            {
                if (this.currentIcon == null)
                {
                    this.currentIcon = new IconFile();
                    this.imageList.ItemsSource = this.currentIcon.Images;
                }

                this.currentIcon.Images.Remove(image);
                this.modified = true;
            }
        }
        private void CutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.imageList.SelectedItem is BitmapSource image)
            {
                this.currentIcon.Images.Remove(image);
                Clipboard.SetImage(image);
                this.modified = true;
            }
        }
        private void CopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.imageList.SelectedItem is BitmapSource image)
                Clipboard.SetImage(image);
        }
        private void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var image = Clipboard.GetImage();
            if (image != null && image.PixelWidth == image.PixelHeight && image.PixelWidth <= 256)
            {
                this.currentIcon.Images.Set(image);
                this.modified = true;
            }
        }
    }
}
