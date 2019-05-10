using Microsoft.Win32;
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
        int[] allFontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
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
            cbFontSizes.SelectedItem = 12;


            // Set up font sizes box
            cbFontSizes.ItemsSource = new List<int>(allFontSizes);


            // Load title and content
            dpMainPanel.DataContext = currentNote;
            contentLastCaretPosition = rtbContent.CaretPosition;
            tbTitle.SelectAll();
            FocusManager.SetFocusedElement(this, tbTitle);


            // Load notebooks
            List<Notebook> notebooksList = new List<Notebook>();
            cbNotebooks.ItemsSource = notebooksList;
            notebooksList.Add(new Notebook() { Name = "(no notebook)", Id = 0 }); // Add a blank dummy notebook to indicate user selected no notebook
            foreach (Notebook nb in Globals.LoginUser.Notebooks)
                notebooksList.Add(nb);

            if (currentNote.Notebook == null)
                cbNotebooks.SelectedIndex = 0;
            else
                cbNotebooks.SelectedItem = notebooksList.Where(note => note.Id == currentNote.Notebook.Id).FirstOrDefault();
        }

        private void SaveNote()
        {
            Notebook selNotebook = cbNotebooks.SelectedItem as Notebook;

            if (tbTitle.Text != "")
            currentNote.Title = tbTitle.Text;
                currentNote.Content = rtbContent.Text.ToString();
            if ((cbNotebooks.SelectedItem as Notebook).Id != 0)
                currentNote.Notebook = Globals.Ctx.Notebooks.Where(nb => nb.Id == selNotebook.Id).FirstOrDefault();

            if (currentNote.Id < 1) //== null)
                Globals.Ctx.Notes.Add(currentNote);
            else
            {
                Note updateNote = Globals.Ctx.Notes.Where(note => note.Id == currentNote.Id).ToList()[0];
                updateNote.Title = currentNote.Title;
                updateNote.Content = currentNote.Content;
                updateNote.LastUpdatedDate = DateTime.Now;

                if ((cbNotebooks.SelectedItem as Notebook).Id == 0)
                    updateNote.Notebook = Globals.Ctx.Notebooks.Where(nb => nb.Id == selNotebook.Id).FirstOrDefault();
                else
                    updateNote.Notebook = selNotebook;
            }

            Globals.Ctx.SaveChanges();

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

        private void FontFamilySize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string newFontName;
            double newFontSize;

            try
            {
                if (sender.Equals(cbFonts))
                {
                    newFontName = e.AddedItems[0].ToString();
                    if (!double.TryParse(cbFontSizes.Text, out newFontSize))
                        newFontSize = 12; // If error parsing new font size, revert to default
                }
                else
                {
                    newFontName = cbFonts.Text;
                    if (!double.TryParse(e.AddedItems[0].ToString(), out newFontSize))
                        newFontSize = 12; // If error parsing new font size, revert to default
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                // If no event args AddedItems passed, set defaults
                newFontName = SystemFonts.MessageFontFamily.Source.ToString();
                newFontSize = 12;
            }

            // See if we can select a single font name/size from selection (if error, it signals there are multiple different ones in selection
            bool multi = false;
            string selectedFontNameCheck;
            string selectedFontSizeCheck;
            try
            {
                selectedFontNameCheck = (rtbContent.Selection.GetPropertyValue(FontFamilyProperty) as FontFamily).Source;
                selectedFontSizeCheck = rtbContent.Selection.GetPropertyValue(FontSizeProperty).ToString();
            }
            catch (NullReferenceException ex)
            {
                multi = true;
            }

            if (multi ||
                newFontName != (rtbContent.Selection.GetPropertyValue(FontFamilyProperty) as FontFamily).Source || 
                newFontSize.ToString() != rtbContent.Selection.GetPropertyValue(FontSizeProperty).ToString())
            {
                FontFamily newFF = new FontFamily(newFontName);

                if (rtbContent.Selection.IsEmpty)
                {
                    if (rtbContent.Selection.Start.Paragraph == null)
                    {
                        Paragraph p = new Paragraph
                        {
                            FontFamily = newFF,
                            FontSize = newFontSize
                        };
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
                                FontFamily = newFF,
                                FontSize = newFontSize
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
                    selectedRange.ApplyPropertyValue(TextElement.FontSizeProperty, newFontSize);
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
                    if (double.TryParse(rtbContent.Selection.GetPropertyValue(FontSizeProperty).ToString(), out double fontSizeDouble))
                        cbFontSizes.SelectedItem = allFontSizes.OrderBy(x => Math.Abs((long)x - fontSizeDouble)).First(); // Output if font size of run matches one of the predefined font sizes
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

        private void MenuItemImport_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Warning: This will erase all currently existing contents of the note. Would you still like to proceed?", "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "JTNote Documents (*.jtn)|*.jtn|Text Documents (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All Supported Formats|*.jtn;*.txt;*.rtf";
                d.FilterIndex = 4;
                d.Title = "JTNote - Import Document";
                d.ShowDialog();

                if (d.FileName != "")
                {
                    try
                    {                        
                        rtbContent.Text = FileFunctions.GetContentOfFile(d.FileName);
                    }
                    catch (Exception ex)
                    {
                        //FIXME: Catch specific exception
                    }
                }
            }
        }

        private void BtnShare_Click(object sender, RoutedEventArgs e)
        {
            SharedNoteDialog sharedNoteDlg = new SharedNoteDialog(this, currentNote);
            
            if (sharedNoteDlg.ShowDialog() == true)
            {

            }
        }
    }
}
