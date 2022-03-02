using MagicalNote.Pages;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MagicalNote
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly List<string> _fileTypeChoices = new List<string>() { ".rtf", ".txt", ".cs", ".js", ".css", ".html" };

        public MainPage()
        {
            this.InitializeComponent();
        }


        private void TabView_Loaded(object sender, RoutedEventArgs e)
        {
            this.maintab.TabItems.Add(CreateNewTab(0));
        }

        private void OnGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var gridView = (Grid)sender;
            maintab.Height = gridView.ActualHeight;
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            TabViewItem tab = this.maintab.SelectedItem as TabViewItem;
            var page = (tab.Content as Frame).Content as EditPage;
            page.maintext.Document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            TabViewItem tab = this.maintab.SelectedItem as TabViewItem;
            var page = (tab.Content as Frame).Content as EditPage;
            page.maintext.Document.Selection.CharacterFormat.Italic = FormatEffect.Toggle;
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            TabViewItem tab = this.maintab.SelectedItem as TabViewItem;
            var page = (tab.Content as Frame).Content as EditPage;
            page.maintext.Document.Selection.CharacterFormat.Underline = UnderlineType.Single;
        }

        private void FontDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            TabViewItem tab = this.maintab.SelectedItem as TabViewItem;
            var page = (tab.Content as Frame).Content as EditPage;
            page.maintext.Document.Selection.CharacterFormat.Size = page.maintext.Document.Selection.CharacterFormat.Size - 1;
        }

        private void FontIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            TabViewItem tab = this.maintab.SelectedItem as TabViewItem;
            var page = (tab.Content as Frame).Content as EditPage;
            page.maintext.Document.Selection.CharacterFormat.Size = page.maintext.Document.Selection.CharacterFormat.Size + 1;
        }


        private void TabView_AddButtonClick(TabView sender, object args)
        {
            sender.TabItems.Add(CreateNewTab(sender.TabItems.Count));
        }


        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
        }

        private TabViewItem CreateNewTab(int index)
        {
            TabViewItem newItem = new TabViewItem
            {
                Header = $"New File {index + 1}",
                IconSource = new SymbolIconSource() { Symbol = Symbol.Document }
            };

            // The content of the tab is often a frame that contains a page, though it could be any UIElement.
            Frame frame = new Frame();

            frame.Navigate(typeof(EditPage));

            newItem.Content = frame;

            return newItem;
        }

        private TabViewItem CreateNewTab(string name, Windows.Storage.Streams.IRandomAccessStream randAccStream)
        {
            TabViewItem newItem = new TabViewItem
            {
                Header = name,
                IconSource = new SymbolIconSource() { Symbol = Symbol.Document }
            };

            // The content of the tab is often a frame that contains a page, though it could be any UIElement.
            Frame frame = new Frame();

            frame.Navigate(typeof(EditPage));

            newItem.Content = frame;

            var page = frame.Content as EditPage;
            page.maintext.Document.LoadFromStream(Microsoft.UI.Text.TextSetOptions.None, randAccStream);
            return newItem;
        }

        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker open =
               new Windows.Storage.Pickers.FileOpenPicker();
            open.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".txt");

            Windows.Storage.StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    this.maintab.TabItems.Add(CreateNewTab(file.Name, randAccStream));
                    this.maintab.SelectedIndex = this.maintab.TabItems.Count - 1;
                }
            }
        }

        private void CreateFile(object sender, RoutedEventArgs e)
        {
            this.maintab.TabItems.Add(CreateNewTab(this.maintab.TabItems.Count));
            this.maintab.SelectedIndex = this.maintab.TabItems.Count - 1;
        }

        private async void Save(object sender, RoutedEventArgs e)
        {
            TabViewItem tab = this.maintab.SelectedItem as TabViewItem;
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Rich Text", _fileTypeChoices);

            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = tab.Header + ".txt";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                // write to file
                using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    var page = (tab.Content as Frame).Content as EditPage;
                    page.maintext.Document.SaveToStream(Microsoft.UI.Text.TextGetOptions.UseObjectText, randAccStream);
                }

                // Let Windows know that we're finished changing the file so the 
                // other app can update the remote version of the file.
                var status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status != FileUpdateStatus.Complete)
                {
                    Windows.UI.Popups.MessageDialog errorBox =
                        new Windows.UI.Popups.MessageDialog("File " + file.Name + " Not Save.");
                    await errorBox.ShowAsync();
                }
            }
        }

        /// <summary>
        /// 剪切
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarCut_Click(object sender, RoutedEventArgs e)
        {
            TabViewItem tab = this.maintab.SelectedItem as TabViewItem;
            var page = (tab.Content as Frame).Content as EditPage;
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Move;
            dataPackage.SetText(page.maintext.Document.Selection.Text);
            Clipboard.SetContent(dataPackage);
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarCopy_Click(object sender, RoutedEventArgs e)
        {
            TabViewItem tab = this.maintab.SelectedItem as TabViewItem;
            var page = (tab.Content as Frame).Content as EditPage;
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(page.maintext.Document.Selection.Text);
            Clipboard.SetContent(dataPackage);
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BarPast_Click(object sender, RoutedEventArgs e)
        {
            TabViewItem tab = this.maintab.SelectedItem as TabViewItem;
            var page = (tab.Content as Frame).Content as EditPage;
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                string oldtext;
                page.maintext.Document.GetText(TextGetOptions.None, out oldtext);
                page.maintext.Document.SetText(TextSetOptions.None, oldtext + text);
            }
        }
    }
}
