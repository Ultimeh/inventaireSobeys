using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
	/// <summary>
	/// Interaction logic for SendLab.xaml
	/// </summary>
	public partial class SendLab : Window
	{
		IMClient im;

		public SendLab(IMClient imc)
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

		private void btn_Lab_Click(object sender, RoutedEventArgs e)
		{
			tb_erreur.Text = "";
			App.appData.snColor = false;

			if (string.IsNullOrWhiteSpace(tb_serial.Text))
			{
				MessageBox.Show("Aucun numeros de serie." + Environment.NewLine + "Veuillez entrer un ou plusieur numeros de serie avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
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

			bool check = false;

			if (checkRapport.IsChecked == true) check = true;

			im.EnvoieLab(serial, check);

			tb_serial.Focus();

			//if (checkRapport.IsChecked == true)
			//{
			//	Rapport rapport = new Rapport();
			//	rapport.lab = true;
			//	rapport.ShowDialog();
			//}
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
