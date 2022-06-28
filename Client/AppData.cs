using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Client
{
	public class AppData : INotifyPropertyChanged
	{
		Window main;

		public readonly Object lockPrepare = new Object();
		public string version { get; } = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

		private ObservableCollection<ServerName> _serverList = new ObservableCollection<ServerName>(); // liste des server au login
		private ObservableCollection<InvPostes> _invPostes = new ObservableCollection<InvPostes>();
		private ObservableCollection<Waybills> _waybills = new ObservableCollection<Waybills>();
		private ObservableCollection<User> _userList = new ObservableCollection<User>();


		//private ObservableCollection<string> _logs = new ObservableCollection<string>();
		private ObservableCollection<string> _types = new ObservableCollection<string>();
		private ObservableCollection<string> _modeleMoniteur = new ObservableCollection<string>();
		private ObservableCollection<string> _choixLCD = new ObservableCollection<string>();
		private ObservableCollection<string> _statutList = new ObservableCollection<string> { "En Stock", "Sortie", "Réservé" };

		private ObservableCollection<string> _validTypes = new ObservableCollection<string>();

		private ObservableCollection<LogsRapport> _logsRapport = new ObservableCollection<LogsRapport>();
		public ObservableCollection<LogsRapport> logsRapport { get { return _logsRapport; } set { _logsRapport = value; } }

		public ObservableCollection<string> statutList { get { return _statutList; } set { _statutList = value; } }

		private ObservableCollection<string> _backup = new ObservableCollection<string>();

		private ObservableCollection<TypeModel> _typesModels = new ObservableCollection<TypeModel>();
		public ObservableCollection<TypeModel> typesModels { get { return _typesModels; } set { _typesModels = value; } }


		public ObservableCollection<string> choixLCD { get { return _choixLCD; } set { _choixLCD = value; } }

		//public ObservableCollection<string> logs { get { return _logs; } set { _logs = value; } }
		public ObservableCollection<string> types { get { return _types; } set { _types = value; } }

		public ObservableCollection<string> validTypes { get { return _validTypes; } set { _validTypes = value; } }

		public ObservableCollection<string> modeleMoniteur { get { return _modeleMoniteur; } set { _modeleMoniteur = value; } }

		public ObservableCollection<string> backup { get { return _backup; } set { _backup = value; } }

		private ObservableCollection<string> _modelSearchCB = new ObservableCollection<string>();
		public ObservableCollection<string> modelSearchCB { get { return _modelSearchCB; } set { _modelSearchCB = value; } }

		private ObservableCollection<string> _validList = new ObservableCollection<string>();
		public ObservableCollection<string> validList { get { return _validList; } set { _validList = value; } }

		private ObservableCollection<string> _validModel = new ObservableCollection<string>();
		public ObservableCollection<string> validModel { get { return _validModel; } set { _validModel = value; } }

		private ObservableCollection<string> _wbYears = new ObservableCollection<string>();
		public ObservableCollection<string> wbYears { get { return _wbYears; } set { _wbYears = value; } }

		private ObservableCollection<string> _mois = new ObservableCollection<string>();
		public ObservableCollection<string> mois { get { return _mois; } set { _mois = value; } }

		private ObservableCollection<string> _jour = new ObservableCollection<string>();
		public ObservableCollection<string> jour { get { return _jour; } set { _jour = value; } }


		private ObservableCollection<string> _modelPoste = new ObservableCollection<string>();
		public ObservableCollection<string> modelPoste { get { return _modelPoste; } set { _modelPoste = value; } }

		private ObservableCollection<string> _modelPortable = new ObservableCollection<string>();
		public ObservableCollection<string> modelPortable { get { return _modelPortable; } set { _modelPortable = value; } }

		private ObservableCollection<string> _modelServeur = new ObservableCollection<string>();
		public ObservableCollection<string> modelServeur { get { return _modelServeur; } set { _modelServeur = value; } }

		private string _user;
		private string _IPAddress = "";
		private string _contextName;
		private string _menuLogOut;
		private string _modelText = "";
		private string _WBdate = "";
		//public string curentStatus = "";

		private int _Port;
		private int _statusSelection;
		private int _count;
		private int _countLog;
		private int _countLab;
		private int _countAdd;
		private int _countAddLab;
		private int _countWB;
		//public int adminCount = 0;
		//public int adminCountTotal = 0;

		private SecureString _pass;
		private SecureString _oldPass;

		private string _privilege = "Aucun";
		private string _sortieSearch = "";
		private string _serialSearch = "";
		private string _ajoutSearch = "";
		private string _empSearch = "";
		private string _rfSearch = "";
		private string _wbRFsearch = "";
		private string _WBsearch = "";
		private string _cloneSearch = "";
		private string _selectedType = "";
		private string _selectedYear = "";
		private string _selectedMois = "";
		private string _selectedJour = "";
		private string _searchLog = "";

		private string _searchSNlab = "";
		private string _searchMODlab = "";

		private int _typeSearch = 0;
		private int _modelSearch = 0;
		private int _statutSearch = 0;

		private bool _wbClosed = true;
		public bool netChange = false;
		public bool loginList = true;
		private bool _quit = false;
		private bool _ok = false;
		private bool _connectEnable = false;
		private bool _registerEnable = false;
		private bool _confirmEnable = false;
		private bool _confirmUserInfo = false;
		private bool _registerForm = true;
		private bool _appQuit = false;
		private bool _firstRun = true;
		private bool _Ignore = false;
		private bool _serverAnswer;
		public bool waybill = false;
		private bool _enableEntrepot = false;
		private bool _enableLab = false;
		private bool _enableMain = false;
		private bool _enableAjout = true;
		private bool _enableAll = false;
		private bool _snColor = false;
		private bool _rapportCheck;
		private bool _delWB;
		private bool _delSeuil;
		private bool _isAdmin = false;

		private bool _enableCTX = false;

		private bool _transCheck = true;
		private bool _accesCheck = true;

		private Visibility _rapport = Visibility.Collapsed;
		private Visibility _admin = Visibility.Collapsed;
		private Visibility _visEnt = Visibility.Collapsed;
		private Visibility _visLab = Visibility.Collapsed;

		private Options _settings = new Options();
		public Options settings { get { return _settings; } set { _settings = value; OnPropertyChanged(); } }

		public void setMain(Window window)
		{
			main = window as MainWindow;
		}

		public int typeSearch { get { return _typeSearch; } set { _typeSearch = value; OnPropertyChanged(); invPostesView.Refresh(); } }
		public int modelSearch { get { return _modelSearch; } set { _modelSearch = value; OnPropertyChanged(); } }
		public int statutSearch { get { return _statutSearch; } set { _statutSearch = value; OnPropertyChanged(); invPostesView.Refresh(); } }
		public int statusSelection { get { return _statusSelection; } set { _statusSelection = value; OnPropertyChanged(); } }
		public int Port { get { return _Port; } set { _Port = value; OnPropertyChanged(); } }
		public int count { get { return _count; } set { _count = value; OnPropertyChanged(); } }
		public int countWB { get { return _countWB; } set { _countWB = value; OnPropertyChanged(); } }
		public int countAdd { get { return _countAdd; } set { _countAdd = value; OnPropertyChanged(); } }
		public int countAddLab { get { return _countAddLab; } set { _countAddLab = value; OnPropertyChanged(); } }
		public int countLab { get { return _countLab; } set { _countLab = value; OnPropertyChanged(); } }
		public int countLog { get { return _countLog; } set { _countLog = value; OnPropertyChanged(); } }

		public ObservableCollection<ServerName> serverList { get { return _serverList; } set { _serverList = value; OnPropertyChanged(); } }
		public ObservableCollection<InvPostes> invPostes { get { return _invPostes; } set { _invPostes = value; } }
		public ObservableCollection<Waybills> waybills { get { return _waybills; } set { _waybills = value; } }


		public ObservableCollection<User> userList { get { return _userList; } set { _userList = value; } }
		public ListCollectionView invPostesView { get { return CollectionViewSource.GetDefaultView(invPostes) as ListCollectionView; } }
		public ListCollectionView logView { get { return CollectionViewSource.GetDefaultView(logsRapport) as ListCollectionView; } }
		public ListCollectionView waybillsView { get { return CollectionViewSource.GetDefaultView(waybills) as ListCollectionView; } }
		public ListCollectionView sortMain { get { return CollectionViewSource.GetDefaultView(invPostesView) as ListCollectionView; } } // sorting file size
		public ListCollectionView invPostesViewLab { get { return test as ListCollectionView; } }


		public ICollectionView test;

		public void setFilter()
		{
			test = new ListCollectionView(invPostes);
			invPostesView.Filter = new Predicate<object>(o => FilterResult(o as InvPostes));
			invPostesViewLab.Filter = new Predicate<object>(o => FilterResultLab(o as InvPostes));
		}

		public void setWBfilter()
		{
			waybillsView.Filter = new Predicate<object>(o => FilterResultWB(o as Waybills));
		}

		public void setFilterLogs()
		{
			logView.Filter = new Predicate<object>(o => FilterLogs(o as LogsRapport));
		}

		public bool isAdmin { get { return _isAdmin; } set { _isAdmin = value; OnPropertyChanged(); } }
		public bool rapportCheck { get { return _rapportCheck; } set { _rapportCheck = value; OnPropertyChanged(); } }
		public bool snColor { get { return _snColor; } set { _snColor = value; OnPropertyChanged(); } }
		public bool enableMain { get { return _enableMain; } set { _enableMain = value; OnPropertyChanged(); } }
		public bool enableAll { get { return _enableAll; } set { _enableAll = value; OnPropertyChanged(); } }
		public bool enableEntrepot { get { return _enableEntrepot; } set { _enableEntrepot = value; OnPropertyChanged(); } }
		public bool enableLab { get { return _enableLab; } set { _enableLab = value; OnPropertyChanged(); } }
		public bool enableAjout { get { return _enableAjout; } set { _enableAjout = value; OnPropertyChanged(); } }
		public bool delWB { get { return _delWB; } set { _delWB = value; OnPropertyChanged(); } }
		public bool delSeuil { get { return _delSeuil; } set { _delSeuil = value; OnPropertyChanged(); } }
		public bool enableCTX { get { return _enableCTX; } set { _enableCTX = value; OnPropertyChanged(); } }
		public bool accesCheck { get { return _accesCheck; } set { _accesCheck = value; OnPropertyChanged(); } }
		public bool transCheck { get { return _transCheck; } set { _transCheck = value; OnPropertyChanged(); } }
		public bool wbClosed { get { return _wbClosed; } set { _wbClosed = value; OnPropertyChanged(); } }

		public Visibility rapport { get { return _rapport; } set { _rapport = value; OnPropertyChanged(); } }
		public Visibility admin { get { return _admin; } set { _admin = value; OnPropertyChanged(); } }
		public Visibility visEnt { get { return _visEnt; } set { _visEnt = value; OnPropertyChanged(); } }
		public Visibility visLab { get { return _visLab; } set { _visLab = value; OnPropertyChanged(); } }

		public bool serverAnswer { get { return _serverAnswer; } set { _serverAnswer = value; } }
		public bool confirmUserInfo { get { return _confirmUserInfo; } set { _confirmUserInfo = value; } }
		public bool Ignore { get { return _Ignore; } set { _Ignore = value; } }
		public bool appQuit { get { return _appQuit; } set { _appQuit = value; } }
		public bool firstRun { get { return _firstRun; } set { _firstRun = value; } }
		public bool quit { get { return _quit; } set { _quit = value; } }
		public bool ok { get { return _ok; } set { _ok = value; } }
		public bool connectEnable { get { return _connectEnable; } set { _connectEnable = value; OnPropertyChanged(); } }
		public bool registerForm { get { return _registerForm; } set { _registerForm = value; OnPropertyChanged(); } }
		public bool registerEnable { get { return _registerEnable; } set { _registerEnable = value; OnPropertyChanged(); } }
		public bool confirmEnable { get { return _confirmEnable; } set { _confirmEnable = value; OnPropertyChanged(); } }

		public SecureString Password { get { return _pass; } set { _pass = value; OnPropertyChanged(); } }
		public SecureString oldPassword { get { return _oldPass; } set { _oldPass = value; OnPropertyChanged(); } }

		public string WBdate { get { return _WBdate; } set { _WBdate = value; OnPropertyChanged(); } }
		public string selectedType { get { return _selectedType; } set { _selectedType = value; OnPropertyChanged(); } }
		public string selectedYear { get { return _selectedYear; } set { _selectedYear = value; OnPropertyChanged(); } }
		public string selectedMois { get { return _selectedMois; } set { _selectedMois = value; OnPropertyChanged(); waybillsView.Refresh(); } }
		public string selectedJour { get { return _selectedJour; } set { _selectedJour = value; OnPropertyChanged(); waybillsView.Refresh(); } }
		public string modelText { get { return _modelText; } set { _modelText = value; OnPropertyChanged(); invPostesView.Refresh(); } }
		public string sortieSearch { get { return _sortieSearch; } set { _sortieSearch = value; OnPropertyChanged(); invPostesView.Refresh(); } }
		public string serialSearch { get { return _serialSearch; } set { _serialSearch = value; OnPropertyChanged(); invPostesView.Refresh(); } }
		public string ajoutSearch { get { return _ajoutSearch; } set { _ajoutSearch = value; OnPropertyChanged(); invPostesView.Refresh(); } }
		public string empSearch { get { return _empSearch; } set { _empSearch = value; OnPropertyChanged(); invPostesView.Refresh(); } }
		public string cloneSearch { get { return _cloneSearch; } set { _cloneSearch = value; OnPropertyChanged(); invPostesView.Refresh(); } }
		public string rfSearch { get { return _rfSearch; } set { _rfSearch = value; OnPropertyChanged(); invPostesView.Refresh(); } }
		public string wbRFsearch { get { return _wbRFsearch; } set { _wbRFsearch = value; OnPropertyChanged(); waybillsView.Refresh(); } }
		public string WBsearch { get { return _WBsearch; } set { _WBsearch = value; OnPropertyChanged(); waybillsView.Refresh(); } }
		public string searchLog { get { return _searchLog; } set { _searchLog = value; OnPropertyChanged(); logView.Refresh(); } }
		public string menuLogOut { get { return _menuLogOut; } set { _menuLogOut = value; OnPropertyChanged(); } }
		public string contextName { get { return _contextName; } set { _contextName = value; OnPropertyChanged(); } }
		public string UserName { get { return _user; } set { _user = value; OnPropertyChanged(); } }
		public string searchSNlab { get { return _searchSNlab; } set { _searchSNlab = value; OnPropertyChanged(); invPostesViewLab.Refresh(); } }
		public string searchMODlab { get { return _searchMODlab; } set { _searchMODlab = value; OnPropertyChanged(); invPostesViewLab.Refresh(); } }

		public string IPAddress
		{
			get { return _IPAddress; }
			set
			{
				_IPAddress = value;
				OnPropertyChanged();
			}
		}

		public string privilege { get { return _privilege; } set { _privilege = value; OnPropertyChanged(); Permissions(); invPostesView.Refresh(); } }

		private bool FilterResult(InvPostes item)
		{
			if (privilege == "Aucun") return false;
			//if (item.serial != null && item.serial.IndexOf(serialSearch, StringComparison.OrdinalIgnoreCase) != -1) return true;

			if (!string.IsNullOrEmpty(rfSearch))
			{
				if (item.RF.Contains(rfSearch.ToUpper()) || item.RFretour.Contains(rfSearch.ToUpper())) return true;
				else return false;
			}

			if (!string.IsNullOrEmpty(serialSearch))
			{
				if (item.serial.Contains(serialSearch.ToUpper())) return true;
				else return false;
			}

			//if (!string.IsNullOrEmpty(empSearch))
			//{
			//	if (item.emplacement.ToLower() == empSearch.ToLower()) return true;
			//	else return false;
			//}

			if (!string.IsNullOrEmpty(cloneSearch))
			{
				if (item.dateClone.Contains(cloneSearch)) return true;
				else return false;
			}

			//if (!string.IsNullOrEmpty(sortieSearch))
			//{
			//	if (item.dateSortie.Contains(sortieSearch)) return true;
			//	else return false;
			//}

			if (!string.IsNullOrEmpty(ajoutSearch))
			{
				if (item.dateEntry.Contains(ajoutSearch)) return true;
				else return false;
			}

			if (typeSearch == 0)
			{
				if (statutSearch == 0)
				{
					if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
					{
						if (item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					return true;
				}

				if (statutSearch == 1 && item.statut.Contains("En Stock") && !item.emplacement.Contains("QUANTUM") && !item.emplacement.Contains("REPAIR"))
				{
					if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
					{
						if (item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					return true;
				}


				if (statutSearch == 2 && item.statut.Contains("Sortie"))
				{
					if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
					{
						if (item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					return true;
				}

				if (statutSearch == 3 && !string.IsNullOrEmpty(item.dateClone) && item.statut == "En Stock")
				{
					if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
					{
						if (item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					return true;
				}

				if (statutSearch == 4 && item.emplacement == "QUANTUM")
				{
					if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
					{
						if (item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					return true;
				}

				if (statutSearch == 5)
				{
					if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
					{
						if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
						else return false;
					}

					if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
					{
						if (item.dateSortie.Contains(sortieSearch)) return true;
						else return false;
					}

					return true;
				}
			}

			if (item.type == selectedType)
			{
				if (modelSearch == 0)
				{
					if (statutSearch == 0)
					{
						if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
						{
							if (item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						return true;
					}

					if (statutSearch == 1 && item.statut.Contains("En Stock") && !item.emplacement.Contains("QUANTUM") && !item.emplacement.Contains("REPAIR"))
					{
						if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
						{
							if (item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						return true;
					}

					if (statutSearch == 2 && item.statut.Contains("Sortie"))
					{
						if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
						{
							if (item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						return true;
					}

					if (statutSearch == 3 && !string.IsNullOrEmpty(item.dateClone))
					{
						if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}
						else if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
							else return false;
						}
						else if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
						{
							if (item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}
						else
						{
							if (item.statut == "En Stock") return true;
						}

						//return true;
					}

					if (statutSearch == 4 && item.emplacement == "QUANTUM")
					{
						if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
						{
							if (item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						return true;
					}

					if (statutSearch == 5 && item.type == "Serveur" && !string.IsNullOrEmpty(item.xcolor))
					{
						if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
						{
							if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
							else return false;
						}

						if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
						{
							if (item.dateSortie.Contains(sortieSearch)) return true;
							else return false;
						}

						return true;
					}
				}

				if (modelSearch > 0)
				{
					if (item.model == modelText)
					{
						if (statutSearch == 0)
						{
							if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
							{
								if (item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							return true;
						}

						if (statutSearch == 1 && item.statut.Contains("En Stock") && !item.emplacement.Contains("QUANTUM") && !item.emplacement.Contains("REPAIR"))
						{
							if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
							{
								if (item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							return true;
						}


						if (statutSearch == 2 && item.statut.Contains("Sortie"))
						{
							if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
							{
								if (item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							return true;
						}

						if (statutSearch == 3 && !string.IsNullOrEmpty(item.dateClone))
						{
							if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}
							else if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
								else return false;
							}
							else if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
							{
								if (item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}
							else
							{
								if (item.statut == "En Stock") return true;
							}

							//return true;
						}

						if (statutSearch == 4 && item.emplacement == "QUANTUM")
						{
							if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
							{
								if (item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							return true;
						}

						if (statutSearch == 5 && item.type == "Serveur" && !string.IsNullOrEmpty(item.xcolor))
						{
							if (!string.IsNullOrEmpty(empSearch) && !string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower()) && item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(empSearch) && string.IsNullOrEmpty(sortieSearch))
							{
								if (item.emplacement.ToLower().Contains(empSearch.ToLower())) return true;
								else return false;
							}

							if (!string.IsNullOrEmpty(sortieSearch) && string.IsNullOrEmpty(empSearch))
							{
								if (item.dateSortie.Contains(sortieSearch)) return true;
								else return false;
							}

							return true;
						}
					}
				}
			}

			return false;
		}

		private bool FilterResultLab(InvPostes item)
		{
			if (item.emplacement == "Au Lab")
			{
				if (string.IsNullOrWhiteSpace(searchSNlab) && string.IsNullOrWhiteSpace(searchMODlab)) return true;
				else if (!string.IsNullOrWhiteSpace(searchSNlab) && string.IsNullOrWhiteSpace(searchMODlab))
				{
					if (item.serial.ToLower().Contains(searchSNlab.ToLower())) return true;
				}
				else if (string.IsNullOrWhiteSpace(searchSNlab) && !string.IsNullOrWhiteSpace(searchMODlab))
				{
					if (item.model.ToLower().Contains(searchMODlab.ToLower())) return true;
				}
			}

			return false;
		}

		private bool FilterLogs(LogsRapport item)
		{
			if (item.info.ToLower().Contains(searchLog.ToLower()) || string.IsNullOrEmpty(searchLog)) return true;
			return false;
		}

		private bool FilterResultWB(Waybills item)
		{
			if (selectedMois == "Tous")
			{
				var info = wbRFsearch.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

				foreach (var data in info)
                {
					if (item.RF.Contains(data.ToUpper()) && string.IsNullOrEmpty(WBsearch)) return true;
				}

				
				if (item.wayb.Contains(WBsearch.ToUpper()) && string.IsNullOrEmpty(wbRFsearch)) return true;
			}
			else if (selectedMois != "Tous" && selectedJour == "Tous")
			{
				if (item.mois == selectedMois)
				{
					if (item.RF.Contains(wbRFsearch.ToUpper()) && string.IsNullOrEmpty(WBsearch)) return true;
					if (item.wayb.Contains(WBsearch.ToUpper()) && string.IsNullOrEmpty(wbRFsearch)) return true;
				}
			}
			else if (selectedMois != "Tous" && selectedJour != "Tous")
			{
				if (item.mois == selectedMois && item.jour == selectedJour)
				{
					if (item.RF.Contains(wbRFsearch.ToUpper()) && string.IsNullOrEmpty(WBsearch)) return true;
					if (item.wayb.Contains(WBsearch.ToUpper()) && string.IsNullOrEmpty(wbRFsearch)) return true;
				}
			}

			return false;
		}

		public void Permissions()
		{
			if (privilege == "Aucun")
			{
				enableEntrepot = false;
				enableLab = false;
				enableAjout = false;
				isAdmin = false;
				admin = Visibility.Collapsed;
				visEnt = Visibility.Collapsed;
				visLab = Visibility.Collapsed;
				rapport = Visibility.Collapsed;
				(main as MainWindow).tabControl.SelectedIndex = -1;
				(main as MainWindow).btn_LabClone.IsEnabled = false;
				CloseAdmin();
				CloseEntrepot();
			}
			else if (privilege == "Vue Inventaire")
			{
				enableEntrepot = false;
				enableLab = false;
				enableAjout = false;
				isAdmin = false;
				visEnt = Visibility.Visible;
				visLab = Visibility.Visible;
				admin = Visibility.Collapsed;
				rapport = Visibility.Visible;
				if ((main as MainWindow).tabControl.SelectedIndex == -1) (main as MainWindow).tabControl.SelectedIndex = 0;
				(main as MainWindow).btn_LabClone.IsEnabled = false;
				CloseAdmin();
				CloseEntrepot();
			}
			else if (privilege == "Entrepot")
			{
				enableEntrepot = true;
				enableLab = false;
				enableAjout = true;
				isAdmin = false;
				visEnt = Visibility.Visible;
				rapport = Visibility.Visible;
				visLab = Visibility.Collapsed;
				admin = Visibility.Collapsed;
				(main as MainWindow).tabControl.SelectedIndex = 0;
				(main as MainWindow).btn_LabClone.IsEnabled = false;
				CloseAdmin();
			}
			else if (privilege == "Lab")
			{
				enableEntrepot = false;
				enableLab = true;
				enableAjout = true;
				isAdmin = false;
				admin = Visibility.Collapsed;
				visEnt = Visibility.Collapsed;
				visLab = Visibility.Visible;
				rapport = Visibility.Collapsed;
				(main as MainWindow).tabControl.SelectedIndex = 1;
				(main as MainWindow).btn_LabClone.IsEnabled = true;
				CloseAdmin();
				CloseEntrepot();
			}
			else if (privilege == "Entrepot/Lab")
			{
				enableEntrepot = true;
				enableLab = true;
				enableAjout = true;
				isAdmin = false;
				admin = Visibility.Collapsed;
				visEnt = Visibility.Visible;
				visLab = Visibility.Visible;
				rapport = Visibility.Visible;
				(main as MainWindow).tabControl.SelectedIndex = 1;
				(main as MainWindow).btn_LabClone.IsEnabled = true;
				CloseAdmin();
				CloseEntrepot();
			}
			else if (privilege == "Administrateur")
			{
				admin = Visibility.Visible;
				visEnt = Visibility.Visible;
				visLab = Visibility.Visible;
				rapport = Visibility.Visible;
				isAdmin = true;
				enableAjout = true;
				enableLab = true;
				enableEntrepot = true;
				(main as MainWindow).tabControl.SelectedIndex = 0;
				(main as MainWindow).btn_LabClone.IsEnabled = true;
			}
		}

		private void CloseAdmin()
		{
			foreach (Window window in Application.Current.Windows)
			{
				if (window.GetType() == typeof(Admin))
				{
					(window as Admin).Close();
					break;
				}

				if (window.GetType() == typeof(Logs))
				{
					(window as Logs).Close();
					break;
				}
			}
		}

		private void CloseEntrepot()
		{
			foreach (Window window in Application.Current.Windows)
			{
				if (window.GetType() == typeof(AjoutInventaire))
				{
					(window as AjoutInventaire).Close();
					break;
				}

				if (window.GetType() == typeof(Retour))
				{
					(window as Retour).Close();
					break;
				}

				if (window.GetType() == typeof(Sortie))
				{
					(window as Sortie).Close();
					break;
				}

				if (window.GetType() == typeof(SendLab))
				{
					(window as SendLab).Close();
					break;
				}

				if (window.GetType() == typeof(Emplacement))
				{
					(window as Emplacement).Close();
					break;
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}
	}

	public class ServerName
	{
		public string Server { get; set; }
		public int Port { get; set; }
	}

	public class User : INotifyPropertyChanged
	{
		public string username { get; set; }
		private string _privilege;

		public string privilege { get { return _privilege; } set { _privilege = value; OnPropertyChanged(); } }

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}
	}

	public class Waybills : INotifyPropertyChanged
	{
		private string _RF;
		private string _wayb;
		private string _wbRetour;
		private string _mois;
		private string _jour;
		private string _comment = "";

		public string mois { get { return _mois; } set { _mois = value; OnPropertyChanged(); } }
		public string jour { get { return _jour; } set { _jour = value; OnPropertyChanged(); } }
		public string RF { get { return _RF; } set { _RF = value; OnPropertyChanged(); } }
		public string wayb { get { return _wayb; } set { _wayb = value; OnPropertyChanged(); } }
		public string wbRetour { get { return _wbRetour; } set { _wbRetour = value; OnPropertyChanged(); } }
		public string comment { get { return _comment; } set { _comment = value; OnPropertyChanged(); } }

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}
	}

	public class TypeModel : INotifyPropertyChanged
	{
		private string _type;
		private ObservableCollection<string> _modeles = new ObservableCollection<string>();

		public string type { get { return _type; } set { _type = value; OnPropertyChanged(); } }
		public ObservableCollection<string> modeles { get { return _modeles; } set { _modeles = value; } }

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}
	}

	public class InvPostes : INotifyPropertyChanged
	{
		private string _type = "";
		private string _model = "";
		private string _magasin = "";
		private string _serial = "";
		private string _statut = "";
		private string _RF = "";
		private string _dateSortie = "";
		private string _RFretour = "";
		private string _dateRetour = "";
		private string _emplacement = "";
		private string _dateEntry = "";
		private string _dateEntryLab = "";
		private string _dateClone = "";
		private string _dateCloneValid = "";
		private string _xcolor = "";
		private string _infoAjout = "";
		private string _comment = "";

		private List<string> _infoEmp = new List<string>();
		private List<string> _infoSortie = new List<string>();
		private List<string> _infoRetour = new List<string>();
		private List<string> _infoEnvoieClone = new List<string>();
		private List<string> _infoValidClone = new List<string>();

		public string type { get { return _type; } set { _type = value; OnPropertyChanged(); } }
		public string model { get { return _model; } set { _model = value; OnPropertyChanged(); } }
		public string magasin { get { return _magasin; } set { _magasin = value; OnPropertyChanged(); } }
		public string serial { get { return _serial; } set { _serial = value; OnPropertyChanged(); } }
		public string statut { get { return _statut; } set { _statut = value; OnPropertyChanged(); } }
		public string RF { get { return _RF; } set { _RF = value; OnPropertyChanged(); } }
		public string dateSortie { get { return _dateSortie; } set { _dateSortie = value; OnPropertyChanged(); } }
		public string RFretour { get { return _RFretour; } set { _RFretour = value; OnPropertyChanged(); } }
		public string dateRetour { get { return _dateRetour; } set { _dateRetour = value; OnPropertyChanged(); } }
		public string emplacement { get { return _emplacement; } set { _emplacement = value; OnPropertyChanged(); } }
		public string dateEntry { get { return _dateEntry; } set { _dateEntry = value; OnPropertyChanged(); } }
		public string dateEntryLab { get { return _dateEntryLab; } set { _dateEntryLab = value; OnPropertyChanged(); } }
		public string dateClone { get { return _dateClone; } set { _dateClone = value; OnPropertyChanged(); } }
		public string dateCloneValid { get { return _dateCloneValid; } set { _dateCloneValid = value; OnPropertyChanged(); } }
		public string xcolor { get { return _xcolor; } set { _xcolor = value; OnPropertyChanged(); } }
		public string infoAjout { get { return _infoAjout; } set { _infoAjout = value; } }
		public string comment { get { return _comment; } set { _comment = value; } }
		public List<string> infoEmp { get { return _infoEmp; } set { _infoEmp = value; } }
		public List<string> infoSortie { get { return _infoSortie; } set { _infoSortie = value; } }
		public List<string> infoRetour { get { return _infoRetour; } set { _infoRetour = value; } }
		public List<string> infoEnvoieClone { get { return _infoEnvoieClone; } set { _infoEnvoieClone = value; } }
		public List<string> infoValidClone { get { return _infoValidClone; } set { _infoValidClone = value; } }

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}
	}

	public class Options : INotifyPropertyChanged
	{
		private bool _DarkMode = false;
		private bool _remember = false;
		private int _lastServer = -1;
		public string user = "";

		public bool DarkMode { get { return _DarkMode; } set { _DarkMode = value; OnPropertyChanged(); } }
		public bool remember { get { return _remember; } set { _remember = value; OnPropertyChanged(); } }
		public int lastServer { get { return _lastServer; } set { _lastServer = value; OnPropertyChanged(); } }

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}
	}

	public class LogsRapport : INotifyPropertyChanged
	{
		private string _date = "";
		private string _nom = "";
		private string _type = "";
		private string _amount = "";
		private string _info = "";

		public string date { get { return _date; } set { _date = value; } }
		public string nom { get { return _nom; } set { _nom = value; } }
		public string type { get { return _type; } set { _type = value; } }
		public string amount { get { return _amount; } set { _amount = value; } }
		public string info { get { return _info; } set { _info = value; OnPropertyChanged(); } }


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}
	}

	public class CustomerSorter : IComparer
	{
		bool sort;
		string task;

		public CustomerSorter(bool data, string info)
		{
			sort = data;
			task = info;
		}

		public int Compare(object x, object y)
		{
			InvPostes fileX = x as InvPostes;
			InvPostes fileY = y as InvPostes;

			if (task == "type")
			{
				if (sort) return fileX.type.CompareTo(fileY.type);
				else return fileY.type.CompareTo(fileX.type);
			}

			if (task == "model")
			{
				if (sort) return fileX.model.CompareTo(fileY.model);
				else return fileY.model.CompareTo(fileX.model);
			}


			if (task == "emp")
			{
				if (sort) return fileX.emplacement.CompareTo(fileY.emplacement);
				else return fileY.emplacement.CompareTo(fileX.emplacement);
			}

			return 0;
		}
	}
}