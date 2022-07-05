using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace entrepotServer
{
	public class Program
	{
		//static X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String("server.pfx", password));
		public X509Certificate2 cert;
		static string password;
		public TcpListener serverSocket;
		public bool running = true;
		public Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();  // Information about users + connections info.
		public UserInfo userInfo = new UserInfo();
		public Options options = new Options();
		public static AppData appData = new AppData();
		string year = DateTime.Now.Year.ToString();
		CultureInfo ci = new CultureInfo("fr-FR");
		public const byte IM_ForceQuit = 96;
		public const byte IM_NewPreparation = 95;
		public const byte IM_WBdate= 100;

		string pathWay = @".\waybills\";
		string backupFolder = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"backup" + Path.DirectorySeparatorChar;

		public bool IsAliveRunning = false;
		private static readonly Object lockThis = new Object();
		private static readonly Object lockSave = new Object();
		private static readonly Object LOCKLOG = new object();
		bool exited = false;
		public static bool runAPI = false;

		static void Main(string[] args)
		{
			ProtectSection();
			Program p = new Program();
			p.Start(p);

			Console.WriteLine();
			Console.WriteLine("Press enter to close program.");
			Console.ReadLine();
		}

		//Reads a file.
		internal static byte[] ReadFile(string fileName)
		{
			using FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			int size = (int)f.Length;
			byte[] data = new byte[size];
			size = f.Read(data, 0, size);
			f.Close();
			return data;
		}

		static public void ProtectSection()
		{
			// Get the current configuration file.
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			// Get the section.
			ConfigurationSection section = config.GetSection("appSettings");
			password = "instant";

			// Protect (encrypt)the section.
			//  section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");

			// Save the encrypted section.
			//   section.SectionInformation.ForceSave = true;

			//   config.Save(ConfigurationSaveMode.Full);

			// Display decrypted configuration  
			// section. Note, the system 
			// uses the Rsa provider to decrypt 
			// the section transparently. 
			//string sectionXml = section.SectionInformation.GetRawXml();

			//Console.WriteLine("Decrypted section:");
			//Console.WriteLine(sectionXml);
			//  password = config.AppSettings.Settings["CertificatePassword"].Value;

		}

		public void Start(Program p)
		{
			System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += ctx =>
			{
				if (!exited)
				{
					Console.WriteLine("Unloading fired");
					SaveUsers();
				}
			};

			Console.Clear();
			// Establish an event handler to process key press events.
			Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

			cert = new X509Certificate2("server.pfx", password);

			checkFolder();
			LoadUsers();
			//LoadModel();
			msg("Loading database ...");
			LoadDatabase();

			msg("Loading Modeles ...");
			LoadModel();
			//msg("Loading Inventaire Desjardins ...");
			//LoadingDesjardins();
			//msg("Loading Inventaire Desjardins NIP ...");
			//LoadingNip();
			//msg("Loading Seuils ...");
			//LoadSeuil();
			msg("Loading Waybills ...");
			LoadWaybills();
			SetYearList();
			msg("Loading Logs ...");
			LoadLog();
			//LoadPreparation();
			//UpdateColor();
			//nipAsset();
			//UpdateRepair();
			UpdateColor();
			//Task.Run(WatcherTask);
			//msg("File Watcher Task Started");

			Task.Run(checkTime); // thread pour checker le time (rapport fpt, puro API, autoreset)
			Task.Run(SaveBackup); // thread pour les backups
			msg("Backup / Auto Rapport are running ...");

			//fixWB();
			//Create the server side connection And 
			//Start listening for clients.
			serverSocket = new TcpListener(IPAddress.Any, options.serverPort);
			msg("---Server Started--- ");
			msg("Server is listening on port: " + options.serverPort);
			msg("Waiting for a connection ...");
			serverSocket.Start();
			msg("Server is running properly!");
			Listen();

			void Listen()
			{
				while (running)
				{
					TcpClient tcpClient = serverSocket.AcceptTcpClient();  // Accept incoming connection.
					tcpClient.NoDelay = true;
					Client client = new Client(this, tcpClient);     // Handle in another thread.
				}
			}
		}

		//private void fixWB()
		//      {
		//	foreach (var item in appData.waybills.ToArray())
		//          {
		//		if (!string.IsNullOrEmpty(item.wayb))
		//              {
		//			item.wayb = item.wayb.Replace("-", "");
		//			item.wayb = item.wayb.Replace(" ", "");
		//		}

		//		if (!string.IsNullOrEmpty(item.wbRetour))  
		//              {
		//			item.wbRetour = item.wbRetour.Replace("-", "");
		//			item.wbRetour = item.wbRetour.Replace(" ", "");
		//		}
		//          }
		//      }

		//public async Task puroTracking()
		//{
		//	runAPI = true;
		//	msg("Puro tracking started.");

		//	string value = "";
		//	var month = DateTime.Now.ToString("MMMM", ci);
		//	string oldMonth;
		//	string anner = "";
		//	List<Waybills> temp = new List<Waybills>();

		//	if (month == "janvier")
		//	{
		//		oldMonth = "décembre";
		//		anner = DateTime.Now.AddYears(-1).ToString();
		//		string path = @".\waybills\" + anner + ".wb";

		//		try
		//		{
		//			XmlSerializer xs = new XmlSerializer(typeof(List<Waybills>));
		//			using (StreamReader rd = new StreamReader(path))
		//			{
		//				temp = xs.Deserialize(rd) as List<Waybills>;
		//			}

		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine(ex.Message);
		//		}
		//	}
		//	else oldMonth = DateTime.Now.AddMonths(-1).ToString("MMMM", ci);

		//	string[] waybillSortie;
		//	string[] waybillRetour;
		//	string[] split;
		//	string newData;

		//	foreach (var item in appData.waybills.ToArray())
		//	{
		//		if (item.mois == month || item.mois == oldMonth)
		//		{
		//			if (!string.IsNullOrEmpty(item.wayb))
		//			{
		//				waybillSortie = item.wayb.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		//				for (int i = 0; i < waybillSortie.Count(); i++)
		//				{
		//					split = waybillSortie[i].Split(" - ");

		//					if (split.Count() == 1)
		//					{
		//						value = await TrackingAPI(split[0]);
		//						if (!string.IsNullOrEmpty(value)) waybillSortie[i] = split[0] + " - " + value;
		//					}
		//					else if (split[1].ToLower() != "shipment delivered")
		//					{
		//						value = await TrackingAPI(split[0]);
		//						if (!string.IsNullOrEmpty(value)) waybillSortie[i] = split[0] + " - " + value;
		//					}
		//				}

		//				newData = string.Join(Environment.NewLine, waybillSortie);

		//				if (item.wayb != newData)
		//				{
		//					lock (appData.lockWB)
		//					{
		//						item.wayb = newData;
		//					}
		//				}
		//			}

		//			if (!string.IsNullOrEmpty(item.wbRetour))
		//			{
		//				waybillRetour = item.wbRetour.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		//				for (int i = 0; i < waybillRetour.Count(); i++)
		//				{
		//					split = waybillRetour[i].Split(" - ");

		//					if (split.Count() == 1)
		//					{
		//						value = await TrackingAPI(split[0]);
		//						if (!string.IsNullOrEmpty(value)) waybillRetour[i] = split[0] + " - " + value;
		//					}
		//					else if (split[1].ToLower() != "shipment delivered")
		//					{
		//						value = await TrackingAPI(split[0]);
		//						if (!string.IsNullOrEmpty(value)) waybillRetour[i] = split[0] + " - " + value;
		//					}
		//				}

		//				newData = string.Join(Environment.NewLine, waybillRetour);

		//				if (item.wbRetour != newData)
		//				{
		//					lock (appData.lockWB)
		//					{
		//						item.wbRetour = newData;
		//					}
		//				}
		//			}
		//		}
		//	}

		//	SaveWB();

		//	if (anner != "")
		//	{
		//		foreach (var item in temp.ToArray())
		//		{
		//			if (item.mois == oldMonth)
		//			{
		//				if (!string.IsNullOrEmpty(item.wayb))
		//				{
		//					waybillSortie = item.wayb.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		//					for (int i = 0; i < waybillSortie.Count(); i++)
		//					{
		//						split = waybillSortie[i].Split(" - ");

		//						if (split.Count() == 1)
		//						{
		//							value = await TrackingAPI(split[0]);
		//							if (!string.IsNullOrEmpty(value)) waybillSortie[i] = split[0] + " - " + value;
		//						}
		//						else if (split[1].ToLower() != "shipment delivered")
		//						{
		//							value = await TrackingAPI(split[0]);
		//							if (!string.IsNullOrEmpty(value)) waybillSortie[i] = split[0] + " - " + value;
		//						}
		//					}

		//					newData = string.Join(Environment.NewLine, waybillSortie);

		//					if (item.wayb != newData)
		//					{
		//						lock (appData.lockWB)
		//						{
		//							item.wayb = newData;
		//						}
		//					}
		//				}

		//				if (!string.IsNullOrEmpty(item.wbRetour))
		//				{
		//					waybillRetour = item.wbRetour.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		//					for (int i = 0; i < waybillRetour.Count(); i++)
		//					{
		//						split = waybillRetour[i].Split(" - ");

		//						if (split.Count() == 1)
		//						{
		//							value = await TrackingAPI(split[0]);
		//							if (!string.IsNullOrEmpty(value)) waybillRetour[i] = split[0] + " - " + value;
		//						}
		//						else if (split[1].ToLower() != "shipment delivered")
		//						{
		//							value = await TrackingAPI(split[0]);
		//							if (!string.IsNullOrEmpty(value)) waybillRetour[i] = split[0] + " - " + value;
		//						}
		//					}

		//					newData = string.Join(Environment.NewLine, waybillRetour);

		//					if (item.wbRetour != newData)
		//					{
		//						lock (appData.lockWB)
		//						{
		//							item.wbRetour = newData;
		//						}
		//					}
		//				}
		//			}
		//		}

		//		SaveWBold(temp, anner);
		//	}

		//	appData.dateWB = DateTime.Now.ToString();

		//	lock (appData.lockWrite)
		//	{
		//		foreach (var UserKey in users.Keys)
		//		{
		//			if (users.TryGetValue(UserKey, out UserInfo user))
		//			{
		//				if (user.LoggedIn)
		//				{
		//					user.Connection.bw.Write(IM_WBdate);
		//					user.Connection.bw.Write(appData.dateWB);
		//					user.Connection.bw.Flush();
		//				}
		//			}
		//		}
		//	}

		//	try
		//	{
		//		File.WriteAllText(@".\config\puroTracking.update", appData.dateWB);
		//	}
		//	catch { }

		//	msg("Puro tracking ended.");
		//	runAPI = false;
		//}

		//private async Task<string> TrackingAPI(string waybill)
		//{
		//	string result;
		//	string sEnv;
		//	string info = "";
		//	string date = "";
		//	string url = "https://webservices.purolator.com/EWS/V1/Tracking/TrackingService.asmx";
		//	string urlSoap = "http://purolator.com/pws/service/v1/TrackPackagesByPin";
		//	var byteArray = Encoding.ASCII.GetBytes("cc983bd6d2b146a8847e5e12a6aa0a5d:|h+JH#D6");

		//	try
		//	{
		//		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
		//		HttpContent httpContent;
		//		HttpResponseMessage response;
		//		HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);
		//		req.Headers.Add("SOAPAction", urlSoap);
		//		req.Method = HttpMethod.Post;

		//		sEnv = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v1=""http://purolator.com/pws/datatypes/v1"">" +
		//			   "<soapenv:Header><v1:RequestContext><v1:Version>1.2</v1:Version><v1:Language>en</v1:Language><v1:GroupID>?</v1:GroupID><v1:RequestReference>?</v1:RequestReference>" +
		//			   "<!--Optional:--><v1:UserToken>?</v1:UserToken></v1:RequestContext></soapenv:Header><soapenv:Body><v1:TrackPackagesByPinRequest><v1:PINs><!--Zero or more repetitions:-->" +
		//			   "<v1:PIN><v1:Value>" + waybill + "</v1:Value></v1:PIN></v1:PINs></v1:TrackPackagesByPinRequest></soapenv:Body></soapenv:Envelope>";

		//		httpContent = new StringContent(sEnv);
		//		req.Content = httpContent;
		//		req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");
		//		response = await httpClient.SendAsync(req);

		//		using (XmlReader reader = XmlReader.Create(new StringReader(await response.Content.ReadAsStringAsync())))
		//		{
		//			while (reader.Read())
		//			{
		//				if (reader.IsStartElement())
		//				{
		//					//return only when you have START tag  
		//					if (reader.Name.ToString() == "ScanDate" && date == "")
		//					{
		//						date = reader.ReadString();
		//					}

		//					if (reader.Name.ToString() == "Description")
		//					{
		//						info = reader.ReadString();
		//						break;
		//					}
		//				}
		//			}
		//		}

		//		if (info.ToLower().Contains("invalid pin") || info.ToLower().Contains("no scanning details") || string.IsNullOrEmpty(info)) result = "Information non disponible";
		//		else result = info + " " + date;

		//		return result;
		//	}
		//	catch (Exception ex)
		//	{
		//		msg("Tracking Puro (" + waybill + "): " + ex.Message);
		//		return "";
		//	}
		//}

		//private void UpdateRepair()
		//      {
		//	foreach (var item in appData.invPostes.ToArray())
		//          {
		//		if (item.emplacement == "REPAIR DEPOT")
		//              {
		//			if (item.RFretour == "ENTR COMP M")
		//                  {
		//				if (!string.IsNullOrEmpty(item.dateRetour)) item.RFretour = "N/A";
		//				else item.RFretour = "";

		//				item.emplacement = "REPAIR DEPOT ENTR COMP M";
		//			}

		//			if (item.RFretour == "ENTR C" || item.RFretour == "ENTREPOT CENT.")
		//			{
		//				if (!string.IsNullOrEmpty(item.dateRetour)) item.RFretour = "N/A";
		//				else item.RFretour = "";

		//				item.emplacement = "REPAIR DEPOT ENTR C";
		//			}

		//			if (item.RFretour == "LÉVIS" || item.RFretour == "LEVIS")
		//			{
		//				if (!string.IsNullOrEmpty(item.dateRetour)) item.RFretour = "N/A";
		//				else item.RFretour = "";

		//				item.emplacement = "REPAIR DEPOT LÉVIS";
		//			}
		//		}
		//          }
		//      }

		private void UpdateColor()
		{
			lock (appData.lockDB)
			{
				var date = DateTime.Now;
				DateTime expire;

				foreach (var item in appData.invPostes.ToArray())
				{
					if (!string.IsNullOrEmpty(item.dateCloneValid) && item.emplacement != "QUANTUM" && item.type == "Serveur" && item.statut == "En Stock")
					{
						var array = item.dateCloneValid.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
						expire = DateTime.Parse(array[array.Count() - 1]);

						if ((expire - date).TotalDays <= 15 && !((expire - date).TotalDays <= 5)) item.xcolor = "1";
						else if ((expire - date).TotalDays <= 5) item.xcolor = "2";
						else if (expire < date) item.xcolor = "2";
						else item.xcolor = "";
					}

					if (item.type == "Serveur" && (item.statut == "Sortie" || item.emplacement == "QUANTUM")) item.xcolor = "";
				}

				SaveDatabase();
			}
		}


		private protected void myHandler(object sender, ConsoleCancelEventArgs args)
		{
			Console.WriteLine($"  Key pressed: {args.SpecialKey}");
			Console.WriteLine("The Server will save data and exit.\n");
			KickAllClient();
			SaveDatabase();
			SaveUsers();
			SaveModelPoste();
			SaveModelLaptop();
			SaveModelServeur();
			exited = true;
			Environment.Exit(0);
		}

		public void SaveUsers()  // Save users data
		{
			lock (lockThis)
			{
				string usersFileName = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"config" + Path.DirectorySeparatorChar + @"users.dat";
				string historyFileName = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"config" + Path.DirectorySeparatorChar + @"history.dat";
				string optionsFileName = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"config" + Path.DirectorySeparatorChar + @"options.cfg";

				try
				{
					msg("Saving options...");
					var jsonString = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(optionsFileName, jsonString);
					msg("Options saved!");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}

				try
				{
					msg("Saving users...");

					var jsonString = JsonSerializer.Serialize(users.Values.ToArray(), new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(usersFileName, jsonString);
					msg("Users saved!");
					//BinaryFormatter bf = new BinaryFormatter();
					//using (FileStream file = new FileStream(usersFileName, FileMode.Create, FileAccess.Write))
					//{
					//    bf.Serialize(file, users.Values.ToArray());  // Serialize UserInfo array
					//    file.Close();
					//}
				}

				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}
			}
		}

		private void SaveBackup()
        {
			AutoResetEvent autoResetEvent = new AutoResetEvent(false);
			autoResetEvent.WaitOne(900000);
			int timeCompare;
			string day;

			while (true)
			{
				timeCompare = int.Parse(DateTime.Now.ToString("H:mm").Replace(":", ""));
				day = DateTime.Now.ToString("dddd", ci);

				if ((timeCompare > 700 && timeCompare < 2300) && (day != "samedi" && day != "dimanche"))
				{
					SaveDatabaseBackup();
					//SaveWBbackup();
					wbBackup();
					AutoRapport(appData.invPostes, "Entrepot Sobeys.xlsx");
					autoResetEvent.WaitOne(7200000);// backup a chaque 2 heurs
				}
				else autoResetEvent.WaitOne(300000); // recheck au 5 min si le time est ok pour saver backup
			}
		}

		private void checkTime()
		{
			AutoResetEvent autoResetEvent = new AutoResetEvent(false);
			autoResetEvent.WaitOne(120000);
			int timeCompare;
			string day;

			while (true)
			{
				timeCompare = int.Parse(DateTime.Now.ToString("H:mm").Replace(":", ""));
				day = DateTime.Now.ToString("dddd", ci);

				if (timeCompare >= 300 && timeCompare < 305)
				{
					KickAllClient();
					UpdateColor();

					if (DateTime.Now.Year.ToString() != year)
					{
						SaveWB();
						year = DateTime.Now.Year.ToString();
						appData.waybills.Clear();
						SaveWB();
						SetYearList();
						LoadWaybills();
					}
					
					//UpdateRepair();
				}

                if (day != "samedi" && day != "dimanche")
                {
                    //if (timeCompare >= 830 && timeCompare < 835) _ = puroTracking();
                    if (timeCompare >= 1520 && timeCompare < 1525) Task.Run(RapportFTP);
                }

                autoResetEvent.WaitOne(300000); // recheck au 5 min si le time est ok
            }
        }

        //private void test()
        //{
        //    var today = DateTime.Now.ToShortDateString();

        //    foreach (var item in appData.invPostes.ToArray())
        //    {
        //        if (item.dateEntry == today || (item.dateSortie.Contains(today) && item.statut == "Sortie")) appData.jour.Add(item);
        //    }

        //    AutoRapport(appData.jour, "RapportEntrerSortie.xlsx");

        //    AutoRapport(appData.invPostes, "Rapport.xlsx");
        //}

        private void RapportFTP()
        {
            var today = DateTime.Now.ToShortDateString();

			InvPostes[] temp;
			List<InvPostes> jour = new List<InvPostes>();

			lock (appData.lockDB)
            {
				temp = appData.invPostes.ToArray();
            }

            foreach (var item in temp)
            {
                if (item.dateSortie == today && item.statut == "Sortie") jour.Add(item);
            }

            foreach (var item in jour.ToArray())
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

            AutoRapport(jour, "RapportSortieSobeys.xlsx");
            //_ = uploadFTP();
        }

        private async Task uploadFTP()
        {
            await Task.Delay(2000);

            int count = 0;
            string path = @".\Rapport Auto\RapportSortieSobeys.xlsx";

            if (!File.Exists(path))
            {
                msg("RapportSortieSobeys.xlsx don't exist, FTP upload cancelled.");
                return;
            }

            msg("Attempting connection to FTP server to send the report ...");

            while (true)
            {
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.getserveur.com/RapportSortieSobeys.xlsx");
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    //request.EnableSsl = true;
                    request.UsePassive = false;
                    request.KeepAlive = false;
                    request.Credentials = new NetworkCredential("dan@getserveur.com", "#isHN$W}QV)@");

                    //ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateCertificate);
                    msg("Connected to FTP server.");
                    // Copy the contents of the file to the request stream.
                    await using FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
                    await using Stream requestStream = request.GetRequestStream();
                    msg("Sending file ...");
                    await fileStream.CopyToAsync(requestStream);
                    msg("File transfer completed.");
                    //using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    //msg($"Upload File Complete, status {response.StatusDescription}");
                    break;
                }
                catch (Exception ex)
                {
                    count++;
                    msg(ex.Message);

                    if (count == 5)
                    {
                        msg("FTP upload cancelled: failed 5 times.");
                        break;
                    }

                    await Task.Delay(5000);
                }
            }
        }

        //static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        //{
        //    return true;
        //}

        private void KickAllClient()
		{
			foreach (var UserKey in users.Keys)
			{
				if (users.TryGetValue(UserKey, out UserInfo user))
				{
					if (user.LoggedIn)
					{
						user.Connection.bw.Write(IM_ForceQuit);
						user.Connection.bw.Flush();
					}
				}
			}
		}

		public void wbBackup()
		{
			lock (lockSave)
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Waybills>));
					using (StreamWriter wr = new StreamWriter(backupFolder + year + ".backup"))
					{
						xs.Serialize(wr, appData.waybills);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void SetYearList()
		{
			List<string> temp = new List<string>(); ;

			try
			{
				temp = Directory.EnumerateFiles(pathWay, "*.*", SearchOption.AllDirectories).ToList();

				if (temp.Count == 0 )
				{
					SaveWB();
					temp = Directory.EnumerateFiles(pathWay, "*.*", SearchOption.AllDirectories).ToList();
				}
			}
			catch (Exception ex)
			{
				msg(ex.Message);
			}

			string name = "";

			foreach (var item in temp.ToArray())
			{
				name = Path.GetFileNameWithoutExtension(item);
				appData.yearList.Add(name);
			}			
		}
	
		public void AutoRapport(List<InvPostes> list, string name)
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

				ws.Cell(2, 1).InsertData(list);

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
				ws.Cells().Style.Alignment.WrapText = true;
				ws.Columns().AdjustToContents();
				//ws.Rows().AdjustToContents();
				ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
				//wb.SaveAs(@"T:\Rapport Auto\" + name);
				////wb.SaveAs(@".\Rapport Auto\" + name);
				//wb.SaveAs(@".\Rapport Auto\" + name);
				//wb.SaveAs(@"C:\test\" + name);

				if (name == "Entrepot Sobeys.xlsx") wb.SaveAs(@"C:\Users\inventaire\CompuCom\Projet Sobeys - Rapport\" + name);
				else wb.SaveAs(@".\Rapport Auto\" + name);
			}
			catch (Exception ex)
			{
				msg(ex.Message);
			}
		}

		public void SaveLog()
		{
			lock (lockSave)
			{
				//File.WriteAllLines(@".\Logs\logs.txt", appData.logs);

				try
				{
					var jsonString = JsonSerializer.Serialize(appData.logsRapport, new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(@".\Logs\logs.txt", jsonString);

				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			}
		}

		public void LoadLog()
		{
			if (!File.Exists(@".\Logs\logs.txt")) return;

			try
			{
				var jsonString = File.ReadAllText(@".\Logs\logs.txt");
				appData.logsRapport = JsonSerializer.Deserialize<List<LogsRapport>>(jsonString, new JsonSerializerOptions { WriteIndented = true });

				msg("Logs Loaded.");
			}
			catch
			{
				msg("Erreur de lecture des Logs");
			}
		}

		public void SaveDatabaseBackup()
		{
			lock (lockSave)
			{
				string timeCheck = backupFolder + "Backup " + DateTime.Now.ToString("dd-MM-yyyy H;mm;ss") + ".db" ;
				string jsonString = "";

				try
				{
					jsonString = JsonSerializer.Serialize(appData.invPostes);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}

				if (jsonString != "")
				{
					try
					{
						File.WriteAllText(timeCheck, jsonString);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						Console.WriteLine(ex.StackTrace);
					}

					try
					{
						File.WriteAllText(@"C:\Users\inventaire\CompuCom\Projet Sobeys - Backup\database.db", jsonString);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						Console.WriteLine(ex.StackTrace);
					}

					msg("New backup saved (DB).");
				}
			}
		}

		//public void SaveWBbackup()
		//{
		//	lock (lockSave)
		//	{
		//		try
		//		{
		//			XmlSerializer xs = new XmlSerializer(typeof(List<Waybills>));
		//			using (StreamWriter wr = new StreamWriter(@"T:\Rapport Auto\backup\" + year + ".wb"))
		//			{
		//				xs.Serialize(wr, appData.waybills);
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine(ex.Message);
		//		}

		//		msg("New backup saved (WB).");
		//	}
		//}

		//public void SaveInventaireDesjardins()
		//{
		//	lock (lockSave)
		//	{
		//		string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"invDesjardins.xml";

		//		try
		//		{
		//			XmlSerializer xs = new XmlSerializer(typeof(List<Achat>));
		//			using (StreamWriter wr = new StreamWriter(path))
		//			{
		//				xs.Serialize(wr, appData.fichierAchat);
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine(ex.Message);
		//		}
		//	}
		//}

		public void SaveDatabase()
		{
			lock (lockSave)
			{
				string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"database.db";

				try
				{
					var jsonString = JsonSerializer.Serialize(appData.invPostes, new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(path, jsonString);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			}
		}

		public void SaveModelPoste()
		{
			lock (lockSave)
			{
				string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"poste.model";

				try
				{
					var jsonString = JsonSerializer.Serialize(appData.modelPoste, new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(path, jsonString);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			}
		}

		public void SaveModelLaptop()
		{
			lock (lockSave)
			{
				string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"laptop.model";

				try
				{
					var jsonString = JsonSerializer.Serialize(appData.modelPortable, new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(path, jsonString);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			}
		}

		public void SaveModelServeur()
		{
			lock (lockSave)
			{
				string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"serveur.model";

				try
				{
					var jsonString = JsonSerializer.Serialize(appData.modelServeur, new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(path, jsonString);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			}
		}

		public void LoadModel()
		{
			string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"poste.model";
			string path2 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"laptop.model";
			string path3 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"serveur.model";

			try
			{
				var jsonString = File.ReadAllText(path);
				appData.modelPoste = JsonSerializer.Deserialize<List<string>>(jsonString, new JsonSerializerOptions { WriteIndented = true });
				msg("Poste model correctly.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				msg("Error loading poste model!");
			}

			try
			{
				var jsonString = File.ReadAllText(path2);
				appData.modelPortable = JsonSerializer.Deserialize<List<string>>(jsonString, new JsonSerializerOptions { WriteIndented = true });
				msg("Laptop model correctly.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				msg("Error loading laptop model!");
			}

			try
			{
				var jsonString = File.ReadAllText(path3);
				appData.modelServeur = JsonSerializer.Deserialize<List<string>>(jsonString, new JsonSerializerOptions { WriteIndented = true });
				msg("Serveur model correctly.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				msg("Error loading serveur model!");
			}
		}

		public void LoadDatabase()
		{
			string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"database.db";

			if (!File.Exists(path)) return;

			try
			{
				var jsonString = File.ReadAllText(path);
				appData.invPostes = JsonSerializer.Deserialize<List<InvPostes>>(jsonString, new JsonSerializerOptions { WriteIndented = true });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
				msg("Database loading error !");
			}

			ModelList();
			msg("Databaseloaded correctly.");
		}

		public void ModelList()
		{
			List<string> type = new List<string>();

			foreach (var item in appData.invPostes.ToArray())
			{
				if (item.type.Contains("�")) item.type = item.type.Replace("�", "ç");

				if (!type.Contains(item.type)) 
				{			
					type.Add(item.type);
					appData.typesModels.Add(new TypeModel { type = item.type });
				}

				foreach (var model in appData.typesModels)
				{
					if (model.type == item.type && !model.modeles.Contains(item.model)) model.modeles.Add(item.model);
				}
			}
		}

		//public void LoadingDesjardins()
		//{
		//	string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"invDesjardins.xml";

		//	if (!File.Exists(path)) return;

		//	try
		//	{
		//		XmlSerializer xs = new XmlSerializer(typeof(List<Achat>));
		//		using (StreamReader rd = new StreamReader(path))
		//		{
		//			appData.fichierAchat = xs.Deserialize(rd) as List<Achat>;
		//		}

		//		msg("---Fichier Desjardins Loaded---");
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(ex.Message);
		//	}
		//}

		public void LoadWaybills()
		{
			if (!File.Exists(pathWay + year + ".wb")) SaveWB();

			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(List<Waybills>));
				using (StreamReader rd = new StreamReader(pathWay + year + ".wb"))
				{
					appData.waybills = xs.Deserialize(rd) as List<Waybills>;
				}

				msg("---Waybills Loaded--- ");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void SaveWB()
		{
			lock (lockSave)
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Waybills>));
					using (StreamWriter wr = new StreamWriter(pathWay + year + ".wb"))
					{
						xs.Serialize(wr, appData.waybills);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void SaveWBold(List<Waybills> temp, string oldYear)
		{
			lock (lockSave)
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Waybills>));
					using (StreamWriter wr = new StreamWriter(pathWay + oldYear + ".wb"))
					{
						xs.Serialize(wr, temp);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		//public void SaveModelMoniteur()
		//{
		//	lock (lockSave)
		//	{
		//		try
		//		{
		//			XmlSerializer xs = new XmlSerializer(typeof(List<string>));
		//			using (StreamWriter wr = new StreamWriter(path3))
		//			{
		//				xs.Serialize(wr, appData.modeleMoniteur);
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine(ex.Message);
		//		}
		//	}
		//}

		//public void LoadModel()
		//{
		//	if (File.Exists(path3))
		//	{
		//		try
		//		{
		//			XmlSerializer xs = new XmlSerializer(typeof(List<string>));
		//			using (StreamReader rd = new StreamReader(path3))
		//			{
		//				appData.modeleMoniteur = xs.Deserialize(rd) as List<string>;
		//			}

		//			msg("---Modele moniteurs loaded--- ");
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine(ex.Message);
		//		}
		//	}
		//}

		public void LoadUsers()  // Load users data
		{
			string usersFileName = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"config" + Path.DirectorySeparatorChar + @"users.dat";
			string historyFileName = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"config" + Path.DirectorySeparatorChar + @"history.dat";
			string optionsFileName = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"config" + Path.DirectorySeparatorChar + @"options.cfg";
			try
			{
				msg("Loading options...");
				var jsonString = File.ReadAllText(optionsFileName);
				options = JsonSerializer.Deserialize<Options>(jsonString, new JsonSerializerOptions { WriteIndented = true });
				msg("Options loaded!");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}

			try
			{
				if (File.Exists(@".\config\puroTracking.update")) appData.dateWB = File.ReadAllText(@".\config\puroTracking.update");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}

			try
			{
				msg("Loading users...");
				var jsonString = File.ReadAllText(usersFileName);
				UserInfo[] infos = JsonSerializer.Deserialize<UserInfo[]>(jsonString, new JsonSerializerOptions { WriteIndented = true });
				users = infos.ToDictionary((u) => u.UserName, (u) => u);  // Convert UserInfo array to Dictionary

				//users = infos.ToDictionary((u) => u.UserName, (u) => u);  // Convert UserInfo array to Dictionary
				//BinaryFormatter bf = new BinaryFormatter();
				//using (FileStream file = new FileStream(usersFileName, FileMode.Open, FileAccess.Read))
				//{
				//    UserInfo[] infos = (UserInfo[])bf.Deserialize(file);      // Deserialize UserInfo array
				//    file.Close();
				//    users = infos.ToDictionary((u) => u.UserName, (u) => u);  // Convert UserInfo array to Dictionary
				//}
				msg("Users loaded! (" + users.Count + ")");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}

			// check file-----------------------------------

			//-----------------------------------------
		}

		private void checkFolder()
		{
			string folder = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"config";
			string folder2 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database";
			string folder3 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"backup";
			//string folder4 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"fichier inventaire";
			string folder5 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"Rapport Auto";
			string folder6 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"Logs";
			string folder7 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"waybills";
			//string folder8 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"seuils";

			if (!Directory.Exists(folder))
			{
				try
				{
					Directory.CreateDirectory(folder);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}

			if (!Directory.Exists(folder2))
			{
				try
				{
					Directory.CreateDirectory(folder2);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}

			if (!Directory.Exists(folder3))
			{
				try
				{
					Directory.CreateDirectory(folder3);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}

			//if (!Directory.Exists(folder4))
			//{
			//	try
			//	{
			//		Directory.CreateDirectory(folder4);
			//	}
			//	catch (Exception ex)
			//	{
			//		Console.WriteLine(ex.Message);
			//		return;
			//	}
			//}

			if (!Directory.Exists(folder5))
			{
				try
				{
					Directory.CreateDirectory(folder5);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}

			if (!Directory.Exists(folder6))
			{
				try
				{
					Directory.CreateDirectory(folder6);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}

			if (!Directory.Exists(folder7))
			{
				try
				{
					Directory.CreateDirectory(folder7);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}

			//if (!Directory.Exists(folder8))
			//{
			//	try
			//	{
			//		Directory.CreateDirectory(folder8);
			//	}
			//	catch (Exception ex)
			//	{
			//		Console.WriteLine(ex.Message);
			//		return;
			//	}
			//}
		}

		public void msg(string mesg)
		{
			mesg.Trim();
			var message = DateTime.Now + " >> " + mesg;
			string LogPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"config" + Path.DirectorySeparatorChar + @"logs.log";
			Console.WriteLine(message);
			// This text is always added, making the file longer over time
			// if it is not deleted.
			lock (LOCKLOG)
			{
				using (StreamWriter sw = File.AppendText(LogPath))
				{
					sw.WriteLine(message);
				}
			}
		}
	}
}
