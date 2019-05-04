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
    /// Interaction logic for LoginRegister.xaml
    /// </summary>
    public partial class LoginRegister : Window
    {
        public LoginRegister()
        {
            InitializeComponent();
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void TextBoxEmailRegister_TextChanged(object sender, TextChangedEventArgs e)
        {
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
            tblRegEmailError.Text = "";
        }

        private void PasswordBoxRegister_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // check first password is valid
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

            if (RegexUtils.IsValidEmail(tbRegisterEmail.Text))
            {
                Console.WriteLine("Email");
            }
        }
    }
}
