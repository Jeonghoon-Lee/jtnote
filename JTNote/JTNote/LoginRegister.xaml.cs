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
using System.Windows.Shapes;

namespace JTNote
{
    /// <summary>
    /// Interaction logic for LoginRegister.xaml
    /// </summary>
    public partial class LoginRegister : Window
    {
        private bool isValidEmail = false;
        private bool isValidPassword = false;

        public LoginRegister()
        {
            InitializeComponent();
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string loginEmail = tbLoginEmail.Text;
                if (loginEmail == "")
                {
                    MessageBox.Show("Error: please enter email address", "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                User loginUser = Globals.Db.GetUser(loginEmail);
                string enteredPassword = pbLoginPassword.Password;

                if (loginUser.Email != loginEmail || !MD5Hash.VerifyMd5Hash(enteredPassword, loginUser.Password))
                {
                    MessageBox.Show("Error: login failed", "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DialogResult = true;
                // Save login user information for future use
                Globals.LoginUser = loginUser;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: unable to login\nPlease try again\n" + ex.Message, "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            if (!isValidEmail)
            {
                MessageBox.Show("Invalid email: please check email.", "JTNotes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!isValidPassword) 
            {
                MessageBox.Show("Invalid password: please check password.", "JTNotes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // TODO: userName will use future.
                string userName = "";
                string email = tbRegisterEmail.Text;
                string password = MD5Hash.GetMd5Hash(pbRegisterPasswd1.Password);

                if (Globals.Db.ExistsEmail(email))
                {
                    MessageBox.Show("Error: email is already registered.\nPlease change email\n", "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                User registeredUser = new User() { UserName = userName, Email = email, Password = password };
                Globals.Db.AddUser(registeredUser);

                MessageBox.Show("User registration was successful.\nContinue to login", "JTNotes", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                // Save login user information for future use
                Globals.LoginUser = registeredUser;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: unable to register user\nPlease try again\n" + ex.Message, "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void TextBoxEmailRegister_TextChanged(object sender, TextChangedEventArgs e)
        {
            isValidEmail = false;
            if (tbRegisterEmail.Text == "")
            {
                tblRegEmailError.Text = "Please enter email";
                return;
            }
            if (!RegexUtils.IsValidEmail(tbRegisterEmail.Text))
            {
                tblRegEmailError.Text = "Invalid email";
                return;
            }
            isValidEmail = true;
            tblRegEmailError.Text = "";
        }

        private void PasswordBoxRegister_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // check first password is valid
            isValidPassword = false;
            if ((PasswordBox)sender == pbRegisterPasswd1 && !RegexUtils.IsValidPassword(pbRegisterPasswd1.Password))
            {
                // TODO: Make error message with detail information
                // special character, number, capital letter and etc
                tblRegPasswdError1.Text = "Invalid password: length should be 2-20 characters long";
                return;
            }
            tblRegPasswdError1.Text = "";

            if ((PasswordBox)sender == pbRegisterPasswd2 && !RegexUtils.IsValidPassword(pbRegisterPasswd2.Password))
            {
                // TODO: Make error message with detail information
                // special character, number, capital letter and etc
                tblRegPasswdError2.Text = "Invalid password: length should be 2-20 characters long";
                return;
            }
            tblRegPasswdError2.Text = "";

            if (pbRegisterPasswd1.Password != pbRegisterPasswd2.Password)
            {
                tblRegPasswdError2.Text = "Invalid password: password is not same";
                return;
            }

            isValidPassword = true;
            tblRegPasswdError2.Text = "";
        }

        private void CbRegisterAccept_Click(object sender, RoutedEventArgs e)
        {
            if (cbRegisterAccept.IsChecked == false)
            {
                tbRegister.IsEnabled = false;
                return;
            }
            tbRegister.IsEnabled = true;
        }

        private void TbLoginEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbLoginEmail.Text == "")
            {
                btLogin.IsEnabled = false;
                return;
            }
            btLogin.IsEnabled = true;
        }
    }
}
