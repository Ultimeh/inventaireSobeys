using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
	/// <summary>
	/// Interaction logic for Admin.xaml
	/// </summary>
	public partial class Admin : Window
	{
		IMClient im;

		public Admin(IMClient imc)
		{
			DataContext = App.appData;
			im = imc;
			InitializeComponent();
		}

		private void MouseClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void ListViewData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void listUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (listUser.SelectedIndex == -1)
			{
				cb_newPriv.IsEnabled = false;
				btn_Apply.IsEnabled = false;
				cb_newPriv.SelectedIndex = -1;
			}
			else
			{
				cb_newPriv.IsEnabled = true;
			}
		}

		private void cb_newPriv_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cb_newPriv.SelectedIndex == -1)
			{
				btn_Apply.IsEnabled = false;
			}
			else
			{
				btn_Apply.IsEnabled = true;
			}
		}

		private void Button1_Click(object sender, RoutedEventArgs e)
		{
			string data = "";
			string user = App.appData.userList[listUser.SelectedIndex].username;
			App.appData.userList[listUser.SelectedIndex].privilege = cb_newPriv.Text;

			if (cb_newPriv.SelectedIndex == 0) data = "0";
			if (cb_newPriv.SelectedIndex == 1) data = "1";
			if (cb_newPriv.SelectedIndex == 2) data = "2";
			if (cb_newPriv.SelectedIndex == 3) data = "3";
			if (cb_newPriv.SelectedIndex == 4) data = "4";
			if (cb_newPriv.SelectedIndex == 5) data = "9";
			im.changePivilege(user, data);

			cb_newPriv.SelectedIndex = -1;
		}

		private void listBackup_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (listBackup.SelectedItems.Count == 0) btn_delFiles.IsEnabled = false;
			else btn_delFiles.IsEnabled = true;
		}

		private void btn_delFiles_Click(object sender, RoutedEventArgs e)
		{
			if (listBackup.SelectedItems.Count == 0) return;

			List<string> temp = new List<string>();

			foreach (string item in listBackup.SelectedItems)
			{
				temp.Add(item);
			}

			im.requestDelete(string.Join("╚", temp));

			foreach (string item in temp)
			{
				App.appData.backup.Remove(item);
			}
		}

		private void listModel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void listBackup_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void option_Click(object sender, RoutedEventArgs e)
		{
			if (listUser.SelectedIndex == -1 || listUser.SelectedItem == null) return;

			User user = listUser.SelectedItem as User;

			var Result = MessageBox.Show("Voulez-vous cet Usager: " + user.username + " ?", "Confirmation de supression", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (Result == MessageBoxResult.Yes)
			{
				im.requestDeleteUser(user.username);
				App.appData.userList.Remove(user);
			}
		}

		private void tb_max_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !char.IsDigit(e.Text.ToCharArray()[0]);
		}

		private void tb_alerte_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !char.IsDigit(e.Text.ToCharArray()[0]);
		}

        private void context_puro_Click(object sender, RoutedEventArgs e)
        {
			im.TrackingPuro();
        }
    }
}
