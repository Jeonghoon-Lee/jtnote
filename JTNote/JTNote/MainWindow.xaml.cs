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
        public MainWindow()
        {
            try
            {
                Globals.Db = new Database();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Fatal error: unable to connect to database\n" + ex.Message, "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();    // set close the main window, terminate the program
                return;
            }

            LoginRegister loginDlg = new LoginRegister();
            if (loginDlg.ShowDialog() != true)
            {
                Close();    // set close the main window event
                return;
            }              

            InitializeComponent();

            // Set login user information onto title bar
            Title = string.Format("JTNote - {0}", Globals.LoginUser.Email);

            // Set default bindings for window elements
            lvCentrePane.ItemsSource = notesList;

            // Load all notes for logged in user
            LoadAllNotes();

            spRightPane.DataContext = lvCentrePane.SelectedItem as Note;
        }

        public void LoadAllNotes()
        {
            notesList.Clear(); // Clear existing notes list to refresh
            trashList.Clear(); // Clear existing trash list to refresh

            try
            {
                // Populate notes lists from DB
                Globals.Db.GetAllNotesByUserId(Globals.LoginUser.Id).ForEach(note =>
                {
                    if (note.IsDeleted)
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
            tbSidebarNotesTitle.Text = string.Format("Notes ({0})", notesList.Count);
            tbSidebarTrashTitle.Text = string.Format("Trash ({0})", trashList.Count);

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
            }
            else if (ex is InvalidCastException || ex is ArgumentException)
            {
                MessageBox.Show(string.Format("Fatal error: unable to perform action in the database due to corrupt data.{0}{1}", Environment.NewLine, ex.Message), "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
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
            if (currentNote.IsDeleted == true)
            {
                // Item is already in trash, permanently delete
                try
                {
                    if (MessageBox.Show(string.Format("Are you sure you want to permanently delete the note \"{0}\"? This is not reversible.", currentNote.Title), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        currentNote.DeleteSelfFromDb();
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
                    currentNote.IsDeleted = true;
                    currentNote.UpdateSelfInDb();
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
                    currentNote.IsDeleted = false;
                    currentNote.UpdateSelfInDb();
                    LoadAllNotes();
                }
            }
            catch (Exception ex)
            {
                ErrorNotifyDbConnection(ex);
            }
        }


        // SIDEBAR NIBS CLICKS
        private void LblSidebarEmptyTrash_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Permanently delete all trash
            try
            {
                if (MessageBox.Show(string.Format("Are you sure you want to permanently delete these {0} notes? This is not reversible.", trashList.Count), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    trashList.ForEach(currentNote =>
                    {
                        currentNote.DeleteSelfFromDb();
                    });
                    LoadAllNotes();
                }
            }
            catch (Exception ex)
            {
                ErrorNotifyDbConnection(ex);
            }
        }
    }
}
