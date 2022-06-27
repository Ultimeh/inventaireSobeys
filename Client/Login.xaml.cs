using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Client
{
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : Window
	{
		IMClient im;
		public Login(IMClient imc)
		{
			DataContext = App.appData;
			im = imc;
			im.LoginOK += new EventHandler(im_LoginOK);
			im.LoginFailed += new IMErrorEventHandler(im_LoginFailed);

			if (App.appData.netChange)
			{
				App.appData.connectEnable = false;
				App.appData.registerForm = false;
				im.Login();
			}

			InitializeComponent();
			checkFolder();

			if (App.appData.settings.remember && App.appData.settings.user != "") App.appData.UserName = App.appData.settings.user;
			else App.appData.UserName = "";
		}


		private void Register_Click(object sender, RoutedEventArgs e)
		{
			Register register = new Register(im);
			register.Owner = this;
			register.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			register.ShowDialog();

			//if (App.appData.UserName != null && App.appData.Password != null)
			//{
			//    im.Register();
			//    lblStatus.Content = "Registering...";
			//}
			//else
			//{
			//    MessageBox.Show("You must enter a username and a password");
			//}
		}

		private void Button1_Click(object sender, RoutedEventArgs e)
		{
			//App.appData.UserName = tbUserName.Text;
			App.appData.Password = tbPassword.SecurePassword;
			//tbUserName.Text = "";
			//tbPassword.Password = "";
			Connect();
		}

		private void Connect()
		{
			if (App.appData.UserName != "" && App.appData.Password.Length != 0)
			{
				im.Login();
				lblStatus.Content = "Login...";
				App.appData.connectEnable = false;
				App.appData.registerForm = false;
			}
			else
			{
				MessageBox.Show("Vous devez enter un nom d'utilisateur et un mot de passe.");
				App.appData.connectEnable = true;
			}

			tbPassword.Clear();
		}

		void im_LoginFailed(object sender, IMErrorEventArgs e)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				if (e.Error == IMError.WrongPassword)
				{
					MessageBox.Show("Le mot de passe n'est pas valide");
					App.appData.registerForm = true;
					tbPassword.Focus();
				}
				else if (e.Error == IMError.NoExists)
				{
					MessageBox.Show("Ce nom d'utilisateur n'existe pas.");
					App.appData.registerForm = true;
					tbUserName.Focus();
				}

				lblStatus.Content = "Login failed!";
				App.appData.connectEnable = true;
				App.appData.registerForm = true;
				tbPassword.Clear();
				App.appData.Password.Clear();
			});
		}

		void im_LoginOK(object sender, EventArgs e)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				App.appData.settings.lastServer = serverList.SelectedIndex;
				if (App.appData.settings.remember) saveUser();

				lblStatus.Content = "Logged in!";
				// btnRegister.IsEnabled = false;
				// btnConnect.IsEnabled = false;
				//btnDisconnect.IsEnabled = true;
				//btnSend.IsEnabled = true;
				App.appData.quit = false;
				App.appData.registerForm = true;
				tbPassword.Clear();
				//App.appData.Password.Clear();
				//App.appData.UserName = "";
				App.appData.netChange = false;
				CloseLoginForm();
			});
		}

		void CloseLoginForm()
		{
			im.LoginOK -= new EventHandler(im_LoginOK);
			im.LoginFailed -= new IMErrorEventHandler(im_LoginFailed);
			this.Close();
		}

		private void checkFolder()
		{
			string folder = @"c:\Inventaire Sobeys Settings";
			string folder2 = @"c:\Inventaire Sobeys Settings\Rapport";

			if (!Directory.Exists(folder))
			{
				try
				{
					Directory.CreateDirectory(folder);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
					return;
				}
			}
			if (!Directory.Exists(folder2))
			{
				try
				{
					Directory.CreateDirectory(folder2);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
					return;
				}
			}
		}

		private void serverList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (serverList.SelectedIndex == -1)
			{
				App.appData.connectEnable = false;
				return;
			}

			ServerName selection = (ServerName)serverList.SelectedItem;
			App.appData.IPAddress = selection.Server;
			App.appData.Port = selection.Port;
			App.appData.connectEnable = true;

			if (!string.IsNullOrEmpty(App.appData.UserName)) tbPassword.Focus();
			else tbUserName.Focus();
		}

		private void serverList_Loaded(object sender, RoutedEventArgs e)
		{
			string serversList = @"c:\Inventaire Sobeys Settings\ServerList.xml";

			if (File.Exists(serversList))
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<ServerName>));
					using (StreamReader rd = new StreamReader(serversList))
					{
						App.appData.serverList = xs.Deserialize(rd) as ObservableCollection<ServerName>;
					}
				}
				catch (Exception)
				{
					MessageBox.Show("Problem detected with 'ServerList.xml' in the 'settings' folder." + Environment.NewLine + "The file was not saved properly or you may have edited the file incorrectly.", "Loading Server List", MessageBoxButton.OK, MessageBoxImage.Warning);
				}

				serverList.SelectedIndex = App.appData.settings.lastServer;
			}
			else
			{
				App.appData.serverList.Add(new ServerName { Server = "inv-entrepot", Port = 1026 });

				serverList.SelectedIndex = 0;
			}
		}

		private void saveServerList()
		{
			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<ServerName>));
				using (StreamWriter wr = new StreamWriter(@"c:\Inventaire Sobeys Settings\ServerList.xml"))
				{
					xs.Serialize(wr, App.appData.serverList);
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Problem detected when saving new Server." + Environment.NewLine + "Make sure you have Write permission.", "Add Server", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void btnAddServer_Click(object sender, RoutedEventArgs e)
		{
			var newWindow = new AddServer();
			newWindow.ShowDialog();
			saveServerList();
			if (App.appData.ok) serverList.SelectedIndex = serverList.Items.Count - 1;
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			ServerName selection = (ServerName)serverList.SelectedItem;
			serverList.SelectedIndex = -1;
			App.appData.serverList.Remove(selection);
			saveServerList();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			App.appData.quit = true;
			this.Close();
		}

		private void tbPassword_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter) Button1_Click(sender, e);
		}

		private void MouseClick(object sender, RoutedEventArgs e)
		{
			App.appData.quit = true;
			this.Close();
		}

		private void login_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void tbUserName_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.OemComma) e.Handled = true;
		}

		private void saveUser()
		{
			App.appData.settings.user = App.appData.UserName;
		}

		private void check_Remember_Unchecked(object sender, RoutedEventArgs e)
		{
			App.appData.settings.user = "";
		}

		private void About_Window(object sender, RoutedEventArgs e)
		{
			var window = new About();
			window.ShowDialog();
		}

		private void login_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				string version = "";
				int counter = 0;

				foreach (string line in File.ReadLines(@".\ChangeLog.txt"))
				{
					counter++;

					if (counter == 2)
					{
						version = line;
						break;
					}
				}

				if (!version.Contains(App.appData.version))
				{
					MessageBox.Show("Votre Client n'est pas a la derniere version." + Environment.NewLine + "Cicker OK pour fermer l'application et attendre quelques minutes avant de ré-ouvrir pour que la mise a jour automatique puisse s'effectuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Information);
					App.appData.quit = true;
					this.Close();
				}
			}
			catch { }
		}

        private void ctx_Aide(object sender, RoutedEventArgs e)
        {
			try
			{
				File.Copy(@".\Documentation.docx", @"C:\Inventaire Sobeys Settings\Documentation.docx", true);

				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					UseShellExecute = true,
					FileName = @"C:\Inventaire Sobeys Settings\Documentation.docx",
				};

				Process.Start(startInfo);
			}
			catch { }
		}
    }
}
