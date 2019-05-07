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
        Note currentNote = new Note(0, Globals.LoginUser.Id, "Untitled Note", "", null, false, DateTime.Now);
        string initialTitle;
        string initialContent;
        bool forceClose = false; // To force the window to close without prompt when clicking checkmark button
        TextPointer contentLastCaretPosition;

        MainWindow mainWindow;
        public NoteEdit(MainWindow parent, Note inputNote = null)
        {
            InitializeComponent();
            Owner = parent;
            mainWindow = parent;

            if (inputNote != null)
                currentNote = inputNote;


            // Set initial title and content, to check for changes
            initialTitle = currentNote.Title;
            initialContent = currentNote.Content;


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
            dpMainPanel.DataContext = currentNote;
            contentLastCaretPosition = rtbContent.CaretPosition;
            tbTitle.SelectAll();
            FocusManager.SetFocusedElement(this, tbTitle);
        }

        private void SaveNote()
        {
            if (tbTitle.Text != "")
                currentNote.Title = tbTitle.Text;
            currentNote.Content = rtbContent.Text.ToString();

            using (var ctx = new JTNoteContext())
            {
                if (currentNote.Id < 1) //== null)
                    ctx.Notes.Add(currentNote);
                else
                {
                    Note updateNote = ctx.Notes.Where(note => note.Id == currentNote.Id).ToList()[0];
                    updateNote.Title = currentNote.Title;
                    updateNote.Content = currentNote.Content;
                    updateNote.LastUpdatedDate = DateTime.Now;
                }

                ctx.SaveChanges();
            }

            mainWindow.LoadAllNotes();
        }

        private void BtnSaveNote_Click(object sender, RoutedEventArgs e)
        {
            SaveNote();
            forceClose = true;
            Close();
            mainWindow.Activate();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!forceClose && (tbTitle.Text != initialTitle || rtbContent.Text.ToString() != initialContent))
            {
                MessageBoxResult saveResponse = MessageBox.Show(string.Format("Would you like to save the changes to {0}?", tbTitle.Text), "JTNote", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (saveResponse)
                {
                    case MessageBoxResult.Yes:
                        SaveNote();
                        mainWindow.Activate();
                        break;
                    case MessageBoxResult.No:
                        tbTitle.Text = initialTitle;
                        rtbContent.Text = initialContent;
                        mainWindow.Activate();
                        break; // Let window close without saving changes
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
            //rtbContent.FontFamily = new FontFamily(cbFonts.Text);
            //rtbContent.Focus();
            string newFontName = e.AddedItems[0].ToString();

            if (newFontName != (rtbContent.Selection.GetPropertyValue(FontFamilyProperty) as FontFamily).Source)
            {
                FontFamily newFF = new FontFamily(newFontName);

                if (rtbContent.Selection.IsEmpty)
                {
                    if (rtbContent.Selection.Start.Paragraph == null)
                    {
                        Paragraph p = new Paragraph();
                        p.FontFamily = newFF;
                        rtbContent.Document.Blocks.Add(p);
                    }
                    else
                    {
                        TextPointer currentCaret = contentLastCaretPosition;
                        Block currentBlock = rtbContent.Document.Blocks.Where(x => x.ContentStart.CompareTo(currentCaret) == -1 && x.ContentEnd.CompareTo(currentCaret) == 1).FirstOrDefault();

                        if (currentBlock != null)
                        {
                            Paragraph newParagraph = currentBlock as Paragraph;
                            Run newRun = new Run
                            {
                                FontFamily = newFF
                            };
                            newParagraph.Inlines.Add(newRun);
                            rtbContent.CaretPosition = newRun.ElementStart;
                        }
                    }
                }
                else
                {
                    TextRange selectedRange = new TextRange(rtbContent.Selection.Start, rtbContent.Selection.End);
                    selectedRange.ApplyPropertyValue(TextElement.FontFamilyProperty, newFF);
                }

                rtbContent.Focus();
            }
        }

        private void CbFontSizes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (int.TryParse(cbFontSizes.Text, out int fontSize))
                rtbContent.FontSize = fontSize;
            else
                MessageBox.Show("Error: Invalid font size.", "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);

            rtbContent.Focus();
        }

        private void RtbContent_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextPointer currentCaret = rtbContent.CaretPosition;
            Block currentBlock = rtbContent.Document.Blocks.Where(x => x.ContentStart.CompareTo(currentCaret) == -1 && x.ContentEnd.CompareTo(currentCaret) == 1).FirstOrDefault();
            if (currentBlock != null)
            {
                try
                {
                    cbFonts.SelectedItem = (rtbContent.Selection.GetPropertyValue(FontFamilyProperty) as FontFamily).Source;
                }
                catch (NullReferenceException ex)
                {
                    // Deliberately do nothing, if selection contains more than one font, do not change the selected font.
                }
            }
        }

        private void RtbContent_LostFocus(object sender, RoutedEventArgs e)
        {
            // Preserve carat position of content box so content from menus inserted correctly
            contentLastCaretPosition = rtbContent.CaretPosition;
        }
    }
}
