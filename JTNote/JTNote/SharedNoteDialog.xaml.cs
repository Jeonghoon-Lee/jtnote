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
    /// Interaction logic for SharedNoteDialog.xaml
    /// </summary>
    public partial class SharedNoteDialog : Window
    {
        public SharedNoteDialog(Window owner, Note note)
        {
            InitializeComponent();

            Owner = owner;
            tbTitle.Text = note.Title;

            foreach (User user in Globals.Ctx.Users) {
                cmbUserList.Items.Add(user);
            }

            // initialize
            foreach (SharedNote shNote in note.SharedNotes)
            {
                lvSharedUsers.Items.Add(shNote.User);
            }
            lvSharedUsers.Items.Refresh();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            User selectedUser = (User)cmbUserList.SelectedValue;

            lvSharedUsers.Items.Add(selectedUser);
            lvSharedUsers.Items.Refresh();
        }
    }
}
