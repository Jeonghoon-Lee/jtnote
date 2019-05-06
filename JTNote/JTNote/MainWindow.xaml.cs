﻿using System;
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
        List<Note> allNotesList = new List<Note>();
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

            // Load tag list from database
            Globals.ReloadTagList();

            // Load all notes for logged in user
            LoadAllNotes();

            // Set default bindings for window elements
            lvCentrePane.ItemsSource = allNotesList;

            if (lvCentrePane.Items.Count > 0)
                lvCentrePane.SelectedIndex = 0;

            spRightPane.DataContext = lvCentrePane.SelectedItem as Note;

            // Set counts for sidebar items - TODO: Replace these with bindings?
            tbSidebarNotesTitle.Text = string.Format("Notes ({0})", allNotesList.Count);
            tbSidebarTrashTitle.Text = string.Format("Trash ({0})", trashList.Count);
        }

        void LoadAllNotes()
        {
            allNotesList.Clear(); // Clear existing notes list to refresh

            try
            {
                // Populate notes lists from DB
                Globals.Db.GetAllNotesByUserId(Globals.LoginUser.Id).ForEach(note =>
                {
                    if (note.IsDeleted)
                        trashList.Add(note); // Add notes flagged for deletion to trash
                    else
                        allNotesList.Add(note); // Add all other notes to main notes list
                });
            }
            catch (Exception ex)
            {
                // Check for known fatal exceptions, notify user and terminate program if they occur
                if (ex is SqlException || ex is System.IO.IOException)
                {
                    MessageBox.Show(string.Format("Fatal error: unable to connect to the database.{0}{1}", Environment.NewLine, ex.Message), "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
                else if (ex is InvalidCastException || ex is ArgumentException)
                {
                    MessageBox.Show(string.Format("Fatal error: unable to load notes from the database due to corrupt data.{0}{1}", Environment.NewLine, ex.Message), "JTNote", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
                else
                    throw ex;
            }
        }

        private void LvCentrePane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            spRightPane.DataContext = lvCentrePane.SelectedItem as Note;
        }

        private void LblTagAddIcon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TagDialog tagDialog = new TagDialog(this, TagDialogType.Create);

            if (tagDialog.ShowDialog() == true)
            {
                //
                // TODO: Reload tag list and insert into menu item
                //
            }
        }
    }
}
