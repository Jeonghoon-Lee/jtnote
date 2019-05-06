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

namespace JTNote
{
    /// <summary>
    /// Interaction logic for NoteEdit.xaml
    /// </summary>
    public partial class NoteEdit : Window
    {
        Note currentNote = new Note(null, Globals.LoginUser.Id, "Untitled Note", "", null, false, DateTime.Today);
        public NoteEdit(MainWindow parent, Note inputNote = null)
        {
            InitializeComponent();
            Owner = parent;

            if (inputNote != null)
                currentNote = inputNote;

            // Load title and content
            tbTitle.Text = currentNote.Title;
            rtbContent.Text = currentNote.Content; // FIXME: RTB does not load plain text data properly!!!!
        }

        private void BtnSaveNote_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(rtbContent.Text.ToString());
        }
    }
}
