using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
	/// <summary>
	/// Interaction logic for Retour.xaml
	/// </summary>
	public partial class Retour : Window
	{
		IMClient im;

		public Retour(IMClient imc)
		{
			im = imc;
			DataContext = App.appData;
			InitializeComponent();
		}

		private void mainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void MouseClick(object sender, RoutedEventArgs e)
		{
			App.appData.snColor = false;
			this.Close();
		}

		private void radio_autre_Checked(object sender, RoutedEventArgs e)
		{
			tb_autre.IsEnabled = true;
		}

		private void radio_autre_Unchecked(object sender, RoutedEventArgs e)
		{
			tb_autre.IsEnabled = false;
			tb_autre.Text = "";
		}

		private void btn_retour_Click(object sender, RoutedEventArgs e)
		{
			if (checkDivers.IsChecked == true)
			{
				tb_erreur.Text = "";

				if (string.IsNullOrWhiteSpace(tb_serial.Text))
				{
					MessageBox.Show("Aucun numero de serie." + Environment.NewLine + "Veuillez entrer un ou plusieur numeros de serie avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				//if (string.IsNullOrWhiteSpace(tb_RF.Text))
				//{
				//	MessageBox.Show("Numero de RF de retour est obligatoire (RF, Case, INC ou Provenance pour Repair Depot).", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				//	return;
				//}

				if (radio_autre.IsChecked == true && string.IsNullOrWhiteSpace(tb_autre.Text))
				{
					MessageBox.Show("'Autre' est selectionné mais aucun emplacement inscrit" + Environment.NewLine + "Veuillez inscrire un emplacement avant d'essayer de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (cb_specialType.SelectedIndex == -1)
				{
					MessageBox.Show("Aucun Type sélectionné.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (cb_specialModel.SelectedIndex == -1)
				{
					MessageBox.Show("Aucun Model sélectionné.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (cb_specialType.SelectedItem.ToString() == "Autre" && string.IsNullOrWhiteSpace(tb_SpecialAutre.Text))
				{
					MessageBox.Show("le Type 'Autre' est selectionné mais aucun Type écrit" + Environment.NewLine + "Veuillez inscrire un Type avant d'essayer de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (cb_specialModel.SelectedItem.ToString() == "Autre" && string.IsNullOrWhiteSpace(tb_SpecialModAutre.Text))
				{
					MessageBox.Show("le Model 'Autre' est selectionné mais aucun Model écrit" + Environment.NewLine + "Veuillez inscrire un Model avant d'essayer de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				App.appData.enableAjout = false;

				string serial = tb_serial.Text.ToUpper();

				var temp = serial.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				//List<string> compare = new List<string>();
				//List<string> erreur = new List<string>();

				var result = temp.Distinct().ToArray();

				App.appData.enableAjout = false;

				serial = string.Join(Environment.NewLine, result);

				string data = "";

				if (!string.IsNullOrWhiteSpace(tb_autre.Text)) data = tb_autre.Text.ToUpper();
				if (radio_quantum.IsChecked == true) data = "QUANTUM";

				string type = "";
				string model = "";

				if (!string.IsNullOrWhiteSpace(tb_SpecialAutre.Text)) type = tb_SpecialAutre.Text;
				else type = cb_specialType.SelectedItem.ToString();

				if (!string.IsNullOrWhiteSpace(tb_SpecialModAutre.Text)) model = tb_SpecialModAutre.Text;
				else model = cb_specialModel.SelectedItem.ToString();

				//im.RetourSpecial(type, model, serial, tb_RF.Text.ToUpper().Trim(), data);


				checkDivers.IsChecked = false;
				tb_serial.Focus();

			}
			else
			{
				tb_erreur.Text = "";

				if (string.IsNullOrWhiteSpace(tb_serial.Text))
				{
					MessageBox.Show("Aucun numero de serie." + Environment.NewLine + "Veuillez entrer un ou plusieur numeros de serie avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (string.IsNullOrWhiteSpace(tb_RF.Text))
				{
					MessageBox.Show("Numero de RF de retour est obligatoire (RF, Case, INC ou Provenance pour Repair Depot).", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (radio_r2go.IsChecked == false && radio_oper.IsChecked == false && radio_autre.IsChecked == false && radio_quantum.IsChecked == false && radio_repair.IsChecked == false)
				{
					MessageBox.Show("Aucun emplacement selectionné" + Environment.NewLine + "Veuillez choisir un emplacement avant d'essayer de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (radio_autre.IsChecked == true && string.IsNullOrWhiteSpace(tb_autre.Text))
				{
					MessageBox.Show("'Autre' est selectionné mais aucun emplacement inscrit" + Environment.NewLine + "Veuillez inscrire un emplacement avant d'essayer de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (radio_repair.IsChecked == true && cb_repair.SelectedIndex == -1)
				{
					MessageBox.Show("'REPAIR DEPOT' est selectionné mais aucun choix fait." + Environment.NewLine + "Veuillez inscrire un choix avant d'essayer de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				App.appData.enableAjout = false;

				string serial = tb_serial.Text.ToUpper();

				var temp = serial.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				//List<string> compare = new List<string>();
				//List<string> erreur = new List<string>();

				var result = temp.Distinct().ToArray();

				App.appData.enableAjout = false;

				serial = string.Join(Environment.NewLine, result);

				string data = "";

				if (!string.IsNullOrWhiteSpace(tb_autre.Text)) data = tb_autre.Text.ToUpper();

				if (radio_r2go.IsChecked == true)
				{
					if (!string.IsNullOrWhiteSpace(tb_r2go.Text)) data = "R2GO " + tb_r2go.Text.ToUpper();
					else data = "R2GO";
				}

				if (radio_repair.IsChecked == true) data = "REPAIR DEPOT " + cb_repair.Text;
				if (radio_oper.IsChecked == true) data = "OPER";
				if (radio_quantum.IsChecked == true) data = "QUANTUM";

				im.RequestRetour(serial, tb_RF.Text.ToUpper().Trim(), data);

				tb_serial.Focus();
			}
		}

		private void tb_serial_TextChanged(object sender, TextChangedEventArgs e)
		{
			App.appData.snColor = false;

			var result = tb_serial.Text.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var dist = result.Distinct().ToArray();

			App.appData.countAdd = dist.Count();
		}

		private void tb_serial_Loaded(object sender, RoutedEventArgs e)
		{
			tb_serial.Focus();
		}

		private void radio_repair_Checked(object sender, RoutedEventArgs e)
		{
			cb_repair.IsEnabled = true;
		}

		private void radio_repair_Unchecked(object sender, RoutedEventArgs e)
		{
			cb_repair.IsEnabled = false;
			cb_repair.SelectedIndex = -1;
		}

		private void radio_r2go_Checked(object sender, RoutedEventArgs e)
		{
			tb_r2go.IsEnabled = true;
		}

		private void radio_r2go_Unloaded(object sender, RoutedEventArgs e)
		{
			tb_r2go.IsEnabled = false;
			tb_r2go.Text = "";
		}

		private void checkDivers_Checked(object sender, RoutedEventArgs e)
		{
			ObservableCollection<string> cbTypes;
			cbTypes = new ObservableCollection<string>(App.appData.types);
			cbTypes.Add("Autre");
			cbTypes.Remove("Tous");
			cb_specialType.ItemsSource = cbTypes;

			cb_specialType.IsEnabled = true;

			radio_oper.IsEnabled = false;
			radio_r2go.IsEnabled = false;
			radio_repair.IsEnabled = false;

			radio_quantum.IsChecked = true;
		}

		private void checkDivers_Unchecked(object sender, RoutedEventArgs e)
		{
			cb_specialType.SelectedIndex = -1;
			tb_SpecialAutre.Text = "";
			tb_SpecialAutre.IsEnabled = false;
			cb_specialType.IsEnabled = false;

			radio_oper.IsEnabled = true;
			radio_r2go.IsEnabled = true;
			radio_repair.IsEnabled = true;
		}

		private void cb_specialType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cb_specialType.SelectedIndex == -1)
			{
				cb_specialModel.IsEnabled = false;
				cb_specialModel.SelectedIndex = -1;
				return;
			}


			if (cb_specialType.SelectedIndex >= 0)
			{
				setModel();
				cb_specialModel.IsEnabled = true;
			}

			if (cb_specialType.SelectedItem.ToString() == "Autre")
			{
				tb_SpecialAutre.IsEnabled = true;
				cb_specialModel.SelectedIndex = 0;
			}
			else
			{
				if (tb_SpecialAutre.IsEnabled)
				{
					tb_SpecialAutre.IsEnabled = false;
					tb_SpecialAutre.Text = "";
				}
			}
		}

		public void setModel()
		{
			ObservableCollection<string> cbModel = new ObservableCollection<string>();
			App.appData.modelSearch = 0;
			cb_specialModel.ItemsSource = cbModel;

			if (cb_specialType.SelectedItem.ToString() != "Autre")
			{
				foreach (var item in App.appData.typesModels)
				{
					if (cb_specialType.SelectedItem.ToString() == item.type)
					{
						foreach (var model in item.modeles)
						{
							cbModel.Add(model);
						}

						break;
					}
				}

			}

			cbModel.Add("Autre");
		}

		private void cb_specialModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cb_specialModel.SelectedIndex == -1)
			{
				tb_SpecialModAutre.IsEnabled = false;
				tb_SpecialModAutre.Text = "";
				return;
			}

			if (cb_specialModel.SelectedItem.ToString() == "Autre") tb_SpecialModAutre.IsEnabled = true;
			else
			{
				tb_SpecialModAutre.IsEnabled = false;
				tb_SpecialModAutre.Text = "";
			}
		}
	}
}
