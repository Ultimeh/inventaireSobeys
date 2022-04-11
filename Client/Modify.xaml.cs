using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Client
{
	/// <summary>
	/// Interaction logic for Modify.xaml
	/// </summary>
	public partial class Modify : Window
	{
		IMClient im;
		ObservableCollection<InvPostes> info;

		public Modify(IMClient imc, List<InvPostes> temp)
		{
			DataContext = App.appData;
			im = imc;
			info = new ObservableCollection<InvPostes>(temp);
			InitializeComponent();
			ListViewMod.ItemsSource = info;
		}

		private void ListViewData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void btnX_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void btn_modify_Click(object sender, RoutedEventArgs e)
		{
			DateTime dateValue;
			bool valid = true;

			List<string> final = new List<string>();

			string[] valeur;

			foreach (var item in info.ToArray())
			{
				if (!string.IsNullOrEmpty(item.dateSortie))
				{
					valeur = item.dateSortie.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

					foreach (var date in valeur)
                    {
						if (!DateTime.TryParse(date, out dateValue))
						{
							valid = false;
							break;
						}
					}
				}

				if (!string.IsNullOrEmpty(item.dateEntryLab))
				{
					valeur = item.dateEntryLab.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

					foreach (var date in valeur)
                    {
						if (!DateTime.TryParse(date, out dateValue))
						{
							valid = false;
							break;
						}

					}
				}

				if (!string.IsNullOrEmpty(item.dateRetour))
				{
					valeur = item.dateRetour.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

					foreach (var date in valeur)
                    {
						if (!DateTime.TryParse(date, out dateValue))
						{
							valid = false;
							break;
						}
					}
				}

				if (item.emplacement.ToLower() == "shipping") item.emplacement = "Shipping";
				else if (item.emplacement.ToLower() == "cage") item.emplacement = "Cage";
				else item.emplacement = item.emplacement.ToUpper();

				final.Add(item.serial + "╚" + item.statut + "╚" + item.RF.ToUpper() + "╚" + item.dateSortie + "╚" + item.RFretour.ToUpper() + "╚" + item.dateRetour + "╚" + item.dateEntryLab + "╚" + item.emplacement);
			}

			if (!valid)
			{
				MessageBox.Show("Une ou plusieurs dates modifiés ne sont pas dans un format valide." + Environment.NewLine + "Bon format: AAAA-MM-JJ", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			else
			{
				im.ModifyInfo(string.Join("§", final));
				this.Close();
			}
		}
	}
}
