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
        public MainWindow()
        {
            try
            {
                Globals.Db = new Database();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Fatal error: unable to connect to database\n" + ex.Message, "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
                Close(); // close the main window, terminate the program
            }

            InitializeComponent();
        }
    }
}
