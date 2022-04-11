using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{
	/// <summary>
	/// Interaction logic for Logs.xaml
	/// </summary>
	public partial class Logs : Window
	{
		public ScrollViewer logViewScrool = new ScrollViewer();

		public Logs()
		{
			DataContext = App.appData;
			InitializeComponent();
			logViewScrool = GetDescendantByType(listLogs, typeof(ScrollViewer)) as ScrollViewer;
			App.appData.setFilterLogs();
			App.appData.logsRapport.CollectionChanged += this.OnCollectionChanged;
			App.appData.countLog = listLogs.Items.Count;
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			logViewScrool.ScrollToBottom();
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void MouseClick(object sender, RoutedEventArgs e)
		{
			App.appData.searchLog = "";
			this.Close();
		}

		private void listLogs_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
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

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			logViewScrool.ScrollToBottom();
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (listLogs.SelectedIndex == -1) return;

			List<string> list = new List<string>();

			foreach (LogsRapport item in listLogs.SelectedItems)
			{
				list.Add(item.info);
			}

			Clipboard.SetText(string.Join(Environment.NewLine, list));
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

				//var view = listLogs.Items;

				//List<LogsRapport> newLog = new List<LogsRapport>();
				//string[] temp;

				//foreach (string item in view)
				//{
				//	if (item.Contains(" - "))
				//	{
				//		temp = item.Split(" - ");

				//		if (temp.Count() == 3)
				//		{
				//			newLog.Add(new LogsRapport { date = temp[0], nom = temp[1], info = temp[2] });
				//		}
				//		else
				//		{
				//			newLog.Add(new LogsRapport { date = temp[0], nom = "Serveur", info = temp[1] });
				//		}
				//	}
				//}

				var wb = new XLWorkbook();
				var ws = wb.Worksheets.Add("Logs");
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

				ws.Cell(2, 1).InsertData(App.appData.logsRapport.ToArray());

				for (int i = 1; i < 6; i++)
				{
					ws.Cell(1, i).Style = titlesStyle;
				}

				ws.Cell(1, 1).Value = "Log";
				ws.Cell(1, 2).Value = "Date";
				ws.Cell(1, 3).Value = "Usager";
				ws.Cell(1, 4).Value = "Action";
				ws.Cell(1, 5).Value = "Valeur";

				ws.Columns().AdjustToContents();
				//ws.Rows().AdjustToContents();
				ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

				string path = @"C:\Inventaire Entrepot Settings\Rapport";
				string date = DateTime.Now.ToString("dd-MM-yyyy H;mm;ss");

				wb.SaveAs(path + @"\Rapport Logs " + date + ".xlsx");

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

		private void tb_RFsearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			App.appData.countLog = listLogs.Items.Count;
		}
	}
}
