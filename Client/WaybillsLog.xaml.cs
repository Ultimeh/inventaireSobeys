using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace Client
{
	/// <summary>
	/// Interaction logic for WaybillsLog.xaml
	/// </summary>
	public partial class WaybillsLog : Window
	{
		public IMClient im;
		public ScrollViewer wbScrool = new ScrollViewer();
		bool initial = true;
		string current = DateTime.Now.Year.ToString();

		public WaybillsLog(IMClient imc)
		{
			DataContext = App.appData;
			im = imc;
			InitializeComponent();
			App.appData.setWBfilter();
			wbScrool = GetDescendantByType(ListBills, typeof(ScrollViewer)) as ScrollViewer;
		}

		private void mainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void MouseClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void ListBills_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void btn_Lab_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(tb_waybills.Text) && chk_Retour.IsChecked == false)
			{
				MessageBox.Show("Aucun Waybills." + Environment.NewLine + "Veuillez entrer un ou plusieurs Waybills avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(tb_waybillsRetour.Text) && chk_Retour.IsChecked == true)
			{
				MessageBox.Show("Aucun Waybills de Retour." + Environment.NewLine + "Si Waybill de Retour est coché, Veuillez entrer un ou plusieurs Waybills avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (string.IsNullOrEmpty(tb_RF.Text) || !tb_RF.Text.ToLower().StartsWith("rf") && !tb_RF.Text.ToLower().StartsWith("inc") && !tb_RF.Text.ToLower().StartsWith("c"))
			{
				MessageBox.Show("Numero de demande (RF, Case, ou INC) est obligatoire.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			string waybills = tb_waybills.Text.ToUpper();
			string retour = tb_waybillsRetour.Text.ToUpper();
			string[] temp = waybills.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			string[] tempRetour;

			if (string.IsNullOrEmpty(waybills)) waybills = "";
			else
			{
				temp = temp.Distinct().ToArray();

				for (int x = 0; x < temp.Count(); x++)
				{
					if (temp[x].Length >= 34)
					{
						temp[x] = temp[x].Remove(0, 11);
						temp[x] = temp[x].Substring(0, temp[x].Length - 11);
					}
				}
			}

			waybills = string.Join(Environment.NewLine, temp);

			if (string.IsNullOrEmpty(retour)) retour = "";
			else
			{
				tempRetour = retour.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				tempRetour = tempRetour.Distinct().ToArray();

				for (int x = 0; x < tempRetour.Count(); x++)
				{
					if (tempRetour[x].Length >= 34)
					{
						tempRetour[x] = tempRetour[x].Remove(0, 11);
						tempRetour[x] = tempRetour[x].Substring(0, tempRetour[x].Length - 11);
					}
				}

				retour = string.Join(Environment.NewLine, tempRetour);
			}

			App.appData.enableAjout = false;

			im.SendWaybills("add", waybills, retour, tb_RF.Text.ToUpper().Trim());

			tb_RF.Text = "";
			tb_waybills.Text = "";
			tb_waybillsRetour.Text = "";
			tb_waybills.Focus();
		}

		private void copyRF_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1) return;
			if (ListBills.SelectedItem == null) return;

			Waybills item = ListBills.SelectedItem as Waybills;
			Clipboard.SetText(item.RF);
		}

		private void tb_RFsearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(tb_WBsearch.Text) && !tb_WBsearch.IsFocused) tb_WBsearch.Text = "";
			App.appData.countWB = ListBills.Items.Count;
		}

		private void tb_WBsearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			App.appData.countWB = ListBills.Items.Count;
		}

		private void puro_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1 || ListBills.SelectedItem is null) return;

			var puroNum = (ListBills.SelectedItem as Waybills).wayb;
			var result = puroNum.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			for (int i = 0; i < result.Count(); i++)
            {
				result[i] = result[i].Split(" - ")[0];
            }

			var final = String.Join(",", result);
			var link = "https://www.purolator.com/en/shipping/tracker?pins=" + final;

			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = link,
				UseShellExecute = true
			};

			Process.Start(psi);
		}

		private void tb_waybills_Loaded(object sender, RoutedEventArgs e)
		{
			tb_waybills.Focus();
		}

		private void tb_waybills_TextChanged(object sender, TextChangedEventArgs e)
		{
			var result = tb_waybills.Text.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var dist = result.Distinct().ToArray();

			App.appData.countAdd = dist.Count();
		}

		public static Visual GetDescendantByType(Visual element, Type type)
		{
			if (element == null)
			{
				return null;
			}
			if (element.GetType() == type)
			{
				return element;
			}
			Visual foundElement = null;
			if (element is FrameworkElement)
			{
				(element as FrameworkElement).ApplyTemplate();
			}
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
			{
				Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
				foundElement = GetDescendantByType(visual, type);
				if (foundElement != null)
				{
					break;
				}
			}
			return foundElement;
		}

		private void ListBills_Loaded(object sender, RoutedEventArgs e)
		{
			wbScrool.ScrollToEnd();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			im.RequestWB(current);
			App.appData.waybill = true;
			App.appData.selectedYear = current;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			App.appData.waybill = false;
			App.appData.waybills.Clear();

			tb_WBsearch.Text = "";
			tb_RFsearch.Text = "";
		}

		private void cb_year_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (initial)
			{
				initial = false;
				return;
			}

			App.appData.enableAjout = false;
			if (!string.IsNullOrEmpty(App.appData.selectedYear)) im.RequestWB(App.appData.selectedYear);
		}

		private void cb_mois_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (App.appData.selectedMois != "Tous")
			{
				App.appData.jour.Clear();
				App.appData.jour.Add("Tous");
				App.appData.selectedJour = "Tous";

				var temp = ListBills.Items;

				foreach (Waybills item in temp)
				{
					if (!App.appData.jour.Contains(item.jour)) App.appData.jour.Add(item.jour);
				}

				if (cb_jour != null)
				{
					cb_jour.IsHitTestVisible = true;
				}
			}
			else
			{
				if (cb_jour != null)
				{
					cb_jour.SelectedIndex = -1;
					cb_jour.IsHitTestVisible = false;
				}
			}

			App.appData.countWB = ListBills.Items.Count;
		}

		private void cb_jour_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			App.appData.countWB = ListBills.Items.Count;
		}

		private void chk_Retour_Checked(object sender, RoutedEventArgs e)
		{
			tb_waybillsRetour.IsEnabled = true;
		}

		private void chk_Retour_Unchecked(object sender, RoutedEventArgs e)
		{
			tb_waybillsRetour.IsEnabled = false;
			tb_waybillsRetour.Text = "";
		}

		private void puroRetour_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1 || ListBills.SelectedItem is null) return;

			var puroNum = (ListBills.SelectedItem as Waybills).wbRetour;
			var result = puroNum.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			for (int i = 0; i < result.Count(); i++)
			{
				result[i] = result[i].Split(" - ")[0];
			}

			var final = String.Join(",", result);
			var link = "https://www.purolator.com/en/shipping/tracker?pins=" + final;

			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = link,
				UseShellExecute = true
			};

			Process.Start(psi);
		}

		private void copyBills_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1) return;
			if (ListBills.SelectedItem == null) return;

			Waybills item = ListBills.SelectedItem as Waybills;
			var info = item.wayb.Split(Environment.NewLine);

			for (int i = 0; i < info.Count(); i++)
            {
				info[i] = info[i].Split(" - ")[0];
            }

			Clipboard.SetText(string.Join(Environment.NewLine, info));
		}

		private void copyRFetBills_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1) return;
			if (ListBills.SelectedItem == null) return;

			Waybills item = ListBills.SelectedItem as Waybills;
			var info = item.wayb.Split(Environment.NewLine);

			for (int i = 0; i < info.Count(); i++)
			{
				info[i] = info[i].Split(" - ")[0];
			}

			Clipboard.SetText(item.RF + Environment.NewLine + string.Join(Environment.NewLine, info));
		}

		private void copyBillsRetour_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1) return;
			if (ListBills.SelectedItem == null) return;

			Waybills item = ListBills.SelectedItem as Waybills;
			var info = item.wbRetour.Split(Environment.NewLine);

			for (int i = 0; i < info.Count(); i++)
			{
				info[i] = info[i].Split(" - ")[0];
			}

			Clipboard.SetText(string.Join(Environment.NewLine, info));
		}

		private void copyRFetBillsRetour_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1) return;
			if (ListBills.SelectedItem == null) return;

			Waybills item = ListBills.SelectedItem as Waybills;
			var info = item.wbRetour.Split(Environment.NewLine);

			for (int i = 0; i < info.Count(); i++)
			{
				info[i] = info[i].Split(" - ")[0];
			}

			Clipboard.SetText(item.RF + Environment.NewLine + string.Join(Environment.NewLine, info));
		}

		private async void btn_rapport_Click(object sender, RoutedEventArgs e)
		{
			btn_rapport.IsEnabled = false;
			await Task.Run(TaskRapport);
			btn_rapport.IsEnabled = true;
		}

		private void TaskRapport()
		{
			try
			{
				var view = ListBills.Items;

				var wb = new XLWorkbook();
				var ws = wb.Worksheets.Add("Waybills");
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

				ws.Cell(2, 1).InsertData(view);

				for (int i = 1; i < 7; i++)
				{
					ws.Cell(1, i).Style = titlesStyle;
				}

				ws.Cell(1, 1).Value = "Mois";
				ws.Cell(1, 2).Value = "Jour";
				ws.Cell(1, 3).Value = "RF";
				ws.Cell(1, 4).Value = "Waybill";
				ws.Cell(1, 5).Value = "Waybill de Retour";
				ws.Cell(1, 6).Value = "Commentaire";

				//ws.Cell(2, 1).InsertData(final);
				ws.Cells().Style.Alignment.WrapText = true;
				ws.Columns().AdjustToContents();
				//ws.Rows().AdjustToContents();
				ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

				string path = @"C:\Inventaire Entrepot Settings\Rapport";
				string date = DateTime.Now.ToString("dd-MM-yyyy H;mm;ss");

				wb.SaveAs(path + @"\Rapport WB " + date + ".xlsx");

				MessageBox.Show("Rapport complété", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Information);

				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					UseShellExecute = true,
					Arguments = path,
					FileName = "explorer.exe",
				};

				Process.Start(startInfo);

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void delete_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1) return;

			Waybills item = ListBills.SelectedItem as Waybills;

			var Result = MessageBox.Show("Voulez-vous suprimer le Waybill pour " + item.RF + " ?", "Confirmation de supression", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (Result == MessageBoxResult.Yes)
			{
				im.SendWaybills("del", item.wayb, item.wbRetour, item.RF);
			}
		}

		private void ListBills_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (App.appData.selectedYear == current) App.appData.delWB = true;
			else App.appData.delWB = false;
		}

		private void comment_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1) return;

			var data = (ListBills.SelectedItem as Waybills);

			CommentaireWB commentaire = new CommentaireWB(im, data.RF, data.wayb, data.comment);
			commentaire.ShowDialog();
		}

		private void copyRFAllBill_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1) return;
			if (ListBills.SelectedItem == null) return;

			Waybills item = ListBills.SelectedItem as Waybills;

			if (string.IsNullOrEmpty(item.wbRetour)) Clipboard.SetText(item.RF + Environment.NewLine + item.wayb);
			else if (string.IsNullOrEmpty(item.wayb)) Clipboard.SetText(item.RF + Environment.NewLine + item.wbRetour);
			else Clipboard.SetText(item.RF + Environment.NewLine + item.wayb + Environment.NewLine + Environment.NewLine + item.wbRetour);
		}

		private void copyAllBill_Click(object sender, RoutedEventArgs e)
		{
			if (ListBills.SelectedIndex == -1) return;
			if (ListBills.SelectedItem == null) return;

			Waybills item = ListBills.SelectedItem as Waybills;

			if (string.IsNullOrEmpty(item.wbRetour)) Clipboard.SetText(item.wayb);
			else if (string.IsNullOrEmpty(item.wayb)) Clipboard.SetText(item.wbRetour);
			else Clipboard.SetText(item.wayb + Environment.NewLine + Environment.NewLine + item.wbRetour);
		}

        private void ListBills_KeyUp(object sender, KeyEventArgs e)
        {
			if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				if (ListBills.SelectedIndex == -1)
				{
					e.Handled = true;
					return;
				}

				var selected = ListBills.SelectedItem as Waybills;
				Clipboard.SetText(selected.RF + Environment.NewLine + selected.wayb);
				e.Handled = true;
			}
		}

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
			var regex = new Regex(@"[^a-zA-Z0-9\s]");
			if (regex.IsMatch(e.Text))
			{
				e.Handled = true;
			}
		}

        private void livre_Click(object sender, RoutedEventArgs e)
        {
			if (ListBills.SelectedIndex == -1) return;
			if (ListBills.SelectedItem == null) return;

			Waybills item = ListBills.SelectedItem as Waybills;

			im.EnvoieComment(item.RF, item.wayb, "Livré");
		}

        private void btn_Filtre_Click(object sender, RoutedEventArgs e)
        {
			tb_RFsearch.Text = "";
			tb_WBsearch.Text = "";
			tb_RFsearch.Focus();
        }
    }
}
