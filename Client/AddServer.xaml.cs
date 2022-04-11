using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for AddServer.xaml
    /// </summary>
    public partial class AddServer : Window
    {
        public AddServer()
        {
            DataContext = App.appData;
            InitializeComponent();
            serverPort.Text = "1025";
            serverText.Focus();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (int.Parse(serverPort.Text) <= 1024 || (int.Parse(serverPort.Text) > 65535)) 
            {
                MessageBox.Show("Valid custom Port range is from 1024 to 65535");
                return;    
            }

            App.appData.serverList.Add(new ServerName { Server = serverText.Text, Port = int.Parse(serverPort.Text) });
            App.appData.ok = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            App.appData.ok = false;
            this.Close();
        }

        private void serverPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            disableAdd();
        }

        private void serverText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            disableAdd();
        }

        private void disableAdd()
        {
            if (serverPort.Text == "" || serverText.Text == "") btnAdd.IsEnabled = false;
            else if (serverPort.Text != "" && serverText.Text != "") btnAdd.IsEnabled = true;
        }
 
        private void serverPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);       
        }

        private void MouseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void serverPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) btnAdd_Click(sender, e);
        }

        private void serverText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && btnAdd.IsEnabled) btnAdd_Click(sender, e);
        }
    }
}
