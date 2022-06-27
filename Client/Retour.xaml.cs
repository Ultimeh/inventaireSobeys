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

			if (string.IsNullOrWhiteSpace(tb_autre.Text))
            {
				MessageBox.Show("Le champ 'Emplacement' est vide." + Environment.NewLine + "Veuillez écrire un emplacement avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
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

			string data = tb_autre.Text.ToUpper();

            im.RequestRetour(serial, tb_RF.Text.ToUpper().Trim(), data);

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
