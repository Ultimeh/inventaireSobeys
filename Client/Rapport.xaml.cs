using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Rapport : Window
	{
		public ItemCollection view;
		public bool lab = false;
		public List<InvPostes> viewList = new List<InvPostes>();

		public Rapport()
		{
			DataContext = App.appData;
			App.appData.rapportCheck = false;
			InitializeComponent();
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void MouseClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void radio_share_Checked(object sender, RoutedEventArgs e)
		{
			btn_rapport.IsEnabled = true;
			tb_nom.IsEnabled = false;
			App.appData.rapportCheck = false;
		}

		private void radio_local_Checked(object sender, RoutedEventArgs e)
		{
			btn_rapport.IsEnabled = true;
			checkLocation.IsEnabled = true;
			checkEmplacement.IsEnabled = true;
			if (checkLocation.IsChecked == true) tb_nom.IsEnabled = true;
			App.appData.rapportCheck = true;
		}

		private async void btn_retour_Click(object sender, RoutedEventArgs e)
		{
			string date = DateTime.Now.ToString("dd-MM-yyyy H;mm;ss");
			string link = "";
			string message = "";
			bool emplacement = false;

			if (radio_share.IsChecked == true)
			{
				link = @".\Rapports\Rapport " + "(" + App.appData.UserName + ") " + date + ".xlsx";
				message = "Generation du Rapport Terminer!";
			}
			else if (radio_local.IsChecked == true)
			{
				if (checkLocation.IsChecked == true && !string.IsNullOrWhiteSpace(tb_nom.Text))
				{
					link = @"C:\Inventaire Sobeys Settings\Rapport\" + tb_nom.Text + ".xlsx";
				}
				else link = @"C:\Inventaire Sobeys Settings\Rapport\Rapport " + "(" + App.appData.UserName + ") " + date + ".xlsx";

				if (checkEmplacement.IsChecked == true)
				{
					emplacement = true;
					message = "Generation du Rapport Terminer!";
				}
				else message = "Generation du Rapport Terminer!" + Environment.NewLine + @"(Emplacement local: C:\Inventaire Entrepot Settings\Rapport)";
			}

			this.Close();

			if (chk_histo.IsChecked == false) await Task.Run(() => GenerateRapport(link, message));
			else await Task.Run(() => GenerateRapportSansHisto(link, message));

			if (emplacement)
			{
				string path = @"C:\Inventaire Sobeys Settings\Rapport";

				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					UseShellExecute = true,
					Arguments = path,
					FileName = "explorer.exe",
				};

				Process.Start(startInfo);
			}
		}

		private void GenerateRapport(string link, string message)
		{
			try
			{
				var wb = new XLWorkbook();
				var ws = wb.Worksheets.Add("Inventaire");
				var titlesStyle = wb.Style;

				titlesStyle.Font.Bold = true;
				titlesStyle.Border.TopBorder = XLBorderStyleValues.Thin;
				titlesStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
				titlesStyle.Border.RightBorder = XLBorderStyleValues.Thin;
				titlesStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
				titlesStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				titlesStyle.Fill.BackgroundColor = XLColor.AppleGreen;
				titlesStyle.Protection.Locked = true;
				ws.SheetView.FreezeRows(1);

				if (viewList.Count != 0) ws.Cell(2, 1).InsertData(viewList);
				else ws.Cell(2, 1).InsertData(view);

				ws.Column(22).Delete();
				ws.Column(21).Delete();
				ws.Column(20).Delete();
				ws.Column(19).Delete();
				ws.Column(18).Delete();
				ws.Column(17).Delete();
				ws.Column(16).Delete();
				ws.Column(15).Delete();

				for (int i = 1; i < 15; i++)
				{
					ws.Cell(1, i).Style = titlesStyle;
				}

				ws.Cell(1, 1).Value = "Type";
				ws.Cell(1, 2).Value = "Modèle";
				ws.Cell(1, 3).Value = "Magasin";
				ws.Cell(1, 4).Value = "Numéro de série";
				ws.Cell(1, 5).Value = "Statut";
				ws.Cell(1, 6).Value = "Case de Sortie";
				ws.Cell(1, 7).Value = "Date de Sortie";
				ws.Cell(1, 8).Value = "Case de Retour";
				ws.Cell(1, 9).Value = "Date de Retour";
				ws.Cell(1, 10).Value = "Emplacement";
				ws.Cell(1, 11).Value = "Date d'entrée";
				ws.Cell(1, 12).Value = "Date d'envoie au Lab";
				ws.Cell(1, 13).Value = "Date de Clonage";
				ws.Cell(1, 14).Value = "Date Expiration Clonage";

				//ws.Cell(2, 1).InsertData(final);
				ws.Cells().Style.Alignment.WrapText = true;
				ws.Columns().AdjustToContents();
				//ws.Rows().AdjustToContents();
				ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

				wb.SaveAs(link);

				MessageBox.Show(message, "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void GenerateRapportSansHisto(string link, string message)
		{
			try
			{
				var temp = view.Cast<InvPostes>().ToList();

				foreach (var item in temp)
				{
					var rf = item.RF.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
					var rfRetour = item.RFretour.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
					var dateRF = item.dateSortie.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
					var dateRetour = item.dateRetour.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
					var dateClone = item.dateClone.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
					var dateEnvoieLab = item.dateEntryLab.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
					var dateCloneValid = item.dateCloneValid.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

					if (rf.Count() != 0) item.RF = rf[rf.Count() - 1];
					if (dateRF.Count() != 0) item.dateSortie = dateRF[dateRF.Count() - 1];
					if (rfRetour.Count() != 0) item.RFretour = rfRetour[rfRetour.Count() - 1];
					if (dateRetour.Count() != 0) item.dateRetour = dateRetour[dateRetour.Count() - 1];
					if (dateClone.Count() != 0) item.dateClone = dateClone[dateClone.Count() - 1];
					if (dateEnvoieLab.Count() != 0) item.dateEntryLab = dateEnvoieLab[dateEnvoieLab.Count() - 1];
					if (dateCloneValid.Count() != 0) item.dateCloneValid = dateCloneValid[dateCloneValid.Count() - 1];
				}

				var wb = new XLWorkbook();
				var ws = wb.Worksheets.Add("Inventaire");
				var titlesStyle = wb.Style;

				titlesStyle.Font.Bold = true;
				titlesStyle.Border.TopBorder = XLBorderStyleValues.Thin;
				titlesStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
				titlesStyle.Border.RightBorder = XLBorderStyleValues.Thin;
				titlesStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
				titlesStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				titlesStyle.Fill.BackgroundColor = XLColor.AppleGreen;
				titlesStyle.Protection.Locked = true;
				ws.SheetView.FreezeRows(1);

				ws.Cell(2, 1).InsertData(temp);

				ws.Column(22).Delete();
				ws.Column(21).Delete();
				ws.Column(20).Delete();
				ws.Column(19).Delete();
				ws.Column(18).Delete();
				ws.Column(17).Delete();
				ws.Column(16).Delete();
				ws.Column(15).Delete();

				for (int i = 1; i < 15; i++)
				{
					ws.Cell(1, i).Style = titlesStyle;
				}

				ws.Cell(1, 1).Value = "Type";
				ws.Cell(1, 2).Value = "Modèle";
				ws.Cell(1, 3).Value = "Magasin";
				ws.Cell(1, 4).Value = "Numéro de série";
				ws.Cell(1, 5).Value = "Statut";
				ws.Cell(1, 6).Value = "Case de Sortie";
				ws.Cell(1, 7).Value = "Date de Sortie";
				ws.Cell(1, 8).Value = "Case de Retour";
				ws.Cell(1, 9).Value = "Date de Retour";
				ws.Cell(1, 10).Value = "Emplacement";
				ws.Cell(1, 11).Value = "Date d'entrée";
				ws.Cell(1, 12).Value = "Date d'envoie au Lab";
				ws.Cell(1, 13).Value = "Date de Clonage";
				ws.Cell(1, 14).Value = "Date Expiration Clonage";

				//ws.Cell(2, 1).InsertData(final);
				ws.Cells().Style.Alignment.WrapText = true;
				ws.Columns().AdjustToContents();
				//ws.Rows().AdjustToContents();
				ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

				wb.SaveAs(link);

				MessageBox.Show(message, "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void radio_local_Unchecked(object sender, RoutedEventArgs e)
		{
			checkLocation.IsEnabled = false;
			checkEmplacement.IsEnabled = false;
		}

		private void checkLocation_Checked(object sender, RoutedEventArgs e)
		{
			tb_nom.IsEnabled = true;
		}

		private void checkLocation_Unchecked(object sender, RoutedEventArgs e)
		{
			tb_nom.IsEnabled = false;
			tb_nom.Text = "";
		}

		private void tb_nom_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			var regex = new Regex(@"[^a-zA-Z0-9\s]");
			if (regex.IsMatch(e.Text))
			{
				e.Handled = true;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (lab)
			{
				chk_histo.IsEnabled = false;
				radio_share.IsEnabled = false;
				radio_local.IsChecked = true;
				checkEmplacement.IsChecked = true;
				checkLocation.IsChecked = true;
			}
		}
	}
}
