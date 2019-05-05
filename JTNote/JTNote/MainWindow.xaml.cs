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
        List<Note> allNotesList = new List<Note>();
        public MainWindow()
        {
            try
            {
                Globals.Db = new Database();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Fatal error: unable to connect to database\n" + ex.Message, "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
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
        }

        void LoadAllNotes()
        {
            // TODO (in progress): Load list of all notes from DB

        }
    }
}
