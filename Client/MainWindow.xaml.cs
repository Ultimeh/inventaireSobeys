using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		IMClient im = new IMClient();
		ManageOptions manageOptions = new ManageOptions();
		public ScrollViewer mainViewScrool = new ScrollViewer();
		public static bool sortType = true;
		public static bool sortModel = true;
		public static bool sortEmp = true;
		public static bool sortValid = true;
		public bool checkSeuil = false;
		private static readonly Object lockChange = new Object();

		ObservableCollection<InvPostes> stock = new ObservableCollection<InvPostes>();
		ObservableCollection<InvPostes> sortie = new ObservableCollection<InvPostes>();
		ObservableCollection<string> erreur = new ObservableCollection<string>(); 
		//public string ID = "0";

		public MainWindow()
		{
			DataContext = App.appData;
			manageOptions.loadOptions();
			InitializeComponent();
			mainViewScrool = GetDescendantByType(ListViewData, typeof(ScrollViewer)) as ScrollViewer;
			App.appData.setMain(this);
			im.setMain(this);
			
			im.Disconnected += new EventHandler(im_Disconnected);

			callLogin();

			Task.Run(CheckUpdate);
		}

		public void btnDisconnect_Click(object sender, RoutedEventArgs e)
		{
			App.appData.netChange = false;
			App.appData.enableEntrepot = false;
			App.appData.enableMain = false;
			ListViewData.ItemsSource = null;
			App.appData.privilege = "Aucun";
			btn_Filtre_Click(null, null);
			tb_labSN.Text = "";
			tb_labMod.Text = "";

			if (App.appData.enableEntrepot || App.appData.enableLab) App.appData.enableAjout = true;

			im.Disconnect();
		}

		void im_Disconnected(object sender, EventArgs e)
		{
			//methode qui reset les variable si le user se déconnecte (log out au lieu de quitter)

			if (App.appData.appQuit) return;

			try
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					App.appData.firstRun = true;
					App.appData.loginList = true;

					App.appData.modeleMoniteur.Clear();

					foreach (Window window in Application.Current.Windows)
					{
						if (window.Name != "mainWindow" && window.Name != "login") window.Close();
						if (window.GetType() == typeof(Logs)) window.Close();
					}

					foreach (Window window in Application.Current.Windows)
					{
						if (window.Name == "login") return;
					}

					if (mainWindow.Visibility == Visibility.Visible) mainWindow.Visibility = Visibility.Hidden;
					callLogin();
				});

			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem when client was disconnecting from the server (manual log off, user kick, connection issues, etc)" + Environment.NewLine + ex.ToString());
			}
		}

		private void callLogin()
		{
			if (!App.appData.quit)
			{
				var loginForm = new Login(im);
				loginForm.ShowDialog();
			}

			if (App.appData.quit) this.mainWindow.Close();
			else mainWindow.Visibility = Visibility.Visible;

			if (!App.appData.quit)
			{
				manageOptions.saveOption();
				App.appData.menuLogOut = "_Se déconnecter " + "(" + App.appData.IPAddress + ")";

				im.requestDatabase();
				im.requestModele();
				im.ModeleMoniteurRequest();
				im.RequestLogs();
				im.Years();
				im.WBdate();
				im.NetChangeON();
			}
		}

		private void mainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void maximize_Click(object sender, RoutedEventArgs e)
		{
			if (this.WindowState == WindowState.Normal)
			{
				this.WindowStyle = WindowStyle.SingleBorderWindow;
				this.WindowState = WindowState.Maximized;
			}
			else if (this.WindowState == WindowState.Maximized)
			{
				this.WindowState = WindowState.Normal;
				this.WindowStyle = WindowStyle.None;
			}
		}

		private void minimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowStyle = WindowStyle.SingleBorderWindow;
			this.WindowState = WindowState.Minimized;
		}

		private void btnX_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void changePassword_Click(object sender, RoutedEventArgs e)
		{
			var window = new ChangePassword(im);
			window.Owner = this;
			window.ShowDialog();
		}

		private void About_Window(object sender, RoutedEventArgs e)
		{
			var window = new About();
			window.ShowDialog();
		}

		private void btn_Ajout_Click(object sender, RoutedEventArgs e)
		{
			App.appData.countAdd = 0;
			AjoutInventaire ajoutInventaire = new AjoutInventaire(im);
			ajoutInventaire.ShowDialog();
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			App.appData.userList.Clear();
			App.appData.backup.Clear();
			im.requestUserList();
			im.requestBackupList();

			Admin admin = new Admin(im);
			admin.ShowDialog();
		}

		private void ListViewData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void btn_Sortie_Click(object sender, RoutedEventArgs e)
		{
			App.appData.countAdd = 0;
			Sortie sortie = new Sortie(im);
			sortie.ShowDialog();
		}

		private void cb_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (tb_RFsearch == null || tb_serialSearch == null || tb_dateClone == null) return;

			if (!string.IsNullOrWhiteSpace(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrWhiteSpace(tb_serialSearch.Text) && !tb_serialSearch.IsFocused) tb_serialSearch.Text = "";
			if (!string.IsNullOrWhiteSpace(tb_dateClone.Text) && !tb_dateClone.IsFocused) tb_dateClone.Text = "";
			if (!string.IsNullOrWhiteSpace(tb_dateAjout.Text) && !tb_dateAjout.IsFocused) tb_dateAjout.Text = "";

			if (App.appData.typeSearch == 0)
			{
				App.appData.modelSearch = -1;
				if (this.IsInitialized) cb_model.IsEnabled = false;
			}

			if (App.appData.typeSearch > 0)
			{
				setModel();
				cb_model.IsEnabled = true;
			}

			//if (btn_Filtre != null)
			//{
			//	if (cb_type.SelectedIndex != 0) btn_Filtre.IsEnabled = true;
			//	else if (string.IsNullOrEmpty(tb_serialSearch.Text) && cb_statut.SelectedIndex == 0 && cb_type.SelectedIndex == 0 && string.IsNullOrEmpty(tb_empSearch.Text) && string.IsNullOrEmpty(tb_dateSortie.Text)) btn_Filtre.IsEnabled = false;
			//}

			App.appData.count = App.appData.invPostesView.Count;
		}

		private void cb_model_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			App.appData.count = App.appData.invPostesView.Count;
		}

		private void cb_statut_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (tb_RFsearch == null || tb_serialSearch == null || tb_dateClone == null) return;

			if (!string.IsNullOrWhiteSpace(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrWhiteSpace(tb_serialSearch.Text) && !tb_serialSearch.IsFocused) tb_serialSearch.Text = "";
			if (!string.IsNullOrWhiteSpace(tb_dateClone.Text) && !tb_dateClone.IsFocused) tb_dateClone.Text = "";
			if (!string.IsNullOrWhiteSpace(tb_dateAjout.Text) && !tb_dateAjout.IsFocused) tb_dateAjout.Text = "";
			
			//if (!string.IsNullOrWhiteSpace(tb_dateSortie.Text) && !tb_dateSortie.IsFocused) tb_dateSortie.Text = "";

			//if (btn_Filtre != null)
			//{
			//	if (cb_statut.SelectedIndex != 0) btn_Filtre.IsEnabled = true;
			//	else if (string.IsNullOrEmpty(tb_serialSearch.Text) && cb_statut.SelectedIndex == 0 && cb_type.SelectedIndex == 0 && string.IsNullOrEmpty(tb_empSearch.Text) && string.IsNullOrEmpty(tb_dateSortie.Text)) btn_Filtre.IsEnabled = false;
			//}

			App.appData.count = App.appData.invPostesView.Count;
		}

		public void setModel()
		{
			App.appData.modelSearchCB.Clear();
			App.appData.modelSearchCB.Add("Tous");
			App.appData.modelSearch = 0;

			foreach (var item in App.appData.typesModels.ToArray())
			{
				if (App.appData.selectedType == item.type)
				{
					foreach (var model in item.modeles)
					{
						App.appData.modelSearchCB.Add(model);
					}

					break;
				}
			}
		}

		public void setModelValid()
		{
			App.appData.validModel.Clear();

			foreach (var item in App.appData.typesModels.ToArray())
			{
				if (cb_specialType.SelectedItem.ToString() == item.type)
				{
					foreach (var model in item.modeles)
					{
						App.appData.validModel.Add(model);
					}

					break;
				}
			}
		}

		private void btn_emplacement_Click(object sender, RoutedEventArgs e)
		{
			App.appData.countAdd = 0;
			Emplacement emplacement = new Emplacement(im);
			emplacement.ShowDialog();
		}

		private void tabControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (App.appData.privilege == "Aucun") tabControl.SelectedIndex = -1;
		}

		private void btn_Retour_Click(object sender, RoutedEventArgs e)
		{
			App.appData.countAdd = 0;
			Retour retour = new Retour(im);
			retour.ShowDialog();
		}

		private void btn_Filtre_Click(object sender, RoutedEventArgs e)
		{
			tb_serialSearch.Text = "";
			tb_dateSortie.Text = "";
			tb_RFsearch.Text = "";
			tb_dateClone.Text = "";
			tb_empSearch.Text = "";
			tb_dateAjout.Text = "";
			cb_type.SelectedIndex = 0;
			cb_statut.SelectedIndex = 0;
			//btn_Filtre.IsEnabled = false;
		}

		private void btn_Lab_Click(object sender, RoutedEventArgs e)
		{
			App.appData.countAdd = 0;
			SendLab sendLab = new SendLab(im);
			sendLab.ShowDialog();
		}

		private void copyRF_Click(object sender, RoutedEventArgs e)
		{
			if (ListViewData.SelectedIndex == -1) return;

			List<string> list = new List<string>();
			var temp = ListViewData.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.RF);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

		private void copyRFv_Click(object sender, RoutedEventArgs e)
		{
			if (ViewValidStock.SelectedIndex == -1) return;

			List<string> list = new List<string>();
			var temp = ViewValidStock.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.RF);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

		private void copyRFretour_Click(object sender, RoutedEventArgs e)
		{
			if (ListViewData.SelectedIndex == -1) return;
			List<string> list = new List<string>();
			var temp = ListViewData.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.RFretour);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

		private void copyRFretourV_Click(object sender, RoutedEventArgs e)
		{
			if (ViewValidStock.SelectedIndex == -1) return;

			List<string> list = new List<string>();
			var temp = ViewValidStock.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.RFretour);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

		private void copySN_Click(object sender, RoutedEventArgs e)
		{
			if (ListViewData.SelectedIndex == -1) return;
			List<string> list = new List<string>();
			var temp = ListViewData.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.serial);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

		private void copySNv_Click(object sender, RoutedEventArgs e)
		{
			if (ViewValidStock.SelectedIndex == -1) return;
			List<string> list = new List<string>();
			var temp = ViewValidStock.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.serial);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

		private void tb_RFsearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (cb_type.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_RFsearch.Text)) cb_type.SelectedIndex = 0;
			if (cb_statut.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_RFsearch.Text)) cb_statut.SelectedIndex = 0;
			if (!string.IsNullOrEmpty(tb_dateSortie.Text) && !tb_dateSortie.IsFocused) tb_dateSortie.Text = "";
			if (!string.IsNullOrEmpty(tb_serialSearch.Text) && !tb_serialSearch.IsFocused) tb_serialSearch.Text = "";
			if (!string.IsNullOrWhiteSpace(tb_dateClone.Text) && !tb_dateClone.IsFocused) tb_dateClone.Text = "";
			if (!string.IsNullOrEmpty(tb_empSearch.Text) && !tb_empSearch.IsFocused) tb_empSearch.Text = "";
			if (!string.IsNullOrEmpty(tb_dateAjout.Text) && !tb_dateAjout.IsFocused) tb_dateAjout.Text = "";

			//if (!string.IsNullOrEmpty(tb_RFsearch.Text)) btn_Filtre.IsEnabled = true;
			//else btn_Filtre.IsEnabled = false;

			App.appData.count = App.appData.invPostesView.Count;
		}

		private void tb_serialSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrWhiteSpace(tb_dateClone.Text) && !tb_dateClone.IsFocused) tb_dateClone.Text = "";
			if (!string.IsNullOrEmpty(tb_dateSortie.Text) && !tb_dateSortie.IsFocused) tb_dateSortie.Text = "";
			if (!string.IsNullOrEmpty(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrEmpty(tb_empSearch.Text) && !tb_empSearch.IsFocused) tb_empSearch.Text = "";
			if (!string.IsNullOrEmpty(tb_dateAjout.Text) && !tb_dateAjout.IsFocused) tb_dateAjout.Text = "";

			if (cb_type.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_serialSearch.Text)) cb_type.SelectedIndex = 0;
			if (cb_statut.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_serialSearch.Text)) cb_statut.SelectedIndex = 0;

			//if (!string.IsNullOrEmpty(tb_serialSearch.Text)) btn_Filtre.IsEnabled = true;
			//else btn_Filtre.IsEnabled = false;

			//if (btn_Filtre != null)
			//{
			//	if (!string.IsNullOrEmpty(tb_serialSearch.Text)) btn_Filtre.IsEnabled = true;
			//	else if (string.IsNullOrEmpty(tb_serialSearch.Text) && cb_statut.SelectedIndex == 0 && cb_type.SelectedIndex == 0) btn_Filtre.IsEnabled = false;
			//}

			int caretPostion = tb_serialSearch.CaretIndex;
			tb_serialSearch.Text = tb_serialSearch.Text.Replace(" ", string.Empty);
			tb_serialSearch.Select(caretPostion, 0);

			App.appData.count = App.appData.invPostesView.Count;
		}

		private void btn_LabClone_Click(object sender, RoutedEventArgs e)
		{
			App.appData.snColor = false;

			if (string.IsNullOrWhiteSpace(tb_serialLab.Text))
			{
				MessageBox.Show("Aucun numeros de serie." + Environment.NewLine + "Veuillez entrer un ou plusieur numeros de serie avant de continuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			string serial = tb_serialLab.Text.ToUpper();

			var temp = serial.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			var result = temp.Distinct().ToArray();

			btn_LabClone.IsEnabled = false;
			serial = string.Join(Environment.NewLine, result);

			im.RequestConfirmClone(serial);

			tb_serialLab.Focus();
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

		private void ListLab_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void tb_dateClone_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrEmpty(tb_empSearch.Text) && !tb_empSearch.IsFocused) tb_empSearch.Text = "";

			if (cb_type.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_dateClone.Text)) cb_type.SelectedIndex = 0;
			if (cb_statut.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_dateClone.Text)) cb_statut.SelectedIndex = 0;
			if (!string.IsNullOrEmpty(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrEmpty(tb_dateSortie.Text) && !tb_dateSortie.IsFocused) tb_dateSortie.Text = "";
			if (!string.IsNullOrEmpty(tb_serialSearch.Text) && !tb_serialSearch.IsFocused) tb_serialSearch.Text = "";

			//if (!string.IsNullOrEmpty(tb_serialSearch.Text)) btn_Filtre.IsEnabled = true;
			//if (!string.IsNullOrEmpty(tb_dateAjout.Text) && !tb_dateAjout.IsFocused) tb_dateAjout.Text = "";
			//else btn_Filtre.IsEnabled = false;

			//if (btn_Filtre != null)
			//{
			//	if (!string.IsNullOrEmpty(tb_dateClone.Text)) btn_Filtre.IsEnabled = true;
			//	else if (string.IsNullOrEmpty(tb_dateClone.Text) && cb_statut.SelectedIndex == 0 && cb_type.SelectedIndex == 0) btn_Filtre.IsEnabled = false;
			//}

			App.appData.count = App.appData.invPostesView.Count;
		}

		private void ctx_Rapport_Click(object sender, RoutedEventArgs e)
		{
			Rapport rapport = new Rapport();
			rapport.view = ListViewData.Items;
			rapport.ShowDialog();
		}

		private void btn_way_Click(object sender, RoutedEventArgs e)
		{
			//im.RequestWB(DateTime.Now.Year.ToString());
			App.appData.countAdd = 0;
			WaybillsLog waybillsLog = new WaybillsLog(im);
			waybillsLog.ShowDialog();
		}

		private void tb_dateSortie_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			//if (!string.IsNullOrEmpty(tb_empSearch.Text) && !tb_empSearch.IsFocused) tb_empSearch.Text = "";

			//if (cb_type.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_dateSortie.Text)) cb_type.SelectedIndex = 0;
			//if (cb_statut.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_dateSortie.Text)) cb_statut.SelectedIndex = 0;
			if (!string.IsNullOrEmpty(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrEmpty(tb_dateClone.Text) && !tb_dateClone.IsFocused) tb_dateClone.Text = "";
			if (!string.IsNullOrEmpty(tb_dateAjout.Text) && !tb_dateAjout.IsFocused) tb_dateAjout.Text = "";
			if (!string.IsNullOrEmpty(tb_serialSearch.Text) && !tb_serialSearch.IsFocused) tb_serialSearch.Text = "";

			//if (!string.IsNullOrEmpty(tb_serialSearch.Text)) btn_Filtre.IsEnabled = true;
			//else btn_Filtre.IsEnabled = false;

			//if (btn_Filtre != null)
			//{
			//	if (!string.IsNullOrEmpty(tb_dateSortie.Text)) btn_Filtre.IsEnabled = true;
			//	else if (string.IsNullOrEmpty(tb_dateSortie.Text) && cb_statut.SelectedIndex == 0 && cb_type.SelectedIndex == 0) btn_Filtre.IsEnabled = false;
			//}

			App.appData.count = App.appData.invPostesView.Count;
		}

		private void tb_serialLab_TextChanged(object sender, TextChangedEventArgs e)
		{
			App.appData.snColor = false;

			var result = tb_serialLab.Text.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var dist = result.Distinct().ToArray();

			App.appData.countAddLab = dist.Count();
		}

		private void delete_Click(object sender, RoutedEventArgs e)
		{
			if (ListViewData.SelectedIndex == -1) return;

			List<string> list = new List<string>();
			var temp = ListViewData.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.serial);
			}

			string data = string.Join(",", list);
			var Result = MessageBox.Show("Voulez-vous suprimer ces Numéro de séries: " + data + " ?", "Confirmation de supression", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (Result == MessageBoxResult.Yes)
			{
				im.RequestDeleteMain(data);
			}
		}

		private void tb_serialLab_Loaded(object sender, RoutedEventArgs e)
		{
			tb_serialLab.Focus();
		}

		private void copyAll_Click(object sender, RoutedEventArgs e)
		{
			if (ListViewData.SelectedIndex == -1) return;

			var temp = ListViewData.SelectedItems;
			List<string> list = new List<string>();

			foreach (InvPostes item in temp)
			{
				list.Add(item.type + " " + item.model + " " + item.asset + " " + item.serial + " " + item.statut + " " + item.RF + " " + item.dateSortie + " " + item.RFretour + " " + item.dateRetour + " " + item.emplacement + " " + item.dateEntry + " " + item.dateEntryLab + " " + item.dateClone + " " + item.dateCloneValid);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

		private void copyAllV_Click(object sender, RoutedEventArgs e)
		{
			if (ViewValidStock.SelectedIndex == -1) return;

			var temp = ViewValidStock.SelectedItems;
			List<string> list = new List<string>();

			foreach (InvPostes item in temp)
			{
				list.Add(item.type + " " + item.model + " " + item.asset + " " + item.serial + " " + item.statut + " " + item.RF + " " + item.dateSortie + " " + item.RFretour + " " + item.dateRetour + " " + item.emplacement + " " + item.dateEntry + " " + item.dateEntryLab + " " + item.dateClone + " " + item.dateCloneValid);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

		private void tb_empSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrWhiteSpace(tb_dateClone.Text) && !tb_dateClone.IsFocused) tb_dateClone.Text = "";
			//if (!string.IsNullOrEmpty(tb_dateSortie.Text) && !tb_dateSortie.IsFocused) tb_dateSortie.Text = "";
			if (!string.IsNullOrEmpty(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrEmpty(tb_serialSearch.Text) && !tb_serialSearch.IsFocused) tb_serialSearch.Text = "";
			if (!string.IsNullOrEmpty(tb_dateAjout.Text) && !tb_dateAjout.IsFocused) tb_dateAjout.Text = "";
			if (cb_type.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_serialSearch.Text)) cb_type.SelectedIndex = 0;
			if (cb_statut.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_serialSearch.Text)) cb_statut.SelectedIndex = 0;

			//if (!string.IsNullOrEmpty(tb_empSearch.Text)) btn_Filtre.IsEnabled = true;
			//else btn_Filtre.IsEnabled = false;

			//if (btn_Filtre != null)
			//{
			//	if (!string.IsNullOrEmpty(tb_empSearch.Text)) btn_Filtre.IsEnabled = true;
			//	else if (string.IsNullOrEmpty(tb_empSearch.Text) && cb_statut.SelectedIndex == 0 && cb_type.SelectedIndex == 0) btn_Filtre.IsEnabled = false;
			//}

			App.appData.count = App.appData.invPostesView.Count;
		}

		private void Logs_Window(object sender, RoutedEventArgs e)
		{
			Logs logs = new Logs();
			logs.Show();
		}

		private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			foreach (Window window in Application.Current.Windows)
			{
				if (window.GetType() != typeof(MainWindow)) window.Close();
			}

			manageOptions.saveOption();
		}

		private void copySNlab_Click(object sender, RoutedEventArgs e)
		{
			if (ListLab.SelectedIndex == -1) return;
			List<string> list = new List<string>();
			var temp = ListLab.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.serial);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

		private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)   //sort by file size in fileClean on header click
		{
			//if (!App.appData.enableMain) return;
			if (e.OriginalSource.ToString().Contains(" Type"))
			{
				if (!sortType)
				{
					sortType = true;
					App.appData.sortMain.CustomSort = new CustomerSorter(sortType, "type");
					return;

				}
				if (sortType)
				{
					sortType = false;
					App.appData.sortMain.CustomSort = new CustomerSorter(sortType, "type");
					return;
				}
			}

			if (e.OriginalSource.ToString().Contains(" Mod"))
			{
				if (!sortModel)
				{
					sortModel = true;
					App.appData.sortMain.CustomSort = new CustomerSorter(sortModel, "model");
					return;

				}
				if (sortModel)
				{
					sortModel = false;
					App.appData.sortMain.CustomSort = new CustomerSorter(sortModel, "model");
					return;
				}
			}

			if (e.OriginalSource.ToString().Contains(" Empl"))
			{
				if (!sortEmp)
				{
					sortEmp = true;
					App.appData.sortMain.CustomSort = new CustomerSorter(sortEmp, "emp");
					return;

				}
				if (sortEmp)
				{
					sortEmp = false;
					App.appData.sortMain.CustomSort = new CustomerSorter(sortEmp, "emp");
					return;
				}
			}

			if (e.OriginalSource.ToString().Contains(" Expiration"))
			{
				if (!sortValid)
				{
					sortValid = true;
					App.appData.sortMain.CustomSort = new CustomerSorter(sortValid, "valid");
					return;

				}
				if (sortValid)
				{
					sortValid = false;
					App.appData.sortMain.CustomSort = new CustomerSorter(sortValid, "valid");
					return;
				}
			}
		}

		private void CheckUpdate()
		{
			AutoResetEvent autoResetEvent = new AutoResetEvent(false);

			while (true)
			{
				try
				{
					string version = "";
					int counter = 0;

					foreach (string line in File.ReadLines(@".\ChangeLog.txt"))
					{
						counter++;

						if (counter == 2)
						{
							version = line;
							break;
						}
					}

					if (!version.Contains(App.appData.version))
					{
						MessageBox.Show("Une nouvelle version est disponible." + Environment.NewLine + "Fermez l'application au complet et attendre quelques minutes avant de ré-ouvrir pour que la mise a jour automatique puisse s'effectuer.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Information);
						break;
					}
				}
				catch { }

				autoResetEvent.WaitOne(30000);
			}
		}

		private void modify_Click(object sender, RoutedEventArgs e)
		{
			if (ListViewData.SelectedIndex == -1) return;

			List<InvPostes> temp = new List<InvPostes>();
			string[] result;

			foreach (InvPostes item in ListViewData.SelectedItems)
			{
				result = new[] { item.type, item.model, item.asset, item.serial, item.statut, item.RF, item.RFretour, item.emplacement, item.dateEntry, item.dateSortie, item.dateRetour, item.dateEntryLab, item.dateClone, item.dateCloneValid };
				temp.Add(new InvPostes { type = result[0], model = result[1], asset = result[2], serial = result[3], statut = result[4], RF = result[5], RFretour = result[6], emplacement = result[7], dateEntry = result[8], dateSortie = result[9], dateRetour = result[10], dateEntryLab = result[11], dateClone = result[12], dateCloneValid = result[13] });
			}

			Modify modify = new Modify(im, temp);
			modify.ShowDialog();
		}

		private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !char.IsDigit(e.Text.ToCharArray()[0]);
		}

		private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			maximize_Click(null, null);
		}

		private void tb_labSN_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (tb_labSN.IsFocused) tb_labMod.Text = "";
			App.appData.countLab = ListLab.Items.Count;
		}

		private void tb_labMod_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (tb_labMod.IsFocused) tb_labSN.Text = "";
			App.appData.countLab = ListLab.Items.Count;
		}

		private void tb_dateAjout_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrEmpty(tb_empSearch.Text) && !tb_empSearch.IsFocused) tb_empSearch.Text = "";

			if (cb_type.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_dateAjout.Text)) cb_type.SelectedIndex = 0;
			if (cb_statut.SelectedIndex != 0 && !string.IsNullOrEmpty(tb_dateAjout.Text)) cb_statut.SelectedIndex = 0;
			if (!string.IsNullOrEmpty(tb_RFsearch.Text) && !tb_RFsearch.IsFocused) tb_RFsearch.Text = "";
			if (!string.IsNullOrEmpty(tb_dateClone.Text) && !tb_dateClone.IsFocused) tb_dateClone.Text = "";
			if (!string.IsNullOrEmpty(tb_dateSortie.Text) && !tb_dateSortie.IsFocused) tb_dateSortie.Text = "";
			if (!string.IsNullOrEmpty(tb_serialSearch.Text) && !tb_serialSearch.IsFocused) tb_serialSearch.Text = "";

			//if (!string.IsNullOrEmpty(tb_serialSearch.Text)) btn_Filtre.IsEnabled = true;
			//else btn_Filtre.IsEnabled = false;

			//if (btn_Filtre != null)
			//{
			//	if (!string.IsNullOrEmpty(tb_dateAjout.Text)) btn_Filtre.IsEnabled = true;
			//	else if (string.IsNullOrEmpty(tb_dateAjout.Text) && cb_statut.SelectedIndex == 0 && cb_type.SelectedIndex == 0) btn_Filtre.IsEnabled = false;
			//}

			App.appData.count = App.appData.invPostesView.Count;
		}

		private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//if (!App.appData.isAdmin) return;
			if (ListViewData.SelectedIndex == -1) return;
			if (ListViewData.SelectedItems.Count != 1) return;

			var selected = ListViewData.SelectedItem as InvPostes;

			InfoDetail infoDetail = new InfoDetail(selected, im);
			infoDetail.ShowDialog();
		}

        private void ListViewData_KeyUp(object sender, KeyEventArgs e)
        {
			if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				if (ListViewData.SelectedIndex == -1) 
                {
					e.Handled = true;
					return;
				}

				List<string> serial = new List<string>();

				foreach (var item in ListViewData.SelectedItems)
				{
					serial.Add((item as InvPostes).serial);
				}

				Clipboard.SetText(String.Join(Environment.NewLine, serial));
				e.Handled = true;
			}
        }

        private void ctx_Aide(object sender, RoutedEventArgs e)
        {
			try
			{
				File.Copy(@".\Documentation.docx", @"C:\Inventaire Entrepot Settings\Documentation.docx", true);

				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					UseShellExecute = true,
					FileName = @"C:\Inventaire Entrepot Settings\Documentation.docx",
				};

				Process.Start(startInfo);
			}
			catch { }
		}

        private void btn_Valid_Click(object sender, RoutedEventArgs e)
        {
			if (chk_special.IsChecked == true && listValid.Items.Count == 0)
            {
				MessageBox.Show("'Recherche special' est coché main aucun modèle(s) sélectionné." + Environment.NewLine + "Veuillez sélectionner un ou plusieurs modèle(s) ou décocher 'Recherche special'.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			ViewValidStock.ItemsSource = null;
			ViewValidSortie.ItemsSource = null;
			listErreur.ItemsSource = null;

			stock.Clear();
			sortie.Clear();
			erreur.Clear();

			if (chk_special.IsChecked == false) rechercheNormal();
			else rechercheSecial();
		}

		private async void rechercheNormal()
        {
			btn_Valid.IsEnabled = false;
			tb_valid.IsEnabled = false;

			List<InvPostes> temp = new List<InvPostes>();
			bool found = false;
			var listSerial = tb_valid.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			await Task.Run(() =>
			{
				foreach (var serial in listSerial)
				{
					found = false;

					foreach (var item in App.appData.invPostes.ToArray())
					{
						if (item.serial == serial)
						{
							temp.Add(item);
							found = true;
							break;
						}
					}

					if (!found)
                    {
						Application.Current.Dispatcher.Invoke(() =>
						{
							erreur.Add(serial);
						});
					}
				}
			});

			stock = new ObservableCollection<InvPostes>(temp);

			temp.Clear();

			ViewValidStock.ItemsSource = stock;
			ViewValidSortie.ItemsSource = sortie;
			listErreur.ItemsSource = erreur;

			btn_Valid.IsEnabled = true;
			tb_valid.IsEnabled = true;
			lbl_countTrouver.Content = ViewValidStock.Items.Count;
			lbl_countPasTrouver.Content = ViewValidSortie.Items.Count;
			lbl_countValidPasTrouver.Content = listErreur.Items.Count;
		}

		private async void rechercheSecial()
        {
			btn_Valid.IsEnabled = false;
			tb_valid.IsEnabled = false;

			List<InvPostes> temp = new List<InvPostes>();
			bool found = false;
			var listSerial = tb_valid.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			await Task.Run(() =>
			{
				foreach (var model in App.appData.validList.ToArray())
				{
					foreach (var item in App.appData.invPostes.ToArray())
					{
						if (item.model == model)
						{
							temp.Add(item);
						}
					}
				}
			});

			foreach (var serial in listSerial)
			{
				found = false;

				foreach (var item in temp.ToArray())
				{
					if (serial == item.serial)
					{
						stock.Add(item);
						temp.Remove(item);
						found = true;
						break;
					}
				}

				if (!found) erreur.Add(serial);
			}

			foreach (var item in temp.ToArray())
			{
				if (item.statut != "Sortie" && !item.emplacement.ToLower().Contains("repair")) sortie.Add(item);
			}

			temp.Clear();

			ViewValidStock.ItemsSource = stock;
			ViewValidSortie.ItemsSource = sortie;
			listErreur.ItemsSource = erreur;

			btn_Valid.IsEnabled = true;
			tb_valid.IsEnabled = true;

			lbl_countTrouver.Content = ViewValidStock.Items.Count;
			lbl_countPasTrouver.Content = ViewValidSortie.Items.Count;
			lbl_countValidPasTrouver.Content = listErreur.Items.Count;
		}

        private void listModel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
			e.Handled = true;
        }

        private void cb_specialType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (cb_specialType.SelectedIndex == -1)
            {
				if (cb_specialModel.IsLoaded) 
                {
					cb_specialModel.SelectedIndex = -1;
					cb_specialModel.IsEnabled = false;
				}
            }
			else
			{
				setModelValid();
				cb_specialModel.IsEnabled = true;

				foreach (var item in App.appData.validList.ToArray())
                {
					if (App.appData.validModel.Contains(item)) App.appData.validModel.Remove(item);
                }
			}
		}

        private void cb_specialModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (cb_specialModel.SelectedIndex == -1) btn_addValid.IsEnabled = false;
			else btn_addValid.IsEnabled = true;
        }

        private void btn_addValid_Click(object sender, RoutedEventArgs e)
        {
			string text = cb_specialModel.Text;
			App.appData.validList.Add(text);
			App.appData.validModel.Remove(text);
        }

        private void btn_validClear_Click(object sender, RoutedEventArgs e)
        {
			cb_specialType.SelectedIndex = -1;
			App.appData.validList.Clear();
        }

        private void ctx_enleverModel_Click(object sender, RoutedEventArgs e)
        {
			if (listValid.SelectedIndex == -1) return;

			string selected = listValid.SelectedItem.ToString();

			App.appData.validList.Remove(selected);
        }

        private void chk_special_Checked(object sender, RoutedEventArgs e)
        {
			cb_specialType.IsEnabled = true;
			ViewValidSortie.IsEnabled = true;
			btn_validClear.IsEnabled = true;
			btn_addValid.IsEnabled = true;
		}

        private void chk_special_Unchecked(object sender, RoutedEventArgs e)
        {
			btn_validClear_Click(null, null);
			cb_specialType.IsEnabled = false;
			ViewValidSortie.IsEnabled = false;
			btn_validClear.IsEnabled = false;
			btn_addValid.IsEnabled = false;
		}

        private void tb_valid_TextChanged(object sender, TextChangedEventArgs e)
        {
			var result = tb_valid.Text.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var dist = result.Distinct().ToArray();

			lbl_countValid.Content = dist.Count();

			if (string.IsNullOrWhiteSpace(tb_valid.Text)) btn_Valid.IsEnabled = false;
			else btn_Valid.IsEnabled = true;
		}

        private void copyAllVs_Click(object sender, RoutedEventArgs e)
        {
			if (ViewValidSortie.SelectedIndex == -1) return;

			var temp = ViewValidSortie.SelectedItems;
			List<string> list = new List<string>();

			foreach (InvPostes item in temp)
			{
				list.Add(item.type + " " + item.model + " " + item.asset + " " + item.serial + " " + item.statut + " " + item.RF + " " + item.dateSortie + " " + item.RFretour + " " + item.dateRetour + " " + item.emplacement + " " + item.dateEntry + " " + item.dateEntryLab + " " + item.dateClone + " " + item.dateCloneValid);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

        private void copyRFvs_Click(object sender, RoutedEventArgs e)
        {
			if (ViewValidSortie.SelectedIndex == -1) return;

			List<string> list = new List<string>();
			var temp = ViewValidSortie.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.RF);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

        private void copyRFretourVs_Click(object sender, RoutedEventArgs e)
        {
			if (ViewValidSortie.SelectedIndex == -1) return;

			List<string> list = new List<string>();
			var temp = ViewValidSortie.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.RFretour);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

        private void copySNvs_Click(object sender, RoutedEventArgs e)
        {
			if (ViewValidSortie.SelectedIndex == -1) return;

			List<string> list = new List<string>();
			var temp = ViewValidSortie.SelectedItems;

			foreach (InvPostes item in temp)
			{
				list.Add(item.serial);
			}

			Clipboard.SetText(String.Join(Environment.NewLine, list));
		}

        private async void btn_rapportValid_Click(object sender, RoutedEventArgs e)
        {
            btn_rapportValid.IsEnabled = false;
            chk_special.IsEnabled = false;
            await Task.Run(TaskRapport);
            btn_rapportValid.IsEnabled = true;
            chk_special.IsEnabled = true;

			string path = @"C:\Inventaire Entrepot Settings\Rapport";

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				UseShellExecute = true,
				Arguments = path,
				FileName = "explorer.exe",
			};

			Process.Start(startInfo);
		}

        private void TaskRapport()
        {
			try
			{
				bool cocher = false;

				Application.Current.Dispatcher.Invoke(() =>
				{
					if (chk_special.IsChecked == true) cocher = true;
				});

				var recherche = ViewValidStock.Items.Cast<InvPostes>().ToList();
				var missing = listErreur.Items.Cast<string>().ToList();
				List<InvPostes> tous = new List<InvPostes>();

                if (cocher)
                {
                    tous = ViewValidSortie.Items.Cast<InvPostes>().ToList();
                }

                var wb = new XLWorkbook();

				IXLWorksheet ws3 = null;

				var ws = wb.Worksheets.Add("Résultat");

				if (cocher)
				{
					ws3 = wb.Worksheets.Add("Tous les modèles");
				}

				var ws2 = wb.Worksheets.Add("Introuvable");
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
				ws2.SheetView.FreezeRows(1);
				if (cocher) ws3.SheetView.FreezeRows(1);

				ws.Cell(2, 1).InsertData(recherche);
				ws2.Cell(2, 1).InsertData(missing);

				if (cocher) ws3.Cell(2, 1).InsertData(tous);

				ws.Column(22).Delete();
				ws.Column(21).Delete();
				ws.Column(20).Delete();
				ws.Column(19).Delete();
				ws.Column(18).Delete();
				ws.Column(17).Delete();
				ws.Column(16).Delete();
				ws.Column(15).Delete();

				if (cocher)
				{
					ws3.Column(22).Delete();
					ws3.Column(21).Delete();
					ws3.Column(20).Delete();
					ws3.Column(19).Delete();
					ws3.Column(18).Delete();
					ws3.Column(17).Delete();
					ws3.Column(16).Delete();
					ws3.Column(15).Delete();
				}

				for (int i = 1; i < 15; i++)
				{
					ws.Cell(1, i).Style = titlesStyle;
					if (cocher) ws3.Cell(1, i).Style = titlesStyle;
				}

				ws2.Cell(1, 1).Style = titlesStyle;

				ws.Cell(1, 1).Value = "Type";
				ws.Cell(1, 2).Value = "Modèle";
				ws.Cell(1, 3).Value = "# Actif";
				ws.Cell(1, 4).Value = "Numéro de série";
				ws.Cell(1, 5).Value = "Statut";
				ws.Cell(1, 6).Value = "RF de Sortie";
				ws.Cell(1, 7).Value = "Date de Sortie";
				ws.Cell(1, 8).Value = "RF de Retour";
				ws.Cell(1, 9).Value = "Date de Retour";
				ws.Cell(1, 10).Value = "Emplacement";
				ws.Cell(1, 11).Value = "Date d'entrée";
				ws.Cell(1, 12).Value = "Date d'envoie au Lab";
				ws.Cell(1, 13).Value = "Date de Clonage";
				ws.Cell(1, 14).Value = "Date Expiration Clonage";

				ws2.Cell(1, 1).Value = "Numéro de série";

				if (cocher)
				{
					ws3.Cell(1, 1).Value = "Type";
					ws3.Cell(1, 2).Value = "Modèle";
					ws3.Cell(1, 3).Value = "# Actif";
					ws3.Cell(1, 4).Value = "Numéro de série";
					ws3.Cell(1, 5).Value = "Statut";
					ws3.Cell(1, 6).Value = "RF de Sortie";
					ws3.Cell(1, 7).Value = "Date de Sortie";
					ws3.Cell(1, 8).Value = "RF de Retour";
					ws3.Cell(1, 9).Value = "Date de Retour";
					ws3.Cell(1, 10).Value = "Emplacement";
					ws3.Cell(1, 11).Value = "Date d'entrée";
					ws3.Cell(1, 12).Value = "Date d'envoie au Lab";
					ws3.Cell(1, 13).Value = "Date de Clonage";
					ws3.Cell(1, 14).Value = "Date Expiration Clonage";
				}

				ws.Cells().Style.Alignment.WrapText = true;
				ws.Columns().AdjustToContents();
				ws2.Cells().Style.Alignment.WrapText = true;
				ws2.Columns().AdjustToContents();

				if (cocher)
				{
					ws3.Cells().Style.Alignment.WrapText = true;
					ws3.Columns().AdjustToContents();
				}

				//ws.Rows().AdjustToContents();
				ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
				ws2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
				if (cocher) ws3.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

				string date = DateTime.Now.ToString("dd-MM-yyyy H;mm;ss");
				string link = @"C:\Inventaire Entrepot Settings\Rapport\Validation " + "(" + App.appData.UserName + ") " + date + ".xlsx";

				wb.SaveAs(link);

				MessageBox.Show("Generation du Rapport Terminer!", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

        private void btn_clearValid_Click(object sender, RoutedEventArgs e)
        {
			ViewValidSortie.ItemsSource = null;
			ViewValidStock.ItemsSource = null;
			listErreur.ItemsSource = null;
			tb_valid.Text = "";
			App.appData.validList.Clear();
			cb_specialType.SelectedIndex = -1;
		}
    }
}
