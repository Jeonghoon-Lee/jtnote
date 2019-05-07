using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

namespace JTNote
{
    /// <summary>
    /// Interaction logic for NoteEdit.xaml
    /// </summary>
    public partial class NoteEdit : Window
    {
        Note currentNote = new Note(null, Globals.LoginUser.Id, "Untitled Note", "", null, false, DateTime.Now);
        MainWindow mainWindow;
        public NoteEdit(MainWindow parent, Note inputNote = null)
        {
            InitializeComponent();
            Owner = parent;
            mainWindow = parent;

            if (inputNote != null)
                currentNote = inputNote;


            // Set up font families box
            List<string> fontNames = new List<string>();
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                fontNames.Add(font.Source);
            }
            fontNames = fontNames.OrderBy(x => x).ToList();
            cbFonts.ItemsSource = fontNames;
            cbFonts.SelectedItem = rtbContent.FontFamily.Source;

            // Set up font sizes box
            cbFontSizes.ItemsSource = new List<int>(new int[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 });


            // Load title and content
            tbTitle.Text = currentNote.Title;
            if (currentNote.Content != "")
                rtbContent.Text = currentNote.Content;
        }

        private void SaveNote()
        {
            if (tbTitle.Text != "")
                currentNote.Title = tbTitle.Text;
            currentNote.Content = rtbContent.Text.ToString();

            if (currentNote.Id == null)
                Globals.Db.CreateNote(currentNote);
            else
                Globals.Db.UpdateNote(currentNote);

            mainWindow.LoadAllNotes();
        }

        private void BtnSaveNote_Click(object sender, RoutedEventArgs e)
        {
            SaveNote();
            Close();
            return;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (tbTitle.Text != currentNote.Title || rtbContent.Text.ToString() != currentNote.Content)
            {
                MessageBoxResult saveResponse = MessageBox.Show(string.Format("Would you like to save the changes to {0}?", tbTitle.Text), "JTNote", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (saveResponse)
                {
                    case MessageBoxResult.Yes:
                        SaveNote();
                        break;
                    case MessageBoxResult.No:
                        break; // Let window close without saving
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    default:
                        MessageBox.Show("Unable to save changes due to an unknown error.", "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        return;
                }
            }
        }

        private void CbFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rtbContent.FontFamily = new FontFamily(cbFonts.Text);
            rtbContent.Focus();
        }

        private void CbFontSizes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (int.TryParse(cbFontSizes.Text, out int fontSize))
                rtbContent.FontSize = fontSize;
            else
                MessageBox.Show("Error: Invalid font size.", "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);

            rtbContent.Focus();
        }
    }
}
