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
        List<User> userList;
        List<SharedNote> sharedList;

        public SharedNoteDialog(Window owner, Note note)
        {
            InitializeComponent();

            // FIXME: check globals
            Globals.CurrentNote = note;

            Owner = owner;
            tbTitle.Text = note.Title;

            // Remove yourself from user list
            userList = Globals.Ctx.Users.Where(user => user.Id != Globals.LoginUser.Id).ToList();

            sharedList = note.SharedNotes?.ToList();
            foreach (SharedNote item in sharedList)
            {
                if (userList.Contains(item.User))
                {
                    userList.Remove(item.User);
                }
            }

            cmbUserList.ItemsSource = userList;
            lvSharedUsers.ItemsSource = sharedList;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbUserList.SelectedIndex == -1) return;

            User selectedUser = (User)cmbUserList.SelectedValue;

            sharedList.Add(new SharedNote() { User = selectedUser, Note = Globals.CurrentNote });
            lvSharedUsers.Items.Refresh();

            // Remove selected user from user list
            userList.Remove(selectedUser);
            cmbUserList.Items.Refresh();
        }

        private void ButtonSharedNote_Click(object sender, RoutedEventArgs e)
        {
            Globals.CurrentNote.SharedNotes.Clear();
            foreach (SharedNote sharedNote in sharedList)
            {
                Globals.CurrentNote.SharedNotes.Add(sharedNote);
                Globals.Ctx.SaveChanges();
            }
            DialogResult = true;
        }

        private void ButtonRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            SharedNote selected = (SharedNote)(sender as Button).DataContext;

            sharedList.Remove(selected);
            lvSharedUsers.Items.Refresh();

            // recover user list
            userList.Add(selected.User);
            cmbUserList.Items.Refresh();
        }
    }
}
