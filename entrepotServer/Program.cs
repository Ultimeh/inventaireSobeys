﻿using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
		bool watcherFound = false;
		public const byte IM_ForceQuit = 96;
		public const byte IM_NewPreparation = 95;
		public const byte IM_WBdate= 100;
		private static HttpClient httpClient = new HttpClient();

		string pathWay = @".\waybills\";
		string path3 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"modelMoniteur.xml";
		string backupFolder = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"backup" + Path.DirectorySeparatorChar;

		public bool IsAliveRunning = false;
		private static readonly Object lockThis = new Object();
		private static readonly Object lockSave = new Object();
		private static readonly Object LOCKLOG = new object();
		bool exited = false;
		public static bool runAPI = false;

		FileSystemWatcher watcher = new FileSystemWatcher();

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
					System.Console.WriteLine("Unloading fired");
					SaveUsers();
				}
			};

			Console.Clear();
			// Establish an event handler to process key press events.
			Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

			cert = new X509Certificate2("server.pfx", password);

			checkFolder();
			LoadUsers();
			LoadModel();
			msg("Loading database ...");
			LoadDatabase();
			msg("Loading Inventaire Desjardins ...");
			LoadingDesjardins();
			msg("Loading Inventaire Desjardins NIP ...");
			LoadingNip();
			msg("Loading Seuils ...");
			LoadSeuil();
			msg("Loading Waybills ...");
			LoadWaybills();
			SetYearList();
			msg("Loading Logs ...");
			LoadLog();
			LoadPreparation();
			UpdateColor();
			nipAsset();
			UpdateRepair();

			Task.Run(WatcherTask);
			msg("File Watcher Task Started");

			Task.Run(checkTime); // thread pour checker le time (rapport fpt, puro API, autoreset)
			Task.Run(SaveBackup); // thread pour les backups
			msg("Backup / Auto Rapport / FTP Tasks are running ...");

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

		public async Task puroTracking()
        {
			runAPI = true;
			msg("Puro tracking started.");

			var month = DateTime.Now.ToString("MMMM", ci);
			string oldMonth;
			string anner = "";
			List<Waybills> temp = new List<Waybills>();

			if (month == "janvier") 
            {
				oldMonth = "décembre";
				anner = DateTime.Now.AddYears(-1).ToString();		
				string path = @".\waybills\" + anner + ".wb";

				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Waybills>));
					using (StreamReader rd = new StreamReader(path))
					{
						temp = xs.Deserialize(rd) as List<Waybills>;
					}

				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			else oldMonth = DateTime.Now.AddMonths(-1).ToString("MMMM", ci);

			string[] waybillSortie;
			string[] waybillRetour;
			string[] split;
			string newData;

            foreach (var item in appData.waybills.ToArray())
            {
                if (item.mois == month || item.mois == oldMonth)
                {
                    if (!string.IsNullOrEmpty(item.wayb))
                    {
                        waybillSortie = item.wayb.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        for (int i = 0; i < waybillSortie.Count(); i++)
                        {
                            split = waybillSortie[i].Split(" - ");

                            if (split.Count() == 1) waybillSortie[i] = split[0] + " - " + await TrackingAPI(split[0]);
                            else if (!split[1].ToLower().Contains("shipment delivered")) waybillSortie[i] = split[0] + " - " + await TrackingAPI(split[0]);
                        }

                        newData = string.Join(Environment.NewLine, waybillSortie);

                        if (item.wayb != newData)
                        {
                            lock (appData.lockWB)
                            {
                                item.wayb = newData;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(item.wbRetour))
                    {
                        waybillRetour = item.wbRetour.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        for (int i = 0; i < waybillRetour.Count(); i++)
                        {
                            split = waybillRetour[i].Split(" - ");

                            if (split.Count() == 1) waybillRetour[i] = split[0] + " - " + await TrackingAPI(split[0]);
                            else if (!split[1].ToLower().Contains("shipment delivered")) waybillRetour[i] = split[0] + " - " + await TrackingAPI(split[0]);
                        }

                        newData = string.Join(Environment.NewLine, waybillRetour);

                        if (item.wbRetour != newData)
                        {
                            lock (appData.lockWB)
                            {
                                item.wbRetour = newData;
                            }
                        }
                    }
                }
            }

            SaveWB();

            if (anner != "")
            {
                foreach (var item in temp.ToArray())
				{
					if (item.mois == oldMonth)
					{
						if (!string.IsNullOrEmpty(item.wayb))
						{
							waybillSortie = item.wayb.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

							for (int i = 0; i < waybillSortie.Count(); i++)
							{
								split = waybillSortie[i].Split(" - ");

								if (split.Count() == 1) waybillSortie[i] = split[0] + " - " + await TrackingAPI(split[0]);
								else if (split[1].ToLower() != "shipment delivered") waybillSortie[i] = split[0] + " - " + await TrackingAPI(split[0]);
							}

							newData = string.Join(Environment.NewLine, waybillSortie);

							if (item.wayb != newData)
							{
								lock (appData.lockWB)
								{
									item.wayb = newData;
								}
							}
						}

						if (!string.IsNullOrEmpty(item.wbRetour))
						{
							waybillRetour = item.wbRetour.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

							for (int i = 0; i < waybillRetour.Count(); i++)
							{
                                split = waybillRetour[i].Split(" - ");

                                if (split.Count() == 1) waybillRetour[i] = split[0] + " - " + await TrackingAPI(split[0]);
                                else if (split[1].ToLower() != "shipment delivered")
                                {
                                    waybillRetour[i] = split[0] + " - " + await TrackingAPI(split[0]);
                                }
                            }

                            newData = string.Join(Environment.NewLine, waybillRetour);

                            if (item.wbRetour != newData)
                            {
                                lock (appData.lockWB)
                                {
                                    item.wbRetour = newData;
                                }
                            }
                        }
                    }
                }

                SaveWBold(temp, anner);
            }

            appData.dateWB = DateTime.Now.ToString();

            lock (appData.lockWrite)
            {
				foreach (var UserKey in users.Keys)
				{
					if (users.TryGetValue(UserKey, out UserInfo user))
					{
						if (user.LoggedIn)
						{
							user.Connection.bw.Write(IM_WBdate);
							user.Connection.bw.Write(appData.dateWB);
							user.Connection.bw.Flush();
						}
					}
				}
			}

			try
			{
				File.WriteAllText(@".\config\puroTracking.update", appData.dateWB);
			}
			catch { }

			msg("Puro tracking ended.");
			runAPI = false;
		}

        private async Task<string> TrackingAPI(string waybill)
        {
            string result;
			string sEnv;
			string info = "";
			string date = "";
			string url = "https://webservices.purolator.com/EWS/V1/Tracking/TrackingService.asmx";
			string urlSoap = "http://purolator.com/pws/service/v1/TrackPackagesByPin";
			var byteArray = Encoding.ASCII.GetBytes("cc983bd6d2b146a8847e5e12a6aa0a5d:|h+JH#D6");

			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
			HttpContent httpContent;
			HttpResponseMessage response;
			HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);
			req.Headers.Add("SOAPAction", urlSoap);
			req.Method = HttpMethod.Post;

			sEnv = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:v1=""http://purolator.com/pws/datatypes/v1"">" +
				   "<soapenv:Header><v1:RequestContext><v1:Version>1.2</v1:Version><v1:Language>en</v1:Language><v1:GroupID>?</v1:GroupID><v1:RequestReference>?</v1:RequestReference>" +
				   "<!--Optional:--><v1:UserToken>?</v1:UserToken></v1:RequestContext></soapenv:Header><soapenv:Body><v1:TrackPackagesByPinRequest><v1:PINs><!--Zero or more repetitions:-->" +
				   "<v1:PIN><v1:Value>" + waybill + "</v1:Value></v1:PIN></v1:PINs></v1:TrackPackagesByPinRequest></soapenv:Body></soapenv:Envelope>";

			httpContent = new StringContent(sEnv);
			req.Content = httpContent;
			req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");
			response = await httpClient.SendAsync(req);

			using (XmlReader reader = XmlReader.Create(new StringReader(await response.Content.ReadAsStringAsync())))
			{
				while (reader.Read())
				{
					if (reader.IsStartElement())
					{
						//return only when you have START tag  
						if (reader.Name.ToString() == "ScanDate" && date == "")
						{
							date = reader.ReadString();
						}

						if (reader.Name.ToString() == "Description")
						{
							info = reader.ReadString();
							break;
						}
					}
				}
			}

			if (info.ToLower().Contains("invalid pin") || info.ToLower().Contains("no scanning details") || string.IsNullOrEmpty(info)) result = "Information non disponible";
			else result = info + " " + date;

			return result;
		}

		private void UpdateRepair()
        {
			foreach (var item in appData.invPostes.ToArray())
            {
				if (item.emplacement == "REPAIR DEPOT")
                {
					if (item.RFretour == "ENTR COMP M")
                    {
						if (!string.IsNullOrEmpty(item.dateRetour)) item.RFretour = "N/A";
						else item.RFretour = "";

						item.emplacement = "REPAIR DEPOT ENTR COMP M";
					}

					if (item.RFretour == "ENTR C" || item.RFretour == "ENTREPOT CENT.")
					{
						if (!string.IsNullOrEmpty(item.dateRetour)) item.RFretour = "N/A";
						else item.RFretour = "";

						item.emplacement = "REPAIR DEPOT ENTR C";
					}

					if (item.RFretour == "LÉVIS" || item.RFretour == "LEVIS")
					{
						if (!string.IsNullOrEmpty(item.dateRetour)) item.RFretour = "N/A";
						else item.RFretour = "";

						item.emplacement = "REPAIR DEPOT LÉVIS";
					}
				}
            }
        }

		public void nipAsset()
		{
			foreach (var item in appData.nip.ToArray())
			{
				foreach (var element in appData.invPostes.ToArray())
				{
					if (item.serial == element.serial)
					{
						element.asset = item.asset;
						break;
					}
				}
			}

			SaveDatabase();
		}

		private void WatcherTask()
		{
			AutoResetEvent autoResetEvent = new AutoResetEvent(false);

			while (true)
			{
				if (!watcherFound) StartWatcher();
				//if (watcherFound && !Directory.Exists(@"T:\Rapport Auto\Inventaire")) StartWatcher();
				if (watcherFound && !Directory.Exists(@"T:\Rapport Auto\Inventaire")) StartWatcher();

				autoResetEvent.WaitOne(5000);
			}
		}

		private void StartWatcher()
		{
			try
			{
				watcher.Path = @"T:\Rapport Auto\Inventaire";
				//watcher.Path = @"c:\test";
				watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
				//watcher.Filters.Add("inventaire.csv");
				//watcher.Filters.Add("*._nip.csv");
				watcher.Created += new FileSystemEventHandler(watcherEvent);
				watcher.Renamed += new RenamedEventHandler(watcherEvent);

				watcher.EnableRaisingEvents = true;

				watcherFound = true;
				msg("Watcher Folder found.");
			}
			catch (Exception ex)
			{
				watcherFound = false;
				msg(ex.Message);
			}
		}

		private void watcherEvent(object source, FileSystemEventArgs e)
		{
			string lien = @"T:\Rapport Auto\Inventaire\inventaire.csv";
			string lien2 = @"T:\Rapport Auto\Inventaire\";

			lock (appData.lockUpload)
			{
				AutoResetEvent autoResetEvent = new AutoResetEvent(false);
				autoResetEvent.WaitOne(1000);

				int count = 0;

				if (e.Name.Contains("inventaire.csv"))
				{
					lock (appData.lockDB)
					{
						lock (appData.lockModel)
						{
							appData.fichierAchat.Clear();
							//appData.modeleMoniteur.Clear();

							msg("Loading new Full Inventaire File...");

							while (true)
							{
								try
								{
									using (StreamReader file = new StreamReader(lien))
									{
										string ln;
										string[] data;
										string asset = "";
										string model = "";

										while ((ln = file.ReadLine()) != null)
										{
											if (!ln.Contains("EndUser"))
											{
												data = ln.Split(",");
												asset = "";

												if (data[3].ToLower().StartsWith("b")) asset = data[3];
												else if (!data[3].ToLower().StartsWith("b") && data[2].ToLower().StartsWith("b")) asset = data[2];

												if (data[1] == "Clavier nip")
												{
													foreach (var item in appData.nip.ToArray())
													{
														if (data[6].ToUpper() == item.serial)
														{
															asset = item.asset;
															break;
														}
													}
												}

												if (data[5].Contains("�")) model = data[5].Replace("�", "ç");
												else model = data[5];

												appData.fichierAchat.Add(new Achat { type = data[1], model = data[5], asset = asset, serial = data[6].ToUpper() });
											}
										}

										file.Close();
									}

									break;
								}
								catch (Exception ex)
								{
									autoResetEvent.WaitOne(1000);
									count++;

									if (count == 30)
									{
										msg(ex.Message);
										break;
									}
								}
							}
						}
					}

					autoResetEvent.WaitOne(1000);

					try
					{
						File.Delete(lien);
					}
					catch { }

					SaveInventaireDesjardins();
					nipAsset();

					if (appData.nip.Count != 0)
					{
						foreach (var item in appData.nip.ToArray())
						{
							Parallel.ForEach(appData.fichierAchat.ToArray(), (main, state) =>
							{
								if (item.serial == main.serial)
								{
									main.asset = item.asset;
									state.Break();
								}
							});
						}
					}

					msg("Loading new Full Inventaire Completed");
				}

				if (e.Name == "nip.csv")
				{
					lock (appData.lockDB)
					{
						lock (appData.lockModel)
						{
							appData.nip.Clear();
							msg("Loading new NIP File...");


							while (true)
							{
								try
								{
									using (StreamReader file = new StreamReader(lien2 + e.Name))
									{
										string ln;
										string[] valeur;

										while ((ln = file.ReadLine()) != null)
										{
											valeur = ln.Split(",");
											appData.nip.Add(new Achat { type = "Clavier nip", asset = valeur[1], serial = valeur[0].ToUpper() });

											Parallel.ForEach(Program.appData.fichierAchat.ToArray(), (item, state) =>
											{
												if (valeur[0].ToUpper() == item.serial)
												{
													item.asset = valeur[1];
													state.Break();
												}
											});
										}

										file.Close();
									}

									break;
								}
								catch (Exception ex)
								{
									autoResetEvent.WaitOne(1000);
									count++;

									if (count == 20)
									{
										msg(ex.Message);
										break;
									}
								}

							}
						}
					}

					autoResetEvent.WaitOne(1000);

					try
					{
						File.Delete(lien2 + e.Name);
					}
					catch { }

					msg("Loading new NIP file Completed");

					SaveInventaireDesjardinsNIP();
					SaveInventaireDesjardins();
					nipAsset();
				}
			}
		}

		private protected void myHandler(object sender, ConsoleCancelEventArgs args)
		{
			Console.WriteLine($"  Key pressed: {args.SpecialKey}");
			Console.WriteLine("The Server will save data and exit.\n");
			KickAllClient();
			SaveDatabase();
			SavePreparation();
			SaveUsers();
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
			autoResetEvent.WaitOne(60000);
			int timeCompare;
			string day;

			while (true)
			{
				timeCompare = int.Parse(DateTime.Now.ToString("H:mm").Replace(":", ""));
				day = DateTime.Now.ToString("dddd", ci);

				if ((timeCompare > 700 && timeCompare < 2300) && (day != "samedi" && day != "dimanche"))
				{
					SaveDatabaseBackup();
					SaveWBbackup();
					wbBackup();
					SavePreparationBackup();
					AutoRapport(appData.invPostes, "Rapport.xlsx");
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
					resetSelection();

					if (DateTime.Now.Year.ToString() != year)
					{
						year = DateTime.Now.Year.ToString();
						appData.waybills.Clear();
						SaveWB();
						SetYearList();
					}

					foreach (var item in appData.selectedTransit.ToArray()) item.count = 0;
					
					UpdateRepair();
				}

				if (day != "samedi" && day != "dimanche")
                {
					if (timeCompare >= 830 && timeCompare < 835) _ = puroTracking();
					if (timeCompare >= 1520 && timeCompare < 1525) Task.Run(RapportFTP);
					//if (timeCompare >= 1000 && timeCompare < 1005) _ = puroTracking();
					//if (timeCompare >= 1200 && timeCompare < 1205) _ = puroTracking();
					//if (timeCompare >= 1400 && timeCompare < 1405) _ = puroTracking();
					//if (timeCompare >= 1530 && timeCompare < 1535) _ = puroTracking();
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

            foreach (var item in appData.invPostes.ToArray())
            {
                if (item.dateEntry == today || (item.dateSortie.Contains(today) && item.statut == "Sortie")) appData.jour.Add(item);
            }

            foreach (var item in appData.jour.ToArray())
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

            AutoRapport(appData.jour, "RapportEntrerSortie.xlsx");
            appData.jour.Clear();
            _ = uploadFTP();
        }

        private async Task uploadFTP()
		{
			await Task.Delay(2000);

			int count = 0;
			string path = @".\Rapport Auto\RapportEntrerSortie.xlsx";

			if (!File.Exists(path))
			{
				msg("RapportEntrerSortie.xlsx don't exist, FTP upload cancelled.");
				return;
			}

			msg("Attempting connection to FTP server to send the report ...");
				
			while (true)
			{
				try
				{
					FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.getserveur.com/RapportEntrerSortie.xlsx");
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
					count ++;
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

        private void UpdateColor()
		{
			lock (appData.lockDB)
			{
				var date = DateTime.Now;
				DateTime expire;

				foreach (var item in appData.invPostes.ToArray())
				{
					if (!string.IsNullOrEmpty(item.dateCloneValid) && item.emplacement != "QUANTUM" && !item.emplacement.Contains("REPAIR") && (item.statut == "En Stock" || item.statut == "Réservé") && item.statut != "Sortie")
					{
						var array = item.dateCloneValid.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
						expire = DateTime.Parse(array[array.Count() -1]);

						if ((expire - date).TotalDays <= 15 && !((expire - date).TotalDays <= 5)) item.xcolor = "1";
						else if ((expire - date).TotalDays <= 5) item.xcolor = "2";
						else if (expire < date) item.xcolor = "2";
						else item.xcolor = "";
					}

					if (item.statut == "Sortie" || item.emplacement == "QUANTUM" || item.emplacement.Contains("REPAIR")) item.xcolor = "";
				}

				SaveDatabase();
			}
		}

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
				ws.Cells().Style.Alignment.WrapText = true;
				ws.Columns().AdjustToContents();
				//ws.Rows().AdjustToContents();
				ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
				wb.SaveAs(@"T:\Rapport Auto\" + name);
				//wb.SaveAs(@".\Rapport Auto\" + name);
				wb.SaveAs(@".\Rapport Auto\" + name);
				//wb.SaveAs(@"C:\test\" + name);
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

				try
				{
					var jsonString = JsonSerializer.Serialize(appData.invPostes);
					File.WriteAllText(timeCheck, jsonString);
					File.WriteAllText(@"T:\Rapport Auto\backup\DB.bak", jsonString);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}

				msg("New backup saved (DB).");
			}
		}

		public void SaveWBbackup()
		{
			lock (lockSave)
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Waybills>));
					using (StreamWriter wr = new StreamWriter(@"T:\Rapport Auto\backup\" + year + ".wb"))
					{
						xs.Serialize(wr, appData.waybills);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

				msg("New backup saved (WB).");
			}
		}

		public void SaveInventaireDesjardins()
		{
			lock (lockSave)
			{
				string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"invDesjardins.xml";

				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Achat>));
					using (StreamWriter wr = new StreamWriter(path))
					{
						xs.Serialize(wr, appData.fichierAchat);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void SavePreparation()
		{
			lock (lockSave)
			{
				string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"preparation.prep";

				//var jsonString = File.ReadAllText(path);

				try
				{
					var jsonString = JsonSerializer.Serialize(appData.listPreparation, new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(path, jsonString);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void SavePreparationBackup()
		{
			lock (lockSave)
			{
				string path = @"T:\Rapport Auto\backup\preparation.bak";
				string path2 = backupFolder + "prep " + DateTime.Now.ToString("dd-MM-yyyy H;mm;ss") + ".bak";
				//var jsonString = File.ReadAllText(path);

				try
				{
					var jsonString = JsonSerializer.Serialize(appData.listPreparation, new JsonSerializerOptions { WriteIndented = true });
					File.WriteAllText(path, jsonString);
					File.WriteAllText(path2, jsonString);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		private void LoadPreparation()
		{
			string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"preparation.prep";

			if (!File.Exists(path)) return;

			msg("---Loading Preparation-- ");

			try
			{
				var jsonString = File.ReadAllText(path);
				appData.listPreparation = JsonSerializer.Deserialize<List<Preparation>>(jsonString, new JsonSerializerOptions { WriteIndented = true });

				msg("---Preparation Loaded--- ");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			resetSelection();
			//try
			//{
			//	XmlSerializer xs = new XmlSerializer(typeof(List<Preparation>));
			//	using (StreamReader rd = new StreamReader(path))
			//	{
			//		appData.listPreparation = xs.Deserialize(rd) as List<Preparation>;
			//	}

			//	msg("---Preparation Loaded--- ");
			//}
			//catch (Exception ex)
			//{
			//	Console.WriteLine(ex.Message);
			//}
		}

		private void resetSelection()
        {
			foreach (var item in appData.listPreparation.ToArray())
			{
				foreach (var stuff in item.info)
				{
					appData.selectedTransit.Add(new SelectedTransit { transit = stuff.transit });
					stuff.selected = false;
				}
			}
		}

		public void SaveInventaireDesjardinsNIP()
		{
			lock (lockSave)
			{
				string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"invDesjardinsNIP.xml";

				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Achat>));
					using (StreamWriter wr = new StreamWriter(path))
					{
						xs.Serialize(wr, appData.nip);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

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
			}

			ModelList();
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

		public void LoadingDesjardins()
		{
			string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"invDesjardins.xml";

			if (!File.Exists(path)) return;

			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(List<Achat>));
				using (StreamReader rd = new StreamReader(path))
				{
					appData.fichierAchat = xs.Deserialize(rd) as List<Achat>;
				}

				msg("---Fichier Desjardins Loaded---");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void LoadingNip()
		{
			string path = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"database" + Path.DirectorySeparatorChar + @"invDesjardinsNIP.xml";

			if (!File.Exists(path)) return;

			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(List<Achat>));
				using (StreamReader rd = new StreamReader(path))
				{
					appData.nip = xs.Deserialize(rd) as List<Achat>;
				}

				msg("---Fichier NIP Desjardins Loaded---");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

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

		public void SaveSeuilPoste()
		{
			lock (lockSave)
			{
				string folder = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"seuils\";

				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Seuil>));
					using (StreamWriter wr = new StreamWriter(folder + "seuilPoste.xml"))
					{
						xs.Serialize(wr, appData.seuilPoste);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void SaveSeuilMoniteur()
		{
			lock (lockSave)
			{
				string folder = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"seuils\";

				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<seuilMoniteurAdmin>));
					using (StreamWriter wr = new StreamWriter(folder + "seuilMoniteur.xml"))
					{
						xs.Serialize(wr, appData.moniteurSeuil);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void SaveSeuilAccess()
		{
			lock (lockSave)
			{
				string folder = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"seuils\";

				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Seuil>));
					using (StreamWriter wr = new StreamWriter(folder + "seuilAccess.xml"))
					{
						xs.Serialize(wr, appData.seuilAccess);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void LoadSeuil()
		{
			string folder = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"seuils\";

			if (!File.Exists(folder + "seuilPoste.xml"))
			{
				appData.seuilPoste.Add(new Seuil { type = "Postes", alerte = "0", max = "0" });
				appData.seuilPoste.Add(new Seuil { type = "Portables", alerte = "0", max = "0" });
				appData.seuilPoste.Add(new Seuil { type = "Tablettes", alerte = "0", max = "0" });
			}
			else
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<Seuil>));
					using (StreamReader rd = new StreamReader(folder + "seuilPoste.xml"))
					{
						appData.seuilPoste = xs.Deserialize(rd) as List<Seuil>;
					}

					msg("---Seuil poste Loaded--- ");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

			}

			if (File.Exists(folder + "seuilMoniteur.xml"))
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<seuilMoniteurAdmin>));
					using (StreamReader rd = new StreamReader(folder + "seuilMoniteur.xml"))
					{
						appData.moniteurSeuil = xs.Deserialize(rd) as List<seuilMoniteurAdmin>;
					}

					msg("---Seuil Moniteur Loaded--- ");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}



			if (!File.Exists(folder + "seuilAccess.xml")) return;

			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(List<Seuil>));
				using (StreamReader rd = new StreamReader(folder + "seuilAccess.xml"))
				{
					appData.seuilAccess = xs.Deserialize(rd) as List<Seuil>;
				}

				msg("---Seuil Acess Loaded--- ");
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

		public void SaveModelMoniteur()
		{
			lock (lockSave)
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<string>));
					using (StreamWriter wr = new StreamWriter(path3))
					{
						xs.Serialize(wr, appData.modeleMoniteur);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void LoadModel()
		{
			if (File.Exists(path3))
			{
				try
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<string>));
					using (StreamReader rd = new StreamReader(path3))
					{
						appData.modeleMoniteur = xs.Deserialize(rd) as List<string>;
					}

					msg("---Modele moniteurs loaded--- ");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

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
			string folder4 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"fichier inventaire";
			string folder5 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"Rapport Auto";
			string folder6 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"Logs";
			string folder7 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"waybills";
			string folder8 = Environment.CurrentDirectory + Path.DirectorySeparatorChar + @"seuils";

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

			if (!Directory.Exists(folder4))
			{
				try
				{
					Directory.CreateDirectory(folder4);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}

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

			if (!Directory.Exists(folder8))
			{
				try
				{
					Directory.CreateDirectory(folder8);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}

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
