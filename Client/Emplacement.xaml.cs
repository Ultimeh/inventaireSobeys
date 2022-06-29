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

            if (string.IsNullOrWhiteSpace(tb_autre.Text))
            {
				MessageBox.Show("Aucun emplacement indiqué, vous devez entrer un emplacement.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
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
			im.changeEmp(serial, tb_autre.Text.ToUpper());
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
	}
}
