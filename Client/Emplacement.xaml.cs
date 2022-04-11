using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
	/// <summary>
	/// Interaction logic for Emplacement.xaml
	/// </summary>
	public partial class Emplacement : Window
	{
		IMClient im;

		public Emplacement(IMClient imc)
		{
			DataContext = App.appData;
			im = imc;
			InitializeComponent();
		}

		private void MouseClick(object sender, RoutedEventArgs e)
		{
			App.appData.snColor = false;
			this.Close();
		}

		private void mainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
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

		private void btn_emp_Click(object sender, RoutedEventArgs e)
		{
			tb_erreur.Text = "";

			if (string.IsNullOrWhiteSpace(tb_serial.Text))
			{
				MessageBox.Show("Aucun numeros de serie." + Environment.NewLine + "Veuillez entrer un ou plusieur numeros de serie avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
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

			if (tb_autre.Text.ToLower().Contains("lab"))
            {
				MessageBox.Show("Ne peut pas envoyer 'Au Lab' via changement d'emplacement, vous devez utiliser le bouton prévus : 'Envoyer au Lab'.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			App.appData.enableAjout = false;

			string serial = tb_serial.Text.ToUpper();
			var temp = serial.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var result = temp.Distinct().ToArray();


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

			im.changeEmp(serial, data);

			tb_serial.Focus();
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

		private void radio_r2go_Checked(object sender, RoutedEventArgs e)
		{
			tb_r2go.IsEnabled = true;
		}

		private void radio_r2go_Unchecked(object sender, RoutedEventArgs e)
		{
			tb_r2go.IsEnabled = false;
			tb_r2go.Text = "";
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
	}
}
