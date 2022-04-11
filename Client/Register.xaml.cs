using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        IMClient im;

        public Register(IMClient imc)
        {
            InitializeComponent();
            DataContext = App.appData;
            tbUserName.Focus();
            im = imc;
            im.RegisterOK += new EventHandler(im_RegisterOK);
            im.RegisterFailed += new IMErrorEventHandler(im_RegisterFailed);
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (tbPassword.Password != confirm.Password)
            {
                MessageBox.Show("Le nouveau mot de passe et la confirmation sont différent." + Environment.NewLine + "Essayez de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
                tbPassword.Clear();
                confirm.Clear();
                return;
            }

            App.appData.UserName = tbUserName.Text.Trim();
            App.appData.Password = tbPassword.SecurePassword;

            if (App.appData.UserName != "" && App.appData.Password.Length != 0)
            {
                im.Register();
                lblStatus.Content = "Registering...";
                App.appData.registerEnable = false;
                App.appData.registerForm = false;
            }
            else
            {
                MessageBox.Show("Vous devez enter un nom d'utilisateur et un mot de passe.");
            }
        }

        private void im_RegisterOK(object sender, EventArgs e)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                tbPassword.Clear();
                confirm.Clear();
                lblStatus.Content = "Registered!";
                App.appData.registerEnable = false;
                App.appData.registerForm = true;
                App.appData.connectEnable = true;
                this.Close();
                // btnDisconnect.IsEnabled = true;
                //btnSend.IsEnabled = true;
            });
        }

        void im_RegisterFailed(object sender, IMErrorEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lblStatus.Content = "Register failed!";
                App.appData.connectEnable = true;
                App.appData.registerEnable = true;
                App.appData.registerForm = true;
                tbPassword.Clear();
                confirm.Clear();
            });

            MessageBox.Show("Enregistrement a échoué, ce nom d'utilisateur existe deja.");
        }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Register_Click(sender, e);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbUserName.Text != "") App.appData.registerEnable = true;
            else App.appData.registerEnable = false;
        }

        private void MouseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RegisterWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Register_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            im.RegisterOK -= new EventHandler(im_RegisterOK);
            im.RegisterFailed -= new IMErrorEventHandler(im_RegisterFailed);
        }

        private void tbUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemComma) e.Handled = true;
        }
    }
}
