using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
	/// <summary>
	/// Interaction logic for Sortie.xaml
	/// </summary>
	public partial class Sortie : Window
	{
		IMClient im;

		public Sortie(IMClient imc)
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

		private void btn_Sortie_Click(object sender, RoutedEventArgs e)
		{
			tb_erreur.Text = "";

			if (string.IsNullOrWhiteSpace(tb_serial.Text))
			{
				MessageBox.Show("Aucun numero de serie." + Environment.NewLine + "Veuillez entrer un ou plusieur numeros de serie avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(tb_RF.Text))
			{
				MessageBox.Show("Numero de demande est obligatoire pour la sortie d'equipement.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(tb_magasin.Text))
			{
				MessageBox.Show("Choisir entre 'Cage' ou 'Shipping' du menu déroullant est obligatoire pour la sortie d'equipement.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(tb_serial.Text))
			{
				MessageBox.Show("Choisir entre 'Cage' ou 'Shipping' du menu déroullant est obligatoire pour la sortie d'equipement.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			App.appData.enableAjout = false;

			string serial = tb_serial.Text.ToUpper();

			var temp = serial.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var result = temp.Distinct().ToArray();

			App.appData.enableAjout = false;
			serial = string.Join(Environment.NewLine, result);

			im.sendSortie(serial, tb_RF.Text.ToUpper().Trim());
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

        private void tb_magasin_TextChanged(object sender, TextChangedEventArgs e)
        {
			if (string.IsNullOrWhiteSpace(tb_magasin.Text)) btn_valid.IsEnabled = false;
			else btn_valid.IsEnabled = true;
        }

        private async void btn_valid_Click(object sender, RoutedEventArgs e)
        {
			btn_valid.IsEnabled = false;
			await Task.Run(MagasinValid);
			btn_valid.IsEnabled = true;
        }

		private void MagasinValid()
        {
			InvPostes[] temp;
			List<string> serial = new List<string>();

			lock (IMClient.lockDB)
            {
				temp = App.appData.invPostes.ToArray();
            }

			foreach (var item in temp)
            {
				if (item.magasin == tb_magasin.Text.Trim() && item.statut == "En Stock") serial.Add(item.serial);
            }

			Application.Current.Dispatcher.Invoke(() =>
			{
				tb_serial.Text = String.Join(Environment.NewLine, serial);
			});
		}
    }
}
