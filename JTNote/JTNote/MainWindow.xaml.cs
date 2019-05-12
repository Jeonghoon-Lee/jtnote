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
        List<Note> centerPaneNoteList = new List<Note>();
        Tag selectedTag = new Tag();
        Notebook selectedNotebook = new Notebook();

        // States for centre and right panels
        enum EListState { Tags, Notes, Notebooks, SharedNotes, Trash, Search };

        EListState listState = EListState.Notes;        // set default left treeview item

        public MainWindow()
        {
            LoginRegister loginDlg = new LoginRegister();
            if (loginDlg.ShowDialog() != true)
            {
                Close();    // set close the main window event
                return;
            }              

            InitializeComponent();

            // Set login user information onto title bar
            Title = string.Format("JTNote - {0}", Globals.LoginUser);

            // Set default bindings for window elements
            lvCentrePane.ItemsSource = centerPaneNoteList;

            // Load all notes for logged in user
            LoadAllNotes();
        }

        private void ReloadTagTreeView()
        {
            // FIXME: Try to use binding for tree view
            TreeViewItem ParentNode = (TreeViewItem)trvTags.Items[0];
            ParentNode.Items.Clear();

            foreach (Tag tag in Globals.LoginUser.Tags)
            {
                TreeViewItem newItem = new TreeViewItem()
                {
                    DataContext = tag,
                    Header = string.Format("{0} ({1})", tag.Name, tag.Notes.Count)
                };
                ParentNode.Items.Add(newItem);
                newItem.ContextMenu = trvTags.Resources["TagContext"] as System.Windows.Controls.ContextMenu;
            }
        }

        private void ReloadNotebookTreeView()
        {
            // FIXME: Try to use binding for tree view
            TreeViewItem ParentNode = (TreeViewItem)trvNotebook.Items[0];
            ParentNode.Items.Clear();

            foreach (Notebook notebook in Globals.LoginUser.Notebooks)
            {
                TreeViewItem newItem = new TreeViewItem()
                {
                    DataContext = notebook,
                    Header = string.Format("{0} ({1})", notebook.Name, notebook.Notes.Count)
                };
                ParentNode.Items.Add(newItem);
                newItem.ContextMenu = trvNotebook.Resources["NotebookContext"] as System.Windows.Controls.ContextMenu;
            }
        }

        public void LoadAllNotes()
        {
            ReloadTagTreeView();
            ReloadNotebookTreeView();

            // clear existing notes list from Center pane to refresh
            if (listState == EListState.Tags)
            {
                if (selectedTag != null)
                {
                    centerPaneNoteList.Clear();
                }
            }
            else
            {
                centerPaneNoteList.Clear();
            }

            tblNumberOfNotes.Text = Globals.LoginUser.Notes.Where(note => note.IsDeleted == 0).ToList().Count().ToString();
            tblNumberOfTrash.Text = Globals.LoginUser.Notes.Where(note => note.IsDeleted == 1).ToList().Count().ToString();
            tblNotebookNumberOfNotes.Text = Globals.LoginUser.Notes.Where(note => note.IsDeleted == 0 && note.Notebook == null).Count().ToString();
            tblNumberOfSharedNote.Text = Globals.LoginUser.SharedNotes.Count.ToString();

            // Set correct data source and refresh centre pane. Set to null first to make sure itemssource reloads
            switch (listState)
            {
                case EListState.Tags:
                    if (selectedTag != null)
                    {
                        centerPaneNoteList.AddRange(selectedTag.Notes.Where(note => note.IsDeleted == 0).OrderByDescending(item => item.LastUpdatedDate));
                    }
                    break;
                case EListState.Notes:
                    centerPaneNoteList.AddRange(Globals.LoginUser.Notes.Where(note => note.IsDeleted == 0).OrderByDescending(item => item.LastUpdatedDate));
                    break;
                case EListState.Notebooks:
                    if (selectedNotebook == null)
                    {
                        centerPaneNoteList.AddRange(Globals.LoginUser.Notes.Where(note => note.IsDeleted == 0 && note.Notebook == null).OrderByDescending(item => item.LastUpdatedDate));
                    }
                    else
                    {
                        centerPaneNoteList.AddRange(selectedNotebook.Notes.Where(note => note.IsDeleted == 0));
                    }
                    break;
                case EListState.SharedNotes:
                    centerPaneNoteList.AddRange(Globals.LoginUser.SharedNotes.Select(shared => shared.Note).OrderByDescending(item => item.LastUpdatedDate));
                    break;
                case EListState.Trash:
                    centerPaneNoteList.AddRange(Globals.LoginUser.Notes.Where(note => note.IsDeleted == 1).OrderByDescending(item => item.LastUpdatedDate));
                    break;
                case EListState.Search:
                    break;
                default:
                    MessageBox.Show("Unknown error when loading notes selection!", "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
            // Refresh pane
            lvCentrePane.Items.Refresh();

            if (centerPaneNoteList.Count > 0)
            {
                lvCentrePane.SelectedIndex = 0;
                spRightPane.DataContext = null;     // clear
                spRightPane.DataContext = centerPaneNoteList[0];
            }
            else
            {
                HideRightPaneControls();
            }

            // Change tooltip text for delete button and switch between share and restore buttons in right pane, as appropriate
            if (listState == EListState.Trash)
            {
                btnRightPaneDelete.ToolTip = "Permanently delete this note.";
                btnRightPaneEdit.Visibility = Visibility.Visible;
                btnRightPaneDelete.Visibility = Visibility.Visible;
                btnRightPaneRestore.Visibility = Visibility.Visible;
                btnRightPaneShare.Visibility = Visibility.Hidden;
            }
            else if (listState == EListState.SharedNotes)
            {
                btnRightPaneEdit.Visibility = Visibility.Hidden;
                btnRightPaneDelete.Visibility = Visibility.Hidden;
                btnRightPaneRestore.Visibility = Visibility.Hidden;
                btnRightPaneShare.Visibility = Visibility.Hidden;
            }
            else
            {
                btnRightPaneDelete.ToolTip = "Send this note to trash.";
                btnRightPaneEdit.Visibility = Visibility.Visible;
                btnRightPaneDelete.Visibility = Visibility.Visible;
                btnRightPaneRestore.Visibility = Visibility.Hidden;
                btnRightPaneShare.Visibility = Visibility.Visible;
            }
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

        private void LvCentrePane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvCentrePane.SelectedIndex == -1)
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

            if (currentNote.IsDeleted == 1)
            {
                // Item is already in trash, permanently delete
                try
                {
                    if (MessageBox.Show(string.Format("Are you sure you want to permanently delete the note \"{0}\"? This is not reversible.", currentNote.Title), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
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
                    currentNote.IsDeleted = 1;
                    Globals.Ctx.SaveChanges();
                    // Reload
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
                if (MessageBox.Show(string.Format("Are you sure you want to permanently delete these {0} notes? This is not reversible.", centerPaneNoteList.Count), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    centerPaneNoteList.ForEach(currentNote => Globals.Ctx.Notes.Remove(currentNote));
                    Globals.Ctx.SaveChanges();
                    LoadAllNotes();
                }
            }
            catch (Exception ex)
            {
                ErrorNotifyDbConnection(ex);
            }
        }

        private void LblSidebarNewNote_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NoteEdit newNoteWindow = new NoteEdit(this);
            newNoteWindow.Show();
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

        private void TagRename_PopupMenuClick(object sender, RoutedEventArgs e)
        {
            // TODO: need to optimize code
            listState = EListState.Tags;
            trvNotes.Background = null;
            trvTrash.Background = null;
            trvNotebook.Background = null;

            selectedTag = (Tag)((TreeViewItem)trvTags.SelectedItem).DataContext;

            LoadAllNotes();

            TagNotebookDialog tagDialog = new TagNotebookDialog(this, ETagNotebookDlgType.UpdateTag, selectedTag);

            if (tagDialog.ShowDialog() == true)
            {
                LoadAllNotes();
            }
        }

        private void TagDelete_PopupMenuClick(object sender, RoutedEventArgs e)
        {
            // FIXME: Error handling
            listState = EListState.Tags;
            trvNotes.Background = null;
            trvTrash.Background = null;
            trvNotebook.Background = null;

            selectedTag = (Tag)((TreeViewItem)trvTags.SelectedItem).DataContext;

            LoadAllNotes();

            if (MessageBox.Show(string.Format("Are you sure you want to permanently delete tag \"{0}\"?\nThis is not reversible.", selectedTag.Name), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Globals.Ctx.Tags.Remove(Globals.Ctx.Tags.Where(tag => tag.Id == selectedTag.Id).Single());
                Globals.Ctx.SaveChanges();

                LoadAllNotes();
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
                case "tviTags":
                    listState = EListState.Tags;
                    trvNotes.Background = null;
                    trvSharedNote.Background = null;
                    trvTrash.Background = null;
                    trvNotebook.Background = null;
                    break;
                case "tviNotes":
                    listState = EListState.Notes;
                    trvNotes.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                    trvSharedNote.Background = null;
                    trvTrash.Background = null;
                    trvNotebook.Background = null;
                    break;
                case "tviNotebook":
                    listState = EListState.Notebooks;
                    trvNotes.Background = null;
                    trvSharedNote.Background = null;
                    trvTrash.Background = null;
                    trvNotebook.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                    selectedNotebook = null;    // notes that have no notebook (means root node)
                    break;
                case "tviShared":
                    listState = EListState.SharedNotes;
                    trvNotes.Background = null;
                    trvTrash.Background = null;
                    trvNotebook.Background = null;
                    trvSharedNote.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                    break;
                case "tviTrash":
                    listState = EListState.Trash;
                    trvNotes.Background = null;
                    trvNotebook.Background = null;
                    trvSharedNote.Background = null;
                    trvTrash.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                    break;
                default:
                    break;
            }

            LoadAllNotes();
        }

        private void TrvTags_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                listState = EListState.Tags;
                trvNotes.Background = null;
                trvTrash.Background = null;
                trvNotebook.Background = null;
                trvSharedNote.Background = null;

                // FIXME: this is not working properly
                // treeViewItem.Background = null;
                //                treeViewItem.IsSelected = false;
                //                treeViewItem.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                selectedTag = (Tag)treeViewItem.DataContext;
                LoadAllNotes();
            }
        }

        private void TrvNotebook_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                listState = EListState.Notebooks;
                trvTags.Background = null;
                trvNotes.Background = null;
                trvTrash.Background = null;
                //trvNotebook.Background = null;
                // treeViewItem.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                trvSharedNote.Background = null;

                // FIXME: this is not working properly
                // treeViewItem.Background = null;
                //                treeViewItem.IsSelected = false;
                //                treeViewItem.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                selectedNotebook = (Notebook)treeViewItem.DataContext;
                LoadAllNotes();
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

        private void TrvNotebook_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                listState = EListState.Notebooks;
                trvTags.Background = null;
                trvNotes.Background = null;
                trvTrash.Background = null;
                trvSharedNote.Background = null;

                // FIXME: this is not working properly
                // treeViewItem.Background = null;
                //                treeViewItem.IsSelected = false;
                //                treeViewItem.Background = Application.Current.Resources["PrimaryHueMidBrush"] as Brush;
                selectedNotebook = (Notebook)treeViewItem.DataContext;
                LoadAllNotes();
            }
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
                LoadAllNotes();
            }
        }    
    }
}

