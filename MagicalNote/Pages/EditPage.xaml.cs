using MagicalNote.Dto;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MagicalNote.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPage : Page
    {
        private List<EditLineModel> _fonts = new List<EditLineModel>() { new EditLineModel(1) };
        public List<EditLineModel> Fonts
        {
            get { return _fonts; }
        }
        public EditPage()
        {
            this.InitializeComponent();
        }
        public static ObservableCollection<EditLineModel> GetContactsAsync()
        {
            ObservableCollection<EditLineModel> contacts = new ObservableCollection<EditLineModel>();

            for (int i = 0; i < 5; i++)
            {
                contacts.Add(new EditLineModel(i + 1));
            }

            return contacts;
        }

        private void maintext_TextChanged(object sender, RoutedEventArgs e)
        {
            string text;
            maintext.TextDocument.GetText(Microsoft.UI.Text.TextGetOptions.None, out text);
            int line = SubstringCount(text, "\r");
            if (line < 1)
            {
                return;
            }
            else
            {
                if (this.mainlist.Items.Count != line)
                {
                    this.mainlist.Items.Clear();
                    for (int i = 0; i < line; i++)
                    {
                        this.mainlist.Items.Add((i + 1).ToString());
                    }
                }
            }
        }

        public static Int32 GetLineCountFromString(string text)
        {
            Int32 lineCount = 0;
            try
            {
                var lines = Regex.Split(text.Trim(), "\r");
                lineCount = lines.Length;
            }
            catch (Exception ex) { }
            return lineCount;
        }
        static int SubstringCount(string str, string substring)
        {
            if (str.Contains(substring))
            {
                string strReplaced = str.Replace(substring, "");
                return (str.Length - strReplaced.Length) / substring.Length;
            }

            return 0;
        }
    }
}
