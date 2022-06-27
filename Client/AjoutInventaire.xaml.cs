using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
	/// <summary>
	/// Interaction logic for AjoutInventaire.xaml
	/// </summary>
	public partial class AjoutInventaire : Window
	{
		IMClient im;

		public AjoutInventaire(IMClient imc)
		{
			DataContext = App.appData;
			im = imc;
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

		private void btn_Ajout_Click(object sender, RoutedEventArgs e)
		{
			tb_erreur.Text = "";
			App.appData.snColor = false;

			//if (cb_type.SelectedIndex == -1)
			//{
			//	MessageBox.Show("Aucun Type sélectionné." + Environment.NewLine + "Veuillez choisir un 'Type' avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
			//	return;
			//}

			if (cb_type.SelectedIndex == -1 || cb_model.SelectedIndex == -1)
            {
				MessageBox.Show("Aucun 'Type' ou 'Modèle' sélectionné." + Environment.NewLine + "Veuillez choisir un 'Type' et 'Modèle' avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(tb_serial.Text))
			{
				MessageBox.Show("Aucun numeros de serie." + Environment.NewLine + "Veuillez entrer un ou plusieur numeros de serie avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			//if (radio_repair.IsChecked == false && radio_oper.IsChecked == false && radio_emp.IsChecked == false && radio_all.IsChecked == false)
			//{
			//	MessageBox.Show("Aucun emplacement." + Environment.NewLine + "Veuillez choisir un emplacement avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
			//	return;
			//}

			if (string.IsNullOrWhiteSpace(tb_emp.Text))
			{
				MessageBox.Show("Le champ 'Emplacement' est vide." + Environment.NewLine + "Veuillez écrire un emplacement avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			////if (radio_repair.IsChecked == true && cb_repair.SelectedIndex == -1)
			////{
			////	MessageBox.Show("'REPAIR DEPOT' est selectionné mais aucun choix fait." + Environment.NewLine + "Veuillez inscrire un choix avant d'essayer de nouveau.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
			////	return;
			////}

			//string type = cb_type.Text;
			//string model = cb_model.Text;
			string serial = tb_serial.Text.ToUpper();
			string emp = tb_emp.Text.ToUpper().Trim();

			//if (radio_repair.IsChecked == true) emp = "REPAIR DEPOT " + cb_repair.Text;
			//if (radio_all.IsChecked == true) emp = "ALL";
			//if (radio_emp.IsChecked == true) emp = tb_emp.Text.ToUpper();
			//if (radio_oper.IsChecked == true) emp = "OPER";

			var temp = serial.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			//List<string> compare = new List<string>();
			//List<string> erreur = new List<string>();

			var result = temp.Distinct().ToArray();

			App.appData.enableAjout = false;

			serial = string.Join(Environment.NewLine, result);

			im.sendNew(serial, emp, cb_type.Text, cb_model.Text);

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

		private void radio_emp_Checked(object sender, RoutedEventArgs e)
		{
			if (tb_emp != null) tb_emp.IsEnabled = true;
		}

		private void radio_emp_Unchecked(object sender, RoutedEventArgs e)
		{
			tb_emp.IsEnabled = false;
			tb_emp.Text = "";
		}

        private void cb_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (cb_type.SelectedIndex == -1)
            {
				cb_model.IsEnabled = false;
				return;
            }

			cb_model.IsEnabled = true;

			if (cb_type.SelectedIndex == 0) cb_model.ItemsSource = App.appData.modelPoste;
			if (cb_type.SelectedIndex == 1) cb_model.ItemsSource = App.appData.modelPortable;
			if (cb_type.SelectedIndex == 2) cb_model.ItemsSource = App.appData.modelServeur;
		}
    }
}
