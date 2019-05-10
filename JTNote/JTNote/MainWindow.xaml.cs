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

        // States for centre and right panels
        enum ListState {
            Notes = 0,
            Trash = 1
        };
        ListState listState = ListState.Notes;

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
            Title = string.Format("JTNote - {0}", Globals.LoginUser);
            ReloadTagTreeView();

            // testing
            ReloadNotebookTreeView();

            // Set default bindings for window elements
            lvCentrePane.ItemsSource = notesList;

            // Load all notes for logged in user
            LoadAllNotes();

            if (notesList.Count > 0)
                spRightPane.DataContext = lvCentrePane.SelectedItem as Note;
            else
                HideRightPaneControls();
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

            // FIXME: Error handling
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

            // update number of items
            tblNumberOfNotes.Text = notesList.Count.ToString();
            tblNumberOfTrash.Text = trashList.Count.ToString();

            // Set correct data source and refresh centre pane
            switch (listState)
            {
                case ListState.Notes:
                    lvCentrePane.ItemsSource = notesList;
                    break;
                case ListState.Trash:
                    lvCentrePane.ItemsSource = trashList;
                    break;
                default:
                    MessageBox.Show("Unknown error when loading notes selection!", "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
            lvCentrePane.Items.Refresh();

            if (lvCentrePane.Items.Count > 0)
            {
                lvCentrePane.SelectedIndex = 0;
            }

            spRightPane.DataContext = lvCentrePane.SelectedItem as Note;
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

        void HideRightPaneControls()
        {
            // Hide controls in right pane and replace with message to select something
            spActionButtonContainer.Visibility = Visibility.Hidden;
            spRightPaneTagsContainer.Visibility = Visibility.Hidden;
            rtbContent.Visibility = Visibility.Hidden;
            lblRightPaneNoContentMessage.Visibility = Visibility.Visible;
        }

        void ChangeSidebarSelection(List<Note> newList, string newHighlight)
        {
            // Change data source for centre pane, highlighting correct item in right pane
            lvCentrePane.ItemsSource = newList;

            // If there are no notes in the currently selected list, deselect notes.
            if (newList.Count > 0)
                lvCentrePane.SelectedIndex = 0;
            else
                HideRightPaneControls();

            // TODO: Need to modify after applying tree view
/*
            // Highlight correct menu item
            foreach (MenuItem curItem in mnuSidebar.Items)
            {
                if (curItem.Name == newHighlight)
                    curItem.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                else
                    curItem.Background = null;
            }
*/

            // Change tooltip text for delete button and switch between share and restore buttons in right pane, as appropriate
            if (newHighlight == "miSidebarTrashItem")
            {
                HideRightPaneControls();
            }
            else
            {
                btnRightPaneDelete.ToolTip = "Send this note to trash.";
                btnRightPaneRestore.Visibility = Visibility.Hidden;
                btnRightPaneShare.Visibility = Visibility.Visible;
                listState = ListState.Notes;
            }

            lvCentrePane.Items.Refresh();
        }

        private void LvCentrePane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvCentrePane.SelectedItem == null)
            {
                HideRightPaneControls();
            }
            else
            {
                Note selItem = lvCentrePane.SelectedItem as Note;

                spRightPane.DataContext = selItem;
                spActionButtonContainer.Visibility = Visibility.Visible;
                spRightPaneTagsContainer.Visibility = Visibility.Visible;
                rtbContent.Visibility = Visibility.Visible;
                lblRightPaneNoContentMessage.Visibility = Visibility.Hidden;

                if (selItem.Notebook != null)
                    spNotebookInfo.Visibility = Visibility.Visible;
                else
                    spNotebookInfo.Visibility = Visibility.Hidden;
            }
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
                        // FIXME: Need to test
                        Globals.Ctx.Notes.Remove(currentNote);
                        Globals.Ctx.SaveChanges();
                        // Reload
                        LoadAllNotes();
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
                    // FIXME: Error handling
                    // Need to test more
                    //    Note updateNote = Globals.Ctx.Notes.Where(note => note.Id == currentNote.Id).ToList()[0];
                    //    updateNote.IsDeleted = 1;
                    currentNote.IsDeleted = 1;
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
                    // FIXME: Error handling
                    currentNote.IsDeleted = 0;
                    Globals.Ctx.SaveChanges();
                    // reload
                    LoadAllNotes();
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
            editNoteWindow.Show();
        }

        // SIDEBAR NIBS CLICKS
        private void LblSidebarEmptyTrash_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Permanently delete all trash
            try
            {
                if (MessageBox.Show(string.Format("Are you sure you want to permanently delete these {0} notes? This is not reversible.", trashList.Count), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    // FIXME: Need to test
                    trashList.ForEach(currentNote => Globals.Ctx.Notes.Remove(currentNote));
                    Globals.Ctx.SaveChanges();
                    LoadAllNotes();
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

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = false;

            switch (((TreeViewItem)sender).Tag.ToString())
            {
                case "tviNotes":
                    ChangeSidebarSelection(notesList, "miSidebarNotesItem");
                //    trvTags.Background = null;
                    trvNotes.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                //    trvNotebook.Background = null;
                    trvTrash.Background = null;
                    break;
                case "tviNotebook":
                //    trvNotebook.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                    trvNotes.Background = null;
                    trvTrash.Background = null;
                    break;
                case "tviTrash":
                    ChangeSidebarSelection(trashList, "miSidebarTrashItem");
                    trvNotes.Background = null;
                //    trvNotebook.Background = null;
                    trvTrash.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                    break;
                default:
                    break;
            }
        }

        private void TrvTags_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                trvNotes.Background = null;
                trvTrash.Background = null;
                trvNotebook.Background = null;

                // FIXME: this is not working properly
                treeViewItem.Background = null;
                //                treeViewItem.IsSelected = false;
                //                treeViewItem.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;

            }
        }

        private void NewNotebook_MenuClick(object sender, RoutedEventArgs e)
        {
            LblSidebarNewNotebook_MouseLeftButtonUp(sender, null);
        }

        private void Exit_MenuClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LblSidebarNewNotebook_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TagNotebookDialog notebookDlg = new TagNotebookDialog(this, ETagNotebookDlgType.CreateNotebook);

            if (notebookDlg.ShowDialog() == true)
            {
                ReloadNotebookTreeView();
            }
        }

        private void ReloadNotebookTreeView()
        {
            TreeViewItem ParentNode = (TreeViewItem)trvNotebook.Items[0];
            ParentNode.Items.Clear();

            foreach (var notebook in Globals.LoginUser.Notebooks)
            {
                TreeViewItem newItem = new TreeViewItem() {
                    Tag = notebook.Id.ToString(),
                    Header = string.Format("{0} ({1})", notebook.Name, notebook.Notes.Count)
                };
                ParentNode.Items.Add(newItem);
                newItem.ContextMenu = trvNotebook.Resources["NotebookContext"] as System.Windows.Controls.ContextMenu;
            } 
        }

        private void TrvNotebook_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TrvTags_PreviewMouseRightButtonDown(sender, e);
        }

        private void NotebookRename_PopupMenuClick(object sender, RoutedEventArgs e)
        {
            // FIXME: Error handling
            int notebookId = int.Parse(((TreeViewItem)trvNotebook.SelectedItem).Tag.ToString());
            Notebook currentNotebook = Globals.LoginUser.Notebooks.Where(notebook => notebook.Id == notebookId).SingleOrDefault();

            TagNotebookDialog notebookDialog = new TagNotebookDialog(this, ETagNotebookDlgType.UpdateNotebook, currentNotebook);
            if (notebookDialog.ShowDialog() == true)
            {
                ReloadNotebookTreeView();
            }
        }

        private void NotebookDelete_PopupMenuClick(object sender, RoutedEventArgs e)
        {
            // FIXME: Error handling
            int notebookId = int.Parse(((TreeViewItem)trvNotebook.SelectedItem).Tag.ToString());
            Notebook deletingNotebook = Globals.LoginUser.Notebooks.Where(notebook => notebook.Id == notebookId).SingleOrDefault();

            if (MessageBox.Show(string.Format("Are you sure you want to permanently delete tag \"{0}\"?\nThis is not reversible.", deletingNotebook.Name), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Globals.Ctx.Notebooks.Remove(Globals.Ctx.Notebooks.Where(nt => nt.Id == deletingNotebook.Id).Single());
                Globals.Ctx.SaveChanges();

                ReloadNotebookTreeView();
            }
        }

        private void BtnRightPaneRemoveFromNotebook_Click(object sender, RoutedEventArgs e)
        {
            Note currentNote = lvCentrePane.SelectedItem as Note;
            int currentIndex = lvCentrePane.SelectedIndex;
            currentNote.Notebook = Globals.Ctx.Notebooks.Where(nb => nb.Id == 0).FirstOrDefault();
            Globals.Ctx.SaveChanges();
            LoadAllNotes();
            lvCentrePane.SelectedIndex = currentIndex;
        }

        private void UserSetting_MenuClick(object sender, RoutedEventArgs e)
        {
            UserSettingDialog userSettingDlg = new UserSettingDialog(this);

            if (userSettingDlg.ShowDialog() == true)
            {
                // update title
                Title = string.Format("JTNote - {0}", Globals.LoginUser);
                // check user is deleted
                if (Globals.LoginUser == null)
                {
                    MessageBox.Show("Your account is deleted.\nThank you for using JTNotes.", "JTNote", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
            }
        }

        private void BtnRightPaneShare_Click(object sender, RoutedEventArgs e)
        {
            Note currentNote = lvCentrePane.SelectedItem as Note;
            int currentIndex = lvCentrePane.SelectedIndex;
            currentNote.Notebook = Globals.Ctx.Notebooks.Where(nb => nb.Id == 0).FirstOrDefault();

            SharedNoteDialog sharedNoteDlg = new SharedNoteDialog(this, currentNote);

            // TODO: handle after sharing note
            if (sharedNoteDlg.ShowDialog() == true)
            {

            }

        }
    }
}

