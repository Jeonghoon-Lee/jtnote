using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JTNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Create lists of items for main window display
        List<Note> notesList = new List<Note>();
        List<Note> trashList = new List<Note>();

        List<TagsOnUser> tagListOnUser = new List<TagsOnUser>();

        public MainWindow()
        {
            LoginRegister loginDlg = new LoginRegister();
            if (loginDlg.ShowDialog() != true)
            {
                Close();    // set close the main window event
                return;
            }              

            InitializeComponent();

            // bind Tags Tree View
            trvTags.ItemsSource = tagListOnUser;

            // Set login user information onto title bar
            Title = string.Format("JTNote - {0}", Globals.LoginUser.Email);
            ReloadTagTreeView();

            // Set default bindings for window elements
            lvCentrePane.ItemsSource = notesList;

            // Load all notes for logged in user
            LoadAllNotes();

            spRightPane.DataContext = lvCentrePane.SelectedItem as Note;
        }

        private void ReloadTagTreeView()
        {
            TagsOnUser tagsOnUser = new TagsOnUser();

            foreach (Tag tag in Globals.Ctx.Tags.Where(item => item.UserId == Globals.LoginUser.Id).ToList())
            {
                tag.NumberOfNotes = tag.Notes.Count;
                tagsOnUser.TagList.Add(tag);
            }
            tagListOnUser.Clear();
            tagListOnUser.Add(tagsOnUser);
            trvTags.Items.Refresh();
        }

        public void LoadAllNotes()
        {
            notesList.Clear(); // Clear existing notes list to refresh
            trashList.Clear(); // Clear existing trash list to refresh

            try
            {
                // Populate notes lists from DB
                //                Globals.Db.GetAllNotesByUserId(Globals.LoginUser.Id)
                // Updated with EF
                Globals.Ctx.Notes.Where(note => note.UserId == Globals.LoginUser.Id).ToList()
                    .ForEach(note => {
                        // if (note.IsDeleted)
                        if (note.IsDeleted == 1)
                            trashList.Add(note); // Add notes flagged for deletion to trash
                        else
                            notesList.Add(note); // Add all other notes to main notes list
                        });
            }
            catch (Exception ex)
            {
                // Check for known fatal exceptions, notify user and terminate program if they occur
                ErrorNotifyDbConnection(ex);
            }

            // Sort notes list and trash list by last modified date
            notesList = notesList.OrderByDescending(item => item.LastUpdatedDate).ToList();
            trashList = trashList.OrderByDescending(item => item.LastUpdatedDate).ToList();

            // Set counts for sidebar items - TODO: Replace these with bindings?
//            tbSidebarNotesTitle.Text = string.Format("Notes ({0})", notesList.Count);
//            tbSidebarTrashTitle.Text = string.Format("Trash ({0})", trashList.Count);

            // May 7, 2019
            // modified by JH to display note, trash count on left tree view
            tblNumberOfNotes.Text = notesList.Count.ToString();
            tblNumberOfTrash.Text = trashList.Count.ToString();

            // Refresh centre pane
            lvCentrePane.Items.Refresh();

            if (lvCentrePane.Items.Count > 0)
                lvCentrePane.SelectedIndex = 0;
        }

        void ErrorNotifyDbConnection(Exception ex)
        {
            // Check for known database connection errors and warn user, re-throw exception if unknown
            if (ex is SqlException || ex is System.IO.IOException)
            {
                MessageBox.Show(string.Format("Fatal error: unable to connect to the database.{0}{1}", Environment.NewLine, ex.Message), "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            else if (ex is InvalidCastException || ex is ArgumentException)
            {
                MessageBox.Show(string.Format("Fatal error: unable to perform action in the database due to corrupt data.{0}{1}", Environment.NewLine, ex.Message), "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            else
                throw ex;
        }

        void ChangeSidebarSelection(List<Note> newList, string newHighlight)
        {
            // Change data source for centre pane, highlighting correct item in right pane
            lvCentrePane.ItemsSource = newList;
            lvCentrePane.Items.Refresh();

            if (newList.Count > 0)
                lvCentrePane.SelectedIndex = 0;


            // Highlight correct menu item
            foreach (MenuItem curItem in mnuSidebar.Items)
            {
                if (curItem.Name == newHighlight)
                    curItem.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                else
                    curItem.Background = null;
            }


            // Change tooltip text for delete button and switch between share and restore buttons in right pane, as appropriate
            if (newHighlight == "miSidebarTrashItem")
            {
                btnRightPaneDelete.ToolTip = "Permanently delete this note.";
                btnRightPaneShare.Visibility = Visibility.Hidden;
                btnRightPaneRestore.Visibility = Visibility.Visible;
            }
            else
            {
                btnRightPaneDelete.ToolTip = "Send this note to trash.";
                btnRightPaneRestore.Visibility = Visibility.Hidden;
                btnRightPaneShare.Visibility = Visibility.Visible;
            }
        }

        private void LvCentrePane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            spRightPane.DataContext = lvCentrePane.SelectedItem as Note;
        }


        // SIDEBAR CLICKS
        private void MiSidebarTrashItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeSidebarSelection(trashList, "miSidebarTrashItem");
        }

        private void MiSidebarNotesItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeSidebarSelection(notesList, "miSidebarNotesItem");
        }


        // OTHER BUTTON CLICKS
        private void BtnResync_Click(object sender, RoutedEventArgs e)
        {
            LoadAllNotes();
        }


        // RIGHT PANE BUTTON CLICKS
        private void BtnRightPaneDelete_Click(object sender, RoutedEventArgs e)
        {
            Note currentNote = lvCentrePane.SelectedItem as Note;
            // if (currentNote.IsDeleted == true)
            // updated with EF
            if (currentNote.IsDeleted == 1)
            {
                // Item is already in trash, permanently delete
                try
                {
                    if (MessageBox.Show(string.Format("Are you sure you want to permanently delete the note \"{0}\"? This is not reversible.", currentNote.Title), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        //    currentNote.DeleteSelfFromDb();
                        //    LoadAllNotes();

                        // updated with EF
                        // FIXME: Need to test
                        Globals.Ctx.Notes.Remove(currentNote);
                        Globals.Ctx.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    ErrorNotifyDbConnection(ex);
                }
            }
            else
            {
                // Item is not yet in trash, move it there                
                try
                {
                    // currentNote.IsDeleted = true;
                    // currentNote.UpdateSelfInDb();
                    // LoadAllNotes();

                    // updated with EF
//                        currentNote.IsDeleted = 1;
                    // TODO: Error handling
                    Note updateNote = Globals.Ctx.Notes.Where(note => note.Id == currentNote.Id).ToList()[0];

                    updateNote.IsDeleted = 1;
                    Globals.Ctx.SaveChanges();
                    LoadAllNotes();
                }
                catch (Exception ex)
                {
                    ErrorNotifyDbConnection(ex);
                }
            }
        }

        private void BtnRightPaneRestore_Click(object sender, RoutedEventArgs e)
        {
            Note currentNote = lvCentrePane.SelectedItem as Note;

            try
            {
                if (MessageBox.Show(string.Format("Would you like to restore the note \"{0}\"?", currentNote.Title), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // currentNote.IsDeleted = false;
                    //currentNote.IsDeleted = 0;
                    //currentNote.UpdateSelfInDb();
                    //LoadAllNotes();

                    // updated with EF
                    currentNote.IsDeleted = 0;
                    Globals.Ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ErrorNotifyDbConnection(ex);
            }
        }

        private void BtnRightPaneEdit_Click(object sender, RoutedEventArgs e)
        {
            NoteEdit editNoteWindow = new NoteEdit(this, lvCentrePane.SelectedItem as Note);
            editNoteWindow.ShowDialog();
        }

        // SIDEBAR NIBS CLICKS
        private void LblSidebarEmptyTrash_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Permanently delete all trash
            try
            {
                if (MessageBox.Show(string.Format("Are you sure you want to permanently delete these {0} notes? This is not reversible.", trashList.Count), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    /*
                        trashList.ForEach(currentNote =>
                        {
                            currentNote.DeleteSelfFromDb();
                        });
                        LoadAllNotes();
                    */
                    // updated with EF
                    // FIXME: Need to test
                    trashList.ForEach(currentNote => Globals.Ctx.Notes.Remove(currentNote));
                    Globals.Ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ErrorNotifyDbConnection(ex);
            }
        }

        private void LblSidebarNewTag_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TagNotebookDialog tagDialog = new TagNotebookDialog(this, ETagNotebookDlgType.CreateTag);

            if (tagDialog.ShowDialog() == true)
            {
                ReloadTagTreeView();
            }
        }

        private void NewTag_MenuClick(object sender, RoutedEventArgs e)
        {
            LblSidebarNewTag_PreviewMouseLeftButtonDown(sender, null);
        }

        private void LblSidebarNewNote_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NoteEdit newNoteWindow = new NoteEdit(this);
            newNoteWindow.Show();
        }

        private void TagRename_PopupMenuClick(object sender, RoutedEventArgs e)
        {
            Tag currentTag = (Tag)trvTags.SelectedItem;
            TagNotebookDialog tagDialog = new TagNotebookDialog(this, ETagNotebookDlgType.UpdateTag, currentTag);

            if (tagDialog.ShowDialog() == true)
            {
                // Update was made in tag dialog,
                trvTags.Items.Refresh();
            }
        }

        private void TagDelete_PopupMenuClick(object sender, RoutedEventArgs e)
        {
            Tag deletingTag = (Tag)trvTags.SelectedItem;
            if (MessageBox.Show(string.Format("Are you sure you want to permanently delete tag \"{0}\"?\nThis is not reversible.", deletingTag.Name), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Globals.Ctx.Tags.Remove(Globals.Ctx.Tags.Where(tag => tag.Id == deletingTag.Id).Single());
                Globals.Ctx.SaveChanges();

                ReloadTagTreeView();
            }
        }

        // Handling right mouse click on left tree view
        private void TrvTags_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private void DockPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MiSidebarTrashItem_Click(sender, null);
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = false;

            trvNotes.Background = null; // Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
        }

        private void TrvTags_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        //    trvTags.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;

        //    TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
        //    if (treeViewItem != null)
         //   {
        //        treeViewItem.IsSelected = false;

                //                treeViewItem.Focus();
                //               e.Handled = true;
        //    }
        }
    }
}

