using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        IMClient im;
        Stopwatch watch = new Stopwatch();
        public ChangePassword(IMClient imc)
        {
            InitializeComponent();
            DataContext = App.appData;
            im = imc;
            oldPassword.Focus();
        }

        private void ChangePasswordForm_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (newPassword.Password != confirmPassword.Password)
            {
                MessageBox.Show("Le nouveau mot de passe et la confirmation sont différent." + Environment.NewLine + "Essayez de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
                oldPassword.Clear();
                newPassword.Clear();
                confirmPassword.Clear();
                return;
            }

            App.appData.oldPassword = oldPassword.SecurePassword;
            App.appData.Password = confirmPassword.SecurePassword;

            im.ChangePassword();

            oldPassword.Clear();
            newPassword.Clear();
            confirmPassword.Clear();
            //App.ViewModel.Password.Clear();
            App.appData.oldPassword.Clear();

            if (watch.ElapsedMilliseconds != 0) watch.Reset();

            App.appData.confirmUserInfo = false;
            App.appData.serverAnswer = false;
            await Task.Run(serverAnswer);

            if (watch.ElapsedMilliseconds >= 5000)
            {
                MessageBox.Show("Couldn't Receive Server confirmation for the password change." + Environment.NewLine + 
                    "Possible Connection issue betwween you and the Server" + Environment.NewLine + "Unknown if the password was changed or not.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            if (App.appData.confirmUserInfo)
            {
                MessageBox.Show("Le mot de passe a été modifié.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                this.Close();
                return;
            }
            else MessageBox.Show("Mauvais mot de passe actuel." + Environment.NewLine + "Essayez de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void serverAnswer()
        {
            watch.Start();
            while (!App.appData.serverAnswer)
            {
                Thread.Sleep(10);
                if (watch.ElapsedMilliseconds >= 5000) break;
            }
            watch.Stop();
        }

        private void MouseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void confirmPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) btnChangePassword_Click(sender, e);
        }

        private void buttonEnable()
        {
            if (oldPassword.Password.Length == 0 || newPassword.Password.Length == 0 || confirmPassword.Password.Length == 0) App.appData.confirmEnable = false;
            else App.appData.confirmEnable = true;
        }

        private void newPassword_KeyUp(object sender, KeyEventArgs e)
        {
            buttonEnable();
        }

        private void oldPassword_KeyUp(object sender, KeyEventArgs e)
        {
            buttonEnable();
        }

        private void confirmPassword_KeyUp(object sender, KeyEventArgs e)
        {
            buttonEnable();
        }
    }
}
