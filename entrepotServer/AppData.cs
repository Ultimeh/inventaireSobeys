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

		public List<InvPostes> invPostes { get { return _invPostes; } set { _invPostes = value; } }

		//public List<string> logs = new List<string>();


		private List<LogsRapport> _logsRapport = new List<LogsRapport>();
		public List<LogsRapport> logsRapport { get { return _logsRapport; } set { _logsRapport = value; } }



		public List<string> yearList = new List<string>();


		//public List<string> preparationSemaine = new List<string>();

		//private List<Achat> _tempAchat = new List<Achat>();
		//public List<Achat> tempAchat { get { return _tempAchat; } set { _tempAchat = value; } }

		private List<Achat> _fichierAchat = new List<Achat>();
		public List<Achat> fichierAchat { get { return _fichierAchat; } set { _fichierAchat = value; } }

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

		private List<string> _modelPoste = new List<string>();
		public List<string> modelPoste { get { return _modelPoste; } set { _modelPoste = value; } }

		private List<string> _modelPortable = new List<string>();
		public List<string> modelPortable { get { return _modelPortable; } set { _modelPortable = value; } }

		private List<string> _modelServeur = new List<string>();
		public List<string> modelServeur { get { return _modelServeur; } set { _modelServeur = value; } }

		private List<string> _modelRDX = new List<string>();
		public List<string> modelRDX { get { return _modelRDX; } set { _modelRDX = value; } }

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
		private string _type = "";
		private string _model = "";
		private string _magasin = "";
		private string _serial = "";
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
		public string magasin { get { return _magasin; } set { _magasin = value; } }
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

	public class Options
	{
		private int _serverPort = 1026;
		public int serverPort { get { return _serverPort; } set { _serverPort = value; } }
	}
}
