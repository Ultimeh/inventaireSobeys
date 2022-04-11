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

		private void cb_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cb_type.SelectedIndex == -1)
			{
				listModel.ItemsSource = null;
				btn_addModel.IsEnabled = false;
			}

			if (cb_type.SelectedIndex == 0)
			{
				listModel.ItemsSource = App.appData.modeleMoniteur;
			}

			if (cb_type.SelectedIndex != -1 && !string.IsNullOrWhiteSpace(tb_addModel.Text)) btn_addModel.IsEnabled = true;
		}

		private void tb_addModel_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(tb_addModel.Text)) btn_addModel.IsEnabled = false;
			else if (!string.IsNullOrWhiteSpace(tb_addModel.Text) && cb_type.SelectedIndex != -1) btn_addModel.IsEnabled = true;
		}

		private void listModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (listModel.SelectedIndex == -1) btn_delModel.IsEnabled = false;
			else btn_delModel.IsEnabled = true;
		}

		private void btn_addModel_Click(object sender, RoutedEventArgs e)
		{
			//if (cb_type.SelectedIndex == 0) type = "poste";
			//if (cb_type.SelectedIndex == 1) type = "laptop";
			//if (cb_type.SelectedIndex == 2) type = "tablette";
			//if (cb_type.SelectedIndex == 3) type = "moniteur";
			//if (cb_type.SelectedIndex == 4) type = "nip";

			im.updateModel(tb_addModel.Text.Trim(), "add");

			tb_addModel.Text = "";
		}

		private void btn_delModel_Click(object sender, RoutedEventArgs e)
		{
			if (listModel.SelectedItem is null) return;

			//if (cb_type.SelectedIndex == 0) type = "poste";
			//if (cb_type.SelectedIndex == 1) type = "laptop";
			//if (cb_type.SelectedIndex == 2) type = "tablette";
			//if (cb_type.SelectedIndex == 3) type = "moniteur";
			//if (cb_type.SelectedIndex == 4) type = "nip";

			im.updateModel(listModel.SelectedItem.ToString(), "del");
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

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			cb_type.SelectedIndex = 0;
		}

		private void seuilView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (seuilView.SelectedIndex == -1)
			{
				tb_type.Text = "";
				tb_alerte.Text = "";
				tb_max.Text = "";
				tb_alerte.IsEnabled = false;
				tb_max.IsEnabled = false;

				tb_moniteur.Text = "";
				App.appData.choixLCD.Clear();
				btn_lcd.Content = "Ajouter";
				tb_type.IsEnabled = true;
				return;
			}

			Seuil item = seuilView.SelectedItem as Seuil;

			tb_type.Text = item.type;
			tb_alerte.Text = item.seuil;
			tb_max.Text = item.max;
			tb_alerte.IsEnabled = true;
			tb_max.IsEnabled = true;

			bool moniteur = false;



			foreach (var data in App.appData.seuilAdmin.ToArray())
			{
				if (item.type != "Postes" && item.type != "Portables" && item.type != "Tablettes" && data.type == item.type)
				{
					App.appData.choixLCD.Clear();
					tb_moniteur.Text = data.type;

					foreach (var modele in data.modele)
					{
						App.appData.choixLCD.Add(modele);
					}

					moniteur = true;
					btn_lcd.Content = "Modifier";
					App.appData.delSeuil = true;
					break;
				}
			}

			if (!moniteur)
			{
				tb_moniteur.Text = "";
				App.appData.choixLCD.Clear();
				btn_lcd.Content = "Ajouter";
				App.appData.delSeuil = false;
			}
		}

		private void btn_Modifier_Click(object sender, RoutedEventArgs e)
		{
			im.SeuilAdminModif(tb_type.Text, tb_alerte.Text, tb_max.Text);

			foreach (var item in App.appData.seuilAdmin.ToArray())
			{
				if (item.type == tb_type.Text)
				{
					item.seuil = tb_alerte.Text;
					item.max = tb_max.Text;
					break;
				}
			}

			seuilView.SelectedIndex = -1;
		}

		private void tb_alerte_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(tb_alerte.Text)) btn_Modifier.IsEnabled = false;
			else if (!string.IsNullOrWhiteSpace(tb_alerte.Text) && !string.IsNullOrWhiteSpace(tb_max.Text)) btn_Modifier.IsEnabled = true;
		}

		private void tb_max_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(tb_max.Text)) btn_Modifier.IsEnabled = false;
			else if (!string.IsNullOrWhiteSpace(tb_alerte.Text) && !string.IsNullOrWhiteSpace(tb_max.Text)) btn_Modifier.IsEnabled = true;
		}

		private void ajout_seuil_Click(object sender, RoutedEventArgs e)
		{
			if (listModel.SelectedIndex == -1) return;

			string item = listModel.SelectedItem.ToString();
			MessageBox.Show(item);
		}

		private void ajoutSeuil_Click(object sender, RoutedEventArgs e)
		{
			if (listModel.SelectedIndex == -1) return;

			if (App.appData.choixLCD.Contains(listModel.SelectedItem.ToString()))
            {
				MessageBox.Show("Ce modèle est deja présent dans la liste", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
            }
			else App.appData.choixLCD.Add(listModel.SelectedItem.ToString());
		}

		private void remove_Click(object sender, RoutedEventArgs e)
		{
			if (choixLCD.SelectedIndex == -1) return;

			string del = choixLCD.SelectedItem.ToString();
			choixLCD.UnselectAll();
			App.appData.choixLCD.Remove(del);
		}

		private void tb_moniteur_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(tb_moniteur.Text))
			{
				btn_lcd.IsEnabled = false;
			}
			else btn_lcd.IsEnabled = true;
		}

		private void btn_lcd_Click(object sender, RoutedEventArgs e)
		{
			List<string> lcd = new List<string>();

			foreach (var item in App.appData.choixLCD.ToArray())
			{
				lcd.Add(item);
			}

			if (btn_lcd.Content.ToString() == "Ajouter")
			{
				im.SeuilAjout(tb_moniteur.Text, string.Join(";", lcd));
				App.appData.seuilAdmin.Add(new Seuil { type = tb_moniteur.Text, modele = new List<string>(lcd), seuil = "0", max = "0" });

			}
			else im.SeuilModif(tb_moniteur.Text, string.Join(";", lcd));

			tb_moniteur.Text = "";
			App.appData.choixLCD.Clear();
		}

		private void tb_max_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !char.IsDigit(e.Text.ToCharArray()[0]);
		}

		private void tb_alerte_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !char.IsDigit(e.Text.ToCharArray()[0]);
		}

		private void delete_Click(object sender, RoutedEventArgs e)
		{
			if (seuilView.SelectedIndex == -1) return;

			var data = seuilView.SelectedItem as Seuil;

			var Result = MessageBox.Show("Voulez-vous suprimer cet entrée: " + data.type + " ?", "Confirmation de supression", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (Result == MessageBoxResult.Yes)
			{
				im.RequestDelSeuilLCD(data.type, data.seuil, data.max);
			}

			seuilView.UnselectAll();
			App.appData.seuilAdmin.Remove(data);
		}

		private void StackPanel_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
				string fileName = Path.GetFileName(data[0]);
				string filePath = Path.GetFullPath(data[0]);
				string name = Path.GetFileNameWithoutExtension(data[0]).ToLower();
				string contenu = "";

				if ((fileName.ToLower().Contains("semaine ") || fileName.ToLower().Contains("shipping ")) && fileName.ToLower().Contains(".csv"))
				{
					try
					{
						contenu = File.ReadAllText(filePath);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
						return;
					}
				}

				if (fileName.ToLower() == "nip.csv")
				{
					try
					{
						contenu = File.ReadAllText(filePath);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
						return;
					}
				}

				if (fileName.ToLower() == "inventaire.csv")
				{
					try
					{
						contenu = File.ReadAllText(filePath);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
						return;
					}
				}

				if (contenu == "")
				{
					MessageBox.Show("Mauvais nom/format de fichier!", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					im.FileUpload(name, contenu);
					MessageBox.Show("Fichier Envoyé!", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
		}

        private void context_puro_Click(object sender, RoutedEventArgs e)
        {
			im.TrackingPuro();
        }
    }
}
