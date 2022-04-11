using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace entrepotServer
{
	public class AppData
	{
		public readonly Object lockDB = new Object();
		public readonly Object lockModel = new Object();
		public readonly Object lockPrepare = new Object();
		public readonly Object lockUpload= new Object();
		public readonly Object lockWB = new Object();
		public  readonly Object lockWrite = new Object();

		public string dateWB = "";

		private List<InvPostes> _invPostes = new List<InvPostes>();

		private List<InvPostes> _jour = new List<InvPostes>();
		public List<InvPostes> invPostes { get { return _invPostes; } set { _invPostes = value; } }

		public List<InvPostes> jour { get { return _jour; } set { _jour = value; } }

		//public List<string> logs = new List<string>();


		private List<LogsRapport> _logsRapport = new List<LogsRapport>();
		public List<LogsRapport> logsRapport { get { return _logsRapport; } set { _logsRapport = value; } }

		private List<SelectedTransit> _selectedTransit = new List<SelectedTransit>();
		public List<SelectedTransit> selectedTransit { get { return _selectedTransit; } set { _selectedTransit = value; } }

		public List<string> yearList = new List<string>();

		public List<string> seuilMoniteur = new List<string>();

		//public List<string> preparationSemaine = new List<string>();

		//private List<Achat> _tempAchat = new List<Achat>();
		//public List<Achat> tempAchat { get { return _tempAchat; } set { _tempAchat = value; } }

		private List<Achat> _fichierAchat = new List<Achat>();
		public List<Achat> fichierAchat { get { return _fichierAchat; } set { _fichierAchat = value; } }

		private List<Preparation> _listPreparation = new List<Preparation>();
		public List<Preparation> listPreparation { get { return _listPreparation; } set { _listPreparation = value; } }


		private List<Seuil> _seuilPoste = new List<Seuil>();
		public List<Seuil> seuilPoste { get { return _seuilPoste; } set { _seuilPoste = value; } }

		private List<Seuil> _seuilAccess = new List<Seuil>();
		public List<Seuil> seuilAccess { get { return _seuilAccess; } set { _seuilAccess = value; } }

		private List<seuilMoniteurAdmin> _moniteurSeuil = new List<seuilMoniteurAdmin>();
		public List<seuilMoniteurAdmin> moniteurSeuil { get { return _moniteurSeuil; } set { _moniteurSeuil = value; } }


		private List<Achat> _nip = new List<Achat>();
		public List<Achat> nip { get { return _nip; } set { _nip = value; } }

		//private List<AchatNIP> _fichierAchatNIP = new List<AchatNIP>();
		//public List<AchatNIP> fichierAchatNIP { get { return _fichierAchatNIP; } set { _fichierAchatNIP = value; } }

		private List<Waybills> _waybills = new List<Waybills>();
		public List<Waybills> waybills { get { return _waybills; } set { _waybills = value; } }

		//private List<string> _types = new List<string>();
		//private List<string> _modelePoste = new List<string>();
		//private List<string> _modeleLaptop = new List<string>();
		//private List<string> _modeleTablette = new List<string>();
		private List<string> _modeleMoniteur = new List<string>();
		//private List<string> _modeleNIP = new List<string>();
		//private List<string> _modeleIMP = new List<string>();

		private List<TypeModel> _typesModels = new List<TypeModel>();
		public List<TypeModel> typesModels { get { return _typesModels; } set { _typesModels = value; } }

		//public List<string> types { get { return _types; } set { _types = value; } }
		//public List<string> modelePoste { get { return _modelePoste; } set { _modelePoste = value; } }
		//public List<string> modeleLaptop { get { return _modeleLaptop; } set { _modeleLaptop = value; } }
		//public List<string> modeleTablette { get { return _modeleTablette; } set { _modeleTablette = value; } }
		public List<string> modeleMoniteur { get { return _modeleMoniteur; } set { _modeleMoniteur = value; } }
		//public List<string> modeleNIP { get { return _modeleNIP; } set { _modeleNIP = value; } }
		//public List<string> modeleIMP { get { return _modeleIMP; } set { _modeleIMP = value; } }

		public string backupFolder = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"backup";


		//private List<InvMoniteur> _invMoniteurs = new List<InvMoniteur>();
		//public List<InvMoniteur> invMoniteurs { get { return _invMoniteurs; } set { _invMoniteurs = value; } }


		//private List<InvAccess> _invAccess = new List<InvAccess>();
		//public List<InvAccess> invAccess { get { return _invAccess; } set { _invAccess = value; } }
		//private List<seuilMoniteurAdmin> _seuilMoniteurAdmin = new List<seuilMoniteurAdmin>();
		//public List<seuilMoniteurAdmin> seuilMoniteurAdmin { get { return _seuilMoniteurAdmin; } set { _seuilMoniteurAdmin = value; } }
	}

	public class InvPostes
	{
		private string _type;
		private string _model;
		private string _serial;
		private string _asset = "";
		private string _emplacement = "";
		private string _statut = "";
		private string _RF = "";
		private string _RFretour = "";
		private string _dateEntry = "";
		private string _dateSortie= "";
		private string _dateRetour = "";
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

		public string type { get { return _type; } set { _type = value; } }
		public string model { get { return _model; } set { _model = value; } }
		public string asset { get { return _asset; } set { _asset = value; } }
		public string serial { get { return _serial; } set { _serial = value; } }
		public string statut { get { return _statut; } set { _statut = value;} }
		public string RF { get { return _RF; } set { _RF = value; } }
		public string dateSortie { get { return _dateSortie; } set { _dateSortie = value; } }
		public string RFretour { get { return _RFretour; } set { _RFretour = value; } }
		public string dateRetour { get { return _dateRetour; } set { _dateRetour = value; } }
		public string emplacement { get { return _emplacement; } set { _emplacement = value; } }
		public string dateEntry { get { return _dateEntry; } set { _dateEntry = value; } }
		public string dateEntryLab { get { return _dateEntryLab; } set { _dateEntryLab = value; } }
		public string dateClone { get { return _dateClone; } set { _dateClone = value; } }
		public string dateCloneValid { get { return _dateCloneValid; } set { _dateCloneValid = value; } }
		public string xcolor { get { return _xcolor; } set { _xcolor = value; } }
		public string infoAjout { get { return _infoAjout; } set { _infoAjout = value; } }
		public string comment { get { return _comment; } set { _comment = value; } }

		public List<string> infoEmp { get { return _infoEmp; } set { _infoEmp = value; } }
		public List<string> infoSortie { get { return _infoSortie; } set { _infoSortie = value; } }
		public List<string> infoRetour { get { return _infoRetour; } set { _infoRetour = value; } }
		public List<string> infoEnvoieClone { get { return _infoEnvoieClone; } set { _infoEnvoieClone = value; } }
		public List<string> infoValidClone { get { return _infoValidClone; } set { _infoValidClone = value; } }
	}

	public class Achat
	{
		private string _type;
		private string _model;
		private string _serial;
		private string _asset = "";
		private string _guid = "";

		public string type { get { return _type; } set { _type = value; } }
		public string model { get { return _model; } set { _model = value; } }
		public string asset { get { return _asset; } set { _asset = value; } }
		public string serial { get { return _serial; } set { _serial = value; } }
		public string guid { get { return _guid; } set { _guid = value; } }
	}

	public class TypeModel
	{
		private string _type;
		private List<string> _modeles = new List<string>();

		public string type { get { return _type; } set { _type = value; } }
		public List<string> modeles { get { return _modeles; } set { _modeles = value; } }
	}

	public class AchatNIP
	{
		private string _type;
		private string _model;
		private string _serial;

		public string type { get { return _type; } set { _type = value; } }
		public string model { get { return _model; } set { _model = value; } }
		public string serial { get { return _serial; } set { _serial = value; } }
	}

	public class LogsRapport
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
		public string info { get { return _info; } set { _info = value; } }
	}

	public class Waybills
	{
		private string _RF;
		private string _wayb;
		private string _wbRetour;
		private string _mois;
		private string _jour;
		private string _comment = "";

		public string RF { get { return _RF; } set { _RF = value; } }
		public string wayb { get { return _wayb; } set { _wayb = value; } }
		public string wbRetour { get { return _wbRetour; } set { _wbRetour = value; } }
		public string mois { get { return _mois; } set { _mois = value; } }
		public string jour { get { return _jour; } set { _jour = value; } }
		public string comment { get { return _comment; } set { _comment = value; } }
	}

	public class Seuil
	{
		private string _type;
		private string _actuel;
		private string _alerte = "";
		private string _max = "";

		public string type { get { return _type; } set { _type = value; } }
		public string actuel { get { return _actuel; } set { _actuel = value; } }
		public string alerte { get { return _alerte; } set { _alerte = value; } }
		public string max { get { return _max; } set { _max = value; } }
	}

	public class seuilMoniteurAdmin
	{
		public string type { get; set; }
		private List<string> _modele = new List<string>();
		public string alerte { get; set; }
		public string max { get; set; }

		public List<string> modele { get { return _modele; } set { _modele = value; } }
	}

	public class Preparation
	{
		public string semaine { get; set; }

		private List<Contenu> _info = new List<Contenu>();
		public List<Contenu> info { get { return _info; } set { _info = value; } }
	}

	public class SelectedTransit
	{
		private string _transit = "";
		private int _count = 0;

		public string transit { get { return _transit; } set { _transit = value; } }
		public int count { get { return _count; } set { _count = value; } }
	}

	public class Contenu
	{
		private string _transit = "";
		private string _mini = "0";
		private string _sff = "0";
		private string _laptop = "0";
		private string _lcd22 = "0";
		private string _lcd27 = "0";
		private string _clavier = "0";
		private string _sourisSansFil = "0";
		private string _cableSecure = "0";
		private string _sac = "0";
		private string _sacBandou = "0";
		private string _hubUsb = "0";
		private string _usbcDP = "0";
		private string _nip = "0";
		private string _livret = "0";
		private string _recu = "0";
		private bool _ready = false;
		private bool _selected = false;
		private string _comment = "";
		private string _rf = "";

		private bool _clavierCheck = false;
		private bool _sourisSansFilCheck = false;
		private bool _cableSecureCheck = false;
		private bool _sacCheck = false;
		private bool _sacBandouCheck = false;
		private bool _hubUsbCheck = false;
		private bool _usbcDPCheck = false;
		private bool _livretCheck = false;

		public bool clavierCheck { get { return _clavierCheck; } set { _clavierCheck = value; } }
		public bool sourisSansFilCheck { get { return _sourisSansFilCheck; } set { _sourisSansFilCheck = value; } }
		public bool cableSecureCheck { get { return _cableSecureCheck; } set { _cableSecureCheck = value; } }
		public bool sacCheck { get { return _sacCheck; } set { _sacCheck = value; } }
		public bool sacBandouCheck { get { return _sacBandouCheck; } set { _sacBandouCheck = value; } }
		public bool hubUsbCheck { get { return _hubUsbCheck; } set { _hubUsbCheck = value; } }
		public bool usbcDPCheck { get { return _usbcDPCheck; } set { _usbcDPCheck = value; } }
		public bool livretCheck { get { return _livretCheck; } set { _livretCheck = value; } }

		public string transit { get { return _transit; } set { _transit = value; } }
		public string mini { get { return _mini; } set { _mini = value; } }
		public string sff { get { return _sff; } set { _sff = value; } }
		public string laptop { get { return _laptop; } set { _laptop = value; } }
		public string lcd22 { get { return _lcd22; } set { _lcd22 = value;  } }
		public string lcd27 { get { return _lcd27; } set { _lcd27 = value;  } }
		public string clavier { get { return _clavier; } set { _clavier = value;  } }
		public string sourisSansFil { get { return _sourisSansFil; } set { _sourisSansFil = value; } }
		public string cableSecure { get { return _cableSecure; } set { _cableSecure = value;  } }
		public string sac { get { return _sac; } set { _sac = value; } }
		public string sacBandou { get { return _sacBandou; } set { _sacBandou = value; } }
		public string hubUsb { get { return _hubUsb; } set { _hubUsb = value; } }
		public string usbcDP { get { return _usbcDP; } set { _usbcDP = value; } }
		public string nip { get { return _nip; } set { _nip = value; } }
		public string livret { get { return _livret; } set { _livret = value; } }
		public string recu { get { return _recu; } set { _recu = value; } }
		public bool ready { get { return _ready; } set { _ready = value; } }
		public bool selected { get { return _selected; } set { _selected = value; } }
		public string comment { get { return _comment; } set { _comment = value; } }
		public string rf { get { return _rf; } set { _rf = value; } }

		private List<string> _SNmini = new List<string>();
		private List<string> _SNsff = new List<string>();
		private List<string> _SNlaptop = new List<string>();
		private List<string> _SNlcd22 = new List<string>();
		private List<string> _SNlcd27 = new List<string>();
		private List<string> _SNnip = new List<string>();
		private List<string> _SNrecu = new List<string>();

		public List<string> SNmini { get { return _SNmini; } set { _SNmini = value; } }
		public List<string> SNsff { get { return _SNsff; } set { _SNsff = value; } }
		public List<string> SNlaptop { get { return _SNlaptop; } set { _SNlaptop = value; } }
		public List<string> SNlcd22 { get { return _SNlcd22; } set { _SNlcd22 = value; } }
		public List<string> SNlcd27 { get { return _SNlcd27; } set { _SNlcd27 = value; } }
		public List<string> SNnip { get { return _SNnip; } set { _SNnip = value; } }
		public List<string> SNrecu { get { return _SNrecu; } set { _SNrecu = value; } }
	}

	public class Options
	{
		private int _serverPort = 1026;
		public int serverPort { get { return _serverPort; } set { _serverPort = value; } }
	}
}
