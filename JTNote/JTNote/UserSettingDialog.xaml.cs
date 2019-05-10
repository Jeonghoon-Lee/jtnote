using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace JTNote
{
    /// <summary>
    /// Interaction logic for UserSettingDialog.xaml
    /// </summary>
    public partial class UserSettingDialog : Window
    {
        public UserSettingDialog(Window owner)
        {
            InitializeComponent();

            Owner = owner;

            // initialize
            tbEmail.Text = Globals.LoginUser.Email;
            tbUsername.Text = Globals.LoginUser.UserName;
        }

        private void TbUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Globals.LoginUser.UserName != tbUsername.Text)
            {
                btSaveChanges.IsEnabled = true;
                return;
            }
            btSaveChanges.IsEnabled = false;
        }

        private void BtSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // TODO: need to implement Regex to filter special character
            // FIXME: implement to handle error
            if (Globals.LoginUser.UserName != tbUsername.Text)
            {
                Globals.LoginUser.UserName = tbUsername.Text;
                Globals.Ctx.SaveChanges();
                DialogResult = true;
            }
        }

        private void BtChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = pbOldPassword.Password;
            string newPassword = pbNewPassword1.Password;

            if (!MD5Hash.VerifyMd5Hash(oldPassword, Globals.LoginUser.Password))
            {
                tblOldPasswordError.Text = "Password does not matched.";
                return;
            }

            if (MD5Hash.VerifyMd5Hash(newPassword, Globals.LoginUser.Password))
            {
                tblNewPasswordError1.Text = "New password is same as old password.";
                return;
            }

            try
            {
                Globals.LoginUser.Password = MD5Hash.GetMd5Hash(newPassword);
                Globals.Ctx.SaveChanges();

                MessageBox.Show("Password was changed successfully.", "JTNotes", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: Can't update database\n" + ex.Message, "JTNotes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void PbNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // check first password is valid
            btChangePassword.IsEnabled = false;

            if ((PasswordBox)sender == pbNewPassword1 && !RegexUtils.IsValidPassword(pbNewPassword1.Password))
            {
                // TODO: Make error message with detail information
                // special character, number, capital letter and etc
                tblNewPasswordError1.Text = "Invalid password: length should be 2-20 characters long";
                return;
            }
            tblNewPasswordError1.Text = "";

            if ((PasswordBox)sender == pbNewPassword2 && !RegexUtils.IsValidPassword(pbNewPassword2.Password))
            {
                // TODO: Make error message with detail information
                // special character, number, capital letter and etc
                tblNewPasswordError2.Text = "Invalid password: length should be 2-20 characters long";
                return;
            }
            tblNewPasswordError2.Text = "";

            if (pbNewPassword1.Password != pbNewPassword2.Password)
            {
                tblNewPasswordError2.Text = "Invalid password: password is not same";
                return;
            }
            tblNewPasswordError2.Text = "";

            btChangePassword.IsEnabled = true;
        }

        private void BtDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            string password = pbPassword.Password;

            if (!MD5Hash.VerifyMd5Hash(password, Globals.LoginUser.Password))
            {
                tblPasswordError.Text = "Password does not matched.";
                return;
            }

            if (MessageBox.Show(string.Format("Are you sure you want to permanently delete your account \"{0}\"?\nAll of your data will be deleted and this is not reversible.", Globals.LoginUser.Email), "JTNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Globals.Ctx.Users.Remove(Globals.LoginUser);
                Globals.Ctx.SaveChanges();

                Globals.LoginUser = null;
                DialogResult = true;
            }
        }

        private void CbUnsubscribe_Click(object sender, RoutedEventArgs e)
        {
            if (cbUnsubscribe.IsChecked == true)
            {
                btDeleteAccount.IsEnabled = true;
                return;
            }
            btDeleteAccount.IsEnabled = false;
        }
    }
}
