using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;

namespace Client
{
	public class IMClient
	{
		//Stopwatch timeOut = new Stopwatch();
		Window main;
		public static readonly Object lockDB = new Object();
		private static readonly Object lockModel = new Object();
		private static readonly Object lockWB = new Object();
		private static readonly Object lockLog = new Object();
		public TcpClient client;
		public NetworkStream netStream;
		SslStream ssl;
		BinaryReader br;
		BinaryWriter bw;
		Thread tcpThread;      // Receiver
		bool _conn = false;    // Is connected/connecting?
		string _user;          // Username
		string _IPAddress;      // IPAdresse of the host we try to connect to.
		int port;
		bool reg;              // Register mode
		int select = 0;
		int selectMod = 0;

		// Packet types
		public const int IM_Hello = 2012;      // Hello
		public const byte IM_OK = 0;           // OK
		public const byte IM_Login = 1;        // Login
		public const byte IM_Register = 2;     // Register
		public const byte IM_TooUsername = 3;  // Too long username
		public const byte IM_TooPassword = 4;  // Too long password
		public const byte IM_Exists = 5;       // Already exists
		public const byte IM_NoExists = 6;     // Doesn't exist
		public const byte IM_WrongPass = 7;    // Wrong password
		public const byte IM_Ajout = 8;    // client a envoyer des data a ajouter
		public const byte IM_Update = 9;    // serveur envoyer au client un update
		public const byte IM_Doublon = 10;    // serveur envoyer au client un update
		public const byte IM_RequestDatabase = 11;    // client au login demande la database au serveur
		public const byte IM_databaseIncoming = 12;    // serveur envoye la database au client
		public const byte IM_ResetModel = 13; 
		public const byte IM_AjoutEnd = 14; //envoyer au client qui fait un action pour confirmer ( fail ou sucess) et reactiver les bountons 
		public const byte IM_Privilege = 15;    // envoie le privilege
		public const byte IM_RequestUserList = 16; //Request the user list.
		public const byte IM_UserList = 17; //serveur sending users
		public const byte IM_ChangePrivilege = 18; //demande de changement de privilege
		public const byte IM_Sortie = 19; //demande de sortie de stuff
		public const byte IM_Modele = 20; //demande de les modeles
		public const byte IM_UpdateModele = 21; //update model (ajout ou delete)
		public const byte IM_Emplacement = 22; //demande de changement d emplacement
		public const byte IM_Retour = 23; //demande de retour d equipement
		public const byte IM_CheckWB = 24; // infoDetail WB info
		public const byte IM_EnvoyerLab = 25; // envoyer poste au lab
		public const byte IM_BackupList = 26; // demande list des backup
		public const byte IM_DeleteFiles = 27; // files to delete
		public const byte IM_ConfirmCLone = 28; // lab clonage fini et valider
		public const byte IM_ModeleRequest = 29; // si ajout a fair en lier avec clonage valider
		public const byte IM_Waybills = 30; // ajout et del de waybill
		public const byte IM_ServerNotice = 31; // kick notice to user
		public const byte IM_Comment = 32; // commentaire WB
		public const byte IM_ChangeUserInfo = 33; //client request un password change
		public const byte IM_RequestWaybills = 34; //login waybills

		public const byte IM_SetMagasin = 35; // donne les magasin

		public const byte IM_DeleteMain = 36; // delete une entry
		public const byte IM_DeleteUser = 37; // delete une entry
		public const byte IM_WByear = 38; // les anners des wb
		public const byte IM_rapportLab = 39; // rapport lab
		//public const byte IM_Seuil = 40; // seuils client tab
		//public const byte IM_SeuilAdmin = 41; // seuils admin modif
		//public const byte IM_SeuilAdminRequest = 42; // admin tab info
		public const byte IM_Modify = 43; // modify infos
		//public const byte IM_SeuilLCDajout = 44; // ajout moniteur seuil
		//public const byte IM_SeuilLCDmodif = 45; // modif moniteur seuil
		//public const byte IM_SeuilDELlcd = 46; // delete moniteur seuil
		//public const byte IM_AccessAjout = 47; // ajout accessoir seuil
		//public const byte IM_AccessRequest = 48; // request access seuil au login
		//public const byte IM_DelAccess = 49; // request access seuil au login
		//public const byte IM_ModifAccess = 50; // modif des accessoire
		//public const byte IM_QteAccess = 51; // ajout/ retrai Access qté
		//public const byte IM_RetourSpecial = 52; // retour equipement special (admin)
		//public const byte IM_FileUpload = 53; // file upload
		//public const byte IM_AjoutPreparation = 54; // ajout de material a un transit
		//public const byte IM_DeletePreparation = 55; // delete de material a un transit
		//public const byte IM_CheckPreparation = 56; // check a un transit
		//public const byte IM_PreparationSortie = 57; // sortie de material a un transit
		//public const byte IM_ConfirmSortiePrep = 58; // sortie de material a un transit confirmer
		//public const byte IM_PreparationDeleteTransit = 59; // delete transit
		//public const byte IM_ModifyPreparation = 60; // modify / add a un transit
		//public const byte IM_CheckPreparationAcess = 61; //check accessoire
		//public const byte IM_CommentPreparation = 62; //check accessoire
		//public const byte IM_SelectedPreparation = 63; //selection de preparation
		//public const byte IM_PreparationSemaine = 64; //ajout semaine prep manuel
		//public const byte IM_PreparationTransfert = 65; //transfer transit
		//public const byte IM_PreparationRF = 66; //prep RF
		public const byte IM_MainComment = 67; // comment main
		//public const byte IM_UploadNotice = 68; // file error

		public const byte IM_TrackingPuro = 91; //demande tu tracking puro via API
		public const byte IM_UpdateExisting = 92; //update database general

		public const byte IM_ForceDisconnect = 93;
		//public const byte IM_RequestPreparation = 94;
		//public const byte IM_NewPreparation = 95;
		public const byte IM_ForceQuit = 96;
		public const byte IM_RequestLogs = 97;
		public const byte IM_Logs = 98;
		public const byte IM_Test = 99;
		public const byte IM_WBdate = 100;

		// Events
		public event EventHandler LoginOK;
		public event EventHandler RegisterOK;
		public event IMErrorEventHandler LoginFailed;
		public event IMErrorEventHandler RegisterFailed;
		public event EventHandler Disconnected;

		public void setMain(Window win)
		{
			main = win as MainWindow;
		}

		// Start connection thread and login or register.
		void connect(bool register)
		{
			try
			{
				if (!_conn)
				{
					_conn = true;
					_user = App.appData.UserName;
					reg = register;
					_IPAddress = App.appData.IPAddress;
					port = App.appData.Port;
					tcpThread = new Thread(new ThreadStart(SetupConn));
					tcpThread.IsBackground = true;
					tcpThread.Start();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem to initialize connection: " + ex.Message);
			}
		}

		public void Login()
		{
			connect(false);
		}

		public void Register()
		{
			connect(true);
		}

		public void Disconnect()
		{
			if (client != null && (_conn || client.Connected)) CloseConn();
		}

		private string convert(SecureString pass) // pass variable sender au server (secure string a string)
		{
			IntPtr unmanagedString = IntPtr.Zero;
			try
			{
				unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(pass);
				return Marshal.PtrToStringUni(unmanagedString);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
			}
		}

		void SetupConn()  // Setup connection and login
		{
			try
			{
				var ip = Dns.GetHostAddresses(_IPAddress);

				client = new TcpClient();
				client.NoDelay = true;

				client.Connect(ip, port); // Connect to the server.
				netStream = client.GetStream();
			}
			catch (SocketException)
			{
				_conn = false;

				if (client != null) client.Close();

				if (App.appData.netChange) MessageBox.Show("Connection Problem, WChat could not restore the connection." + Environment.NewLine + "Check if you still have internet access.");
				else MessageBox.Show("Could not Connect to the Host, Verify the IP Address/Port or verify if the Server is Running.");

				App.appData.netChange = false;
				App.appData.connectEnable = true;
				App.appData.registerEnable = true;
				App.appData.registerForm = true;
				return;
			}

			try
			{
				ssl = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCert));
				ssl.AuthenticateAsClient("InstantMessengerServer");
				// Now we have encrypted connection.

				br = new BinaryReader(ssl, Encoding.UTF8);
				bw = new BinaryWriter(ssl, Encoding.UTF8);

			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem with server validation : " + ex.Message);

				if (client != null) CloseConn();

				App.appData.connectEnable = true;
				App.appData.registerEnable = true;
				App.appData.registerForm = true;
				App.appData.Password.Clear();
				return;
			}

			// Receive "hello"
			int hello = br.ReadInt32();

			if (hello == IM_Hello)
			{
				// Hello OK, so answer.
				bw.Write(IM_Hello);
				bw.Write(reg ? IM_Register : IM_Login);  // Login or register
				bw.Write(_user);
				bw.Write(convert(App.appData.Password)); // send le pass
				bw.Flush();

				byte ans = br.ReadByte();  // Read answer.

				if (ans == IM_OK)  // Login/register OK
				{
					if (reg) OnRegisterOK();  // Register is OK.
					OnLoginOK();  // Login is OK (when registered, automatically logged in)
					Receiver(); // Time for listening for incoming messages.
				}
				else if (ans == IM_WrongPass)
				{
					IMErrorEventArgs err = new IMErrorEventArgs((IMError)ans);
					OnLoginFailed(err);
				}
				else if (ans == IM_NoExists)
				{
					IMErrorEventArgs err = new IMErrorEventArgs((IMError)ans);
					OnLoginFailed(err);
				}
				else
				{
					IMErrorEventArgs err = new IMErrorEventArgs((IMError)ans);
					if (reg) OnRegisterFailed(err);
					else OnLoginFailed(err);
				}
			}

			if (_conn) CloseConn();
		}

		void Receiver()  // Receive all incoming packets.
		{
			try
			{
				while (client.Connected)  // While we are connected.
				{
					byte data = br.ReadByte();  // Get incoming packet type.

					switch (data)
					{
						//case IM_NewPreparation:
						//	{
						//		var semaine = br.ReadString();
						//		List<Preparation> temp = new List<Preparation>();
						//		temp = JsonSerializer.Deserialize<List<Preparation>>(semaine);

						//		lock (App.appData.lockPrepare)
						//		{
						//			bool exist = false;

						//			foreach (var item in App.appData.prepareList.ToArray())
						//			{
						//				if (item.semaine == temp[0].semaine)
						//				{
						//					exist = true;

						//					foreach (var stuff in temp[0].info)
						//					{
						//						Application.Current.Dispatcher.Invoke(() =>
						//						{
						//							item.info.Add(stuff);

						//							if ((main as MainWindow).cb_semaine.SelectedIndex != -1)
						//							{
						//								if ( (main as MainWindow).cb_semaine.SelectedItem.ToString() == item.semaine) App.appData.transits.Add(new Transit { transit = stuff.transit, ready = stuff.ready });
						//							}
						//						});
						//					}

						//					break;
						//				}
						//			}

						//			if (!exist)
						//			{
						//				Application.Current.Dispatcher.Invoke(() =>
						//				{
						//					App.appData.semaine.Add(temp[0].semaine);
						//					App.appData.prepareList.Add(temp[0]);
						//				});
						//			}
						//		}
						//	}
						//	break;

						//case IM_RequestPreparation:
						//	{
						//		string info = br.ReadString();

						//		lock (App.appData.lockPrepare)
						//		{
						//			try
						//			{
						//				App.appData.prepareList = JsonSerializer.Deserialize<List<Preparation>>(info);
						//			}
						//			catch (Exception ex)
						//			{
						//				MessageBox.Show(ex.Message);
						//			}

						//			List<string> list = new List<string>();

						//			Application.Current.Dispatcher.Invoke(() =>
						//			{
						//				foreach (var item in App.appData.prepareList.ToArray())
						//				{
						//					App.appData.semaine.Add(item.semaine);
						//				}

						//				App.appData.sortSemaine.CustomSort = new CustomerSorterSemaine();
						//			});
						//		}
						//	}
						//	break;

						case IM_WByear:
							{
								string wbs = br.ReadString();
								var dataArray = wbs.Split("╚");

								Application.Current.Dispatcher.Invoke(() =>
								{
									App.appData.wbYears.Clear();

									foreach (var item in dataArray)
									{
										App.appData.wbYears.Add(item);
									}
								});
							}
							break;

						//case IM_UploadNotice:
						//	{
						//		Application.Current.Dispatcher.Invoke(() =>
						//		{
						//			MessageBox.Show("Erreur a la lecteur du fichier CSV." + Environment.NewLine + "Veuillez valider que le fichier est bien conforme.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Error);
						//		});
						//	}
						//	break;

						case IM_ForceQuit:
							{
								Application.Current.Dispatcher.Invoke(() =>
								{
									Application.Current.MainWindow.Close();
								});
							}
							break;

						case IM_Update:
							{
								string alldata = br.ReadString();

								lock (lockDB)
								{
									List<InvPostes> temp = new List<InvPostes>();
									temp = JsonSerializer.Deserialize<List<InvPostes>>(alldata);

									Application.Current.Dispatcher.Invoke(() =>
									{
										foreach (var item in temp.ToArray())
                                        {
											App.appData.invPostes.Add(item);
										}

										(main as MainWindow).mainViewScrool.ScrollToEnd();
									});
								}
							}
							break;

						case IM_UpdateExisting:
							{
								string dataLine = br.ReadString();

								lock (lockDB)
								{
									Application.Current.Dispatcher.Invoke(() =>
									{
										List<InvPostes> temp = new List<InvPostes>();
										temp = JsonSerializer.Deserialize<List<InvPostes>>(dataLine);

										foreach (var modify in temp.ToArray())
										{
											for (int x = 0; x < App.appData.invPostes.ToArray().Count(); x++)
											{
												if (App.appData.invPostes[x].serial == modify.serial)
												{
													App.appData.invPostes[x] = modify;
													break;
												}
											}
										}
									});
								}
							}
							break;

						case IM_Waybills:
							{
								string action = br.ReadString();
								string wb = br.ReadString();
								string wbRetour = br.ReadString();
								string rf = br.ReadString();
								string month = br.ReadString();
								string jour = br.ReadString();

								if (App.appData.waybill)
								{
									if (action == "add")
									{
										lock (lockWB)
										{
											Application.Current.Dispatcher.Invoke(() =>
											{
												foreach (Window window in Application.Current.Windows)
												{
													if (window.GetType() == typeof(WaybillsLog))
													{
														if (App.appData.selectedYear == DateTime.Now.Year.ToString())
														{
															App.appData.waybills.Add(new Waybills { RF = rf, wayb = wb, wbRetour = wbRetour, mois = month, jour = jour });

															if (!App.appData.mois.Contains(month)) App.appData.mois.Add(month);
															if (!App.appData.jour.Contains(jour)) App.appData.jour.Add(jour);
														}

														App.appData.countWB = (window as WaybillsLog).ListBills.Items.Count;
														(window as WaybillsLog).wbScrool.ScrollToBottom();

														break;
													}
												}
											});
										}
									}
									else if (action == "del")
									{
										lock (lockWB)
										{
											Application.Current.Dispatcher.Invoke(() =>
											{
												foreach (Window window in Application.Current.Windows)
												{
													if (window.GetType() == typeof(WaybillsLog))
													{
														if (App.appData.selectedYear == DateTime.Now.Year.ToString())
														{
															foreach (var item in App.appData.waybills.ToArray())
															{
																if (item.wayb == wb && item.wbRetour == wbRetour && item.RF == rf)
																{
																	App.appData.waybills.Remove(item);
																	break;
																}
															}
														}

														App.appData.countWB = (window as WaybillsLog).ListBills.Items.Count;
														break;
													}
												}
											});
										}
									}
								}
							}
							break;

						case IM_Logs:
							{
								string info = br.ReadString();
								string type = br.ReadString();
								string amount = br.ReadString();
								string nom = br.ReadString();
								string date = br.ReadString();

								lock (lockLog)
								{
									Application.Current.Dispatcher.Invoke(() =>
									{
										App.appData.logsRapport.Add(new LogsRapport { info = info, type = type, amount = amount, nom = nom, date = date });

										if (App.appData.logsRapport.Count >= 100001) App.appData.logsRapport.RemoveAt(0);

										foreach (Window window in Application.Current.Windows.OfType<Logs>())
										{
											App.appData.countLog = (window as Logs).listLogs.Items.Count;
										}
									});
								}
							}
							break;

						case IM_DeleteMain:
							{
								string serial = br.ReadString();

								var result = serial.Split(",");

								lock (lockDB)
								{
									foreach (var delete in result)
									{
										foreach (var item in App.appData.invPostes.ToArray())
										{
											if (item.serial == delete)
											{
												Application.Current.Dispatcher.Invoke(() =>
												{
													App.appData.invPostes.Remove(item);
												});

												break;
											}
										}
									}
								}
							}
							break;

						//case IM_Seuil:
						//	{
						//		string dataLine = br.ReadString();
						//		var dataArray = dataLine.Split(";");
						//		string[] temp;

						//		decimal calcul;
						//		string color = "";
						//		string commande = "";

						//		Application.Current.Dispatcher.Invoke(() =>
						//		{
						//			App.appData.seuil.Clear();

						//			foreach (var item in dataArray)
						//			{
						//				temp = item.Split(",");
						//				color = "";
						//				commande = "";

						//				if (temp[2] != "0")
						//				{
						//					calcul = Math.Round(decimal.Parse(temp[1]) / decimal.Parse(temp[2]), 2);

						//					if (calcul <= (decimal)0.99)
						//					{
						//						color = "1";
						//						commande = "À commander";
						//					}

						//					if (calcul <= (decimal)0.25) color = "2";
						//				}

						//				App.appData.seuil.Add(new Seuil { type = temp[0], actuel = temp[1], seuil = temp[2], max = temp[3], color = color, commande = commande });
						//			}
						//		});
						//	}
						//	break;

						//case IM_AccessRequest:
						//	{
						//		string dataLine = br.ReadString();
						//		var dataArray = dataLine.Split("|");
						//		string[] temp;

						//		decimal calcul;
						//		string color = "";
						//		string commande = "";

						//		lock (lockAccess)
						//		{
						//			Application.Current.Dispatcher.Invoke(() =>
						//			{
						//				App.appData.seuilAccess.Clear();

						//				foreach (var item in dataArray)
						//				{
						//					temp = item.Split(";");
						//					color = "";
						//					commande = "";

						//					if (temp[2] != "0")
						//					{
						//						calcul = Math.Round(decimal.Parse(temp[1]) / decimal.Parse(temp[2]), 2);

						//						if (calcul <= (decimal)0.99)
						//						{
						//							color = "1";
						//							commande = "À commander";
						//						}

						//						if (calcul <= (decimal)0.25) color = "2";
						//					}

						//					App.appData.seuilAccess.Add(new Seuil { type = temp[0], actuel = temp[1], seuil = temp[2], max = temp[3], color = color, commande = commande });
						//				}
						//			});
						//		}
						//	}
						//	break;

						//case IM_SeuilAdminRequest:
						//	{
						//		string posteAlerte = br.ReadString();
						//		string posteMax = br.ReadString();
						//		string lapAlerte = br.ReadString();
						//		string lapMax = br.ReadString();
						//		string tabAlerte = br.ReadString();
						//		string tabMax = br.ReadString();
						//		string moniteurs = br.ReadString();

						//		var infoLCD = moniteurs.Split("|");
						//		string[] valeurs;
						//		string[] moniteurArray;

						//		Application.Current.Dispatcher.Invoke(() =>
						//		{
						//			App.appData.seuilAdmin.Clear();

						//			App.appData.seuilAdmin.Add(new Seuil { type = "Postes", seuil = posteAlerte, max = posteMax });
						//			App.appData.seuilAdmin.Add(new Seuil { type = "Portables", seuil = lapAlerte, max = lapMax });
						//			App.appData.seuilAdmin.Add(new Seuil { type = "Tablettes", seuil = tabAlerte, max = tabMax });

						//			foreach (var item in infoLCD)
						//			{
						//				valeurs = item.Split(";");

						//				if (!string.IsNullOrEmpty(valeurs[0]))
						//				{
						//					moniteurArray = valeurs[1].Split(",");
						//					App.appData.seuilAdmin.Add(new Seuil { type = valeurs[0], modele = new List<string>(moniteurArray), seuil = valeurs[2], max = valeurs[3] });
						//				}
						//			}
						//		});

						//	}
						//	break;

						//case IM_AjoutPreparation:
						//	{
						//		string detail = br.ReadString();
						//		string semaine = br.ReadString();
						//		string transif = br.ReadString();

						//		string dataLine = br.ReadString();
						//		var dataArray = dataLine.Split("§");

						//		lock (lockDB)
						//		{
						//			Application.Current.Dispatcher.Invoke(() =>
						//			{
						//				foreach (var serial in dataArray)
						//				{
						//					foreach (var item in App.appData.invPostes.ToArray())
						//					{
						//						if (item.serial == serial.Split(" - ")[1])
						//						{
						//							item.statut = "Réservé";
						//							item.emplacement = transif;
						//							break;
						//						}
						//					}
						//				}
						//			});
						//		}

						//		lock (App.appData.lockPrepare)
						//		{
						//			foreach (var item in App.appData.prepareList.ToArray())
						//			{
						//				if (item.semaine == semaine)
						//				{
						//					foreach (var trans in item.info)
						//					{
						//						if (trans.transit == transif)
						//						{
						//							Application.Current.Dispatcher.Invoke(() =>
						//							{
						//								if (detail == "mini") foreach (var SN in dataArray) trans.SNmini.Add(SN);
						//								if (detail == "sff") foreach (var SN in dataArray) trans.SNsff.Add(SN);
						//								if (detail == "laptop") foreach (var SN in dataArray) trans.SNlaptop.Add(SN);
						//								if (detail == "lcd22") foreach (var SN in dataArray) trans.SNlcd22.Add(SN);
						//								if (detail == "lcd27") foreach (var SN in dataArray) trans.SNlcd27.Add(SN);
						//								if (detail == "nip") foreach (var SN in dataArray) trans.SNnip.Add(SN);
						//								if (detail == "recu") foreach (var SN in dataArray) trans.SNrecu.Add(SN);
						//							});

						//							break;
						//						}
						//					}

						//					break;
						//				}
						//			}
						//		}

						//		if (App.appData.tabPrepareSelected)
						//		{
      //                              Application.Current.Dispatcher.Invoke(() =>
      //                              {
      //                                  (main as MainWindow).UpdateChangeAndColor();
      //                              });
      //                          }
      //                      }
      //                      break;

       //                 case IM_DeletePreparation:
       //                     {
							//	string detail = br.ReadString();
							//	string semaine = br.ReadString();
							//	string transif = br.ReadString();
							//	string dataLine = br.ReadString();

							//	var dataArray = dataLine.Split("§");

							//	lock (lockDB)
							//	{
							//		Application.Current.Dispatcher.Invoke(() =>
							//		{
							//			foreach (var serial in dataArray)
							//			{
							//				foreach (var item in App.appData.invPostes.ToArray())
							//				{
							//					if (serial == item.serial && item.statut != "Sortie")
							//					{
							//						item.statut = "En Stock";

							//						if (item.type == "Moniteur" || item.type == "Clavier nip" || item.type.ToLower().Contains("imprimante")) item.emplacement = "ALL";										
							//						else item.emplacement = "R2GO";

							//						break;
							//					}
							//				}
							//			}
							//		});
							//	}

							//	lock (App.appData.lockPrepare)
							//	{
							//		foreach (var item in App.appData.prepareList.ToArray())
							//		{
							//			if (item.semaine == semaine)
							//			{
							//				foreach (var trans in item.info)
							//				{
							//					if (trans.transit == transif)
							//					{
							//						Application.Current.Dispatcher.Invoke(() =>
							//						{
							//							if (detail == "mini") 
							//							{
							//								foreach (var SN in dataArray)
							//								{
							//									foreach (var del in trans.SNmini.ToArray())
							//									{
							//										if (del.Contains(SN)) trans.SNmini.Remove(del);									
							//									}
							//								}
							//							}

							//							if (detail == "sff")
							//							{
							//								foreach (var SN in dataArray)
							//								{
							//									foreach (var del in trans.SNsff.ToArray())
							//									{
							//										if (del.Contains(SN)) trans.SNsff.Remove(del);
							//									}
							//								}
							//							}

							//							if (detail == "laptop")
							//							{
							//								foreach (var SN in dataArray)
							//								{
							//									foreach (var del in trans.SNlaptop.ToArray())
							//									{
							//										if (del.Contains(SN)) trans.SNlaptop.Remove(del);
							//									}
							//								}
							//							}

							//							if (detail == "lcd22")
							//							{
							//								foreach (var SN in dataArray)
							//								{
							//									foreach (var del in trans.SNlcd22.ToArray())
							//									{
							//										if (del.Contains(SN)) trans.SNlcd22.Remove(del);
							//									}
							//								}
							//							}

							//							if (detail == "lcd27")
							//							{
							//								foreach (var SN in dataArray)
							//								{
							//									foreach (var del in trans.SNlcd27.ToArray())
							//									{
							//										if (del.Contains(SN)) trans.SNlcd27.Remove(del);
							//									}
							//								}
							//							}

							//							if (detail == "nip")
							//							{
							//								foreach (var SN in dataArray)
							//								{
							//									foreach (var del in trans.SNnip.ToArray())
							//									{
							//										if (del.Contains(SN)) trans.SNnip.Remove(del);
							//									}
							//								}
							//							}

							//							if (detail == "recu")
							//							{
							//								foreach (var SN in dataArray)
							//								{
							//									foreach (var del in trans.SNrecu.ToArray())
							//									{
							//										if (del.Contains(SN)) trans.SNrecu.Remove(del);
							//									}
							//								}
							//							}
							//						});

							//						break;
							//					}
							//				}

							//				break;
							//			}
							//		}
							//	}

							//	if (App.appData.tabPrepareSelected)
							//	{
							//		Application.Current.Dispatcher.Invoke(() =>
							//		{
							//			(main as MainWindow).UpdateChangeAndColor();
							//		});
							//	}
							//}
							//break;
							
						case IM_ModeleRequest:
							{
								string poste = br.ReadString();
								string laptop = br.ReadString();
								string serveur = br.ReadString();

								Application.Current.Dispatcher.Invoke(() =>
								{
									App.appData.modelPoste = new ObservableCollection<string>(JsonSerializer.Deserialize<List<string>>(poste));
									App.appData.modelPortable = new ObservableCollection<string>(JsonSerializer.Deserialize<List<string>>(laptop));
									App.appData.modelServeur = new ObservableCollection<string>(JsonSerializer.Deserialize<List<string>>(serveur));
								});
							}
							break;

						case IM_Doublon:
							{
								string info = br.ReadString();
								string listSN = br.ReadString();

								bool setMain = true;

								Application.Current.Dispatcher.Invoke(() =>
								{
									foreach (Window window in Application.Current.Windows)
									{
										if (window.GetType() == typeof(AjoutInventaire))
										{
											(window as AjoutInventaire).tb_erreur.Text = info;
											(window as AjoutInventaire).tb_serial.Text = listSN;
											setMain = false;
										}

										if (window.GetType() == typeof(Sortie))
										{
											(window as Sortie).tb_erreur.Text = info;
											(window as Sortie).tb_serial.Text = listSN;
											setMain = false;
										}

										if (window.GetType() == typeof(Emplacement))
										{
											(window as Emplacement).tb_erreur.Text = info;
											(window as Emplacement).tb_serial.Text = listSN;
											setMain = false;
										}

										if (window.GetType() == typeof(Retour))
										{
											(window as Retour).tb_erreur.Text = info;
											(window as Retour).tb_serial.Text = listSN;
											setMain = false;
										}

										if (window.GetType() == typeof(SendLab))
										{
											(window as SendLab).tb_erreur.Text = info;
											(window as SendLab).tb_serial.Text = listSN;
											setMain = false;
										}

										//if (window.GetType() == typeof(AjoutPreparation))
										//{
										//	(window as AjoutPreparation).tb_erreur.Text = info;
										//	(window as AjoutPreparation).tb_serial.Text = listSN;
										//	setMain = false;
										//}
									}

									if (setMain)
									{
										(main as MainWindow).tb_erreur.Text = info;
										(main as MainWindow).tb_serialLab.Text = listSN;
									}

									App.appData.snColor = true;
								});
							}
							break;

						case IM_Modele:
							{
								string type = br.ReadString();
								string models = br.ReadString();
								var result = models.Split("╚");

								lock (lockModel)
								{
									Application.Current.Dispatcher.Invoke(() =>
									{
										App.appData.typesModels.Add(new TypeModel { type = type, modeles = new ObservableCollection<string>(result) });
										App.appData.types.Add(type);
										App.appData.validTypes.Add(type);

										App.appData.typeSearch = select;
										App.appData.modelSearch = selectMod;
										App.appData.count = (main as MainWindow).ListViewData.Items.Count;
									});
								}
							}
							break;

						//case IM_CheckPreparation:
						//	{
						//		bool valeur = br.ReadBoolean();
						//		string semaine = br.ReadString();
						//		string transit = br.ReadString();

						//		lock (App.appData.lockPrepare)
						//		{
						//			foreach (var item in App.appData.prepareList.ToArray())
						//			{
						//				if (item.semaine == semaine)
						//				{
						//					foreach (var trans in item.info)
						//					{
						//						if (trans.transit == transit)
						//						{
						//							trans.ready = valeur;
						//							break;
						//						}
						//					}
						//				}
						//			}

						//			foreach (var item in App.appData.transits.ToArray())
						//			{
						//				if (item.transit == transit)
						//				{
						//					Application.Current.Dispatcher.Invoke(() =>
						//					{
						//						App.appData.transCheck = false;
						//						item.ready = valeur;
						//						App.appData.transCheck = true;
						//					});

						//					break;
						//				}
						//			}
						//		}

						//		Application.Current.Dispatcher.Invoke(() =>
						//		{
						//			if (App.appData.tabPrepareSelected && (main as MainWindow).cb_semaine.SelectedIndex != -1)
						//			{
						//				(main as MainWindow).UpdateChangeAndColor();
						//			}
						//		});
						//	}
						//	break;

						//case IM_SelectedPreparation:
						//	{
						//		string semaine = br.ReadString();
						//		string transit = br.ReadString();
						//		bool valeur = br.ReadBoolean();

						//		lock (App.appData.lockPrepare)
						//		{

						//			foreach (var item in App.appData.prepareList.ToArray())
						//			{
						//				if (item.semaine == semaine)
						//				{
						//					foreach (var trans in item.info)
						//					{
						//						if (trans.transit == transit)
						//						{
						//							trans.selected = valeur;
						//							break;
						//						}
						//					}

						//					break;
						//				}
						//			}


						//			foreach (var item in App.appData.transits.ToArray())
						//			{
						//				if (item.transit == transit)
						//				{
						//					Application.Current.Dispatcher.Invoke(() =>
						//					{
						//						item.selected = valeur;
						//					});

						//					break;
						//				}
						//			}
						//		}
						//	}
						//	break;

						//case IM_CheckPreparationAcess:
						//	{
						//		string info = br.ReadString();
						//		bool valeur = br.ReadBoolean();
						//		string semaine = br.ReadString();
						//		string transit = br.ReadString();

						//		lock (App.appData.lockPrepare)
						//		{
						//			foreach (var item in App.appData.prepareList.ToArray())
						//			{
						//				if (item.semaine == semaine)
						//				{
						//					foreach (var trans in item.info)
						//					{
						//						if (trans.transit == transit)
						//						{
						//							if (info == "clavier") trans.clavierCheck = valeur;
						//							if (info == "sourisSansFil") trans.sourisSansFilCheck = valeur;
						//							if (info == "cableSecure") trans.cableSecureCheck = valeur;
						//							if (info == "sac") trans.sacCheck = valeur;
						//							if (info == "sacBandou") trans.sacBandouCheck = valeur;
						//							if (info == "hubUsb") trans.hubUsbCheck = valeur;
						//							if (info == "usbcDP") trans.usbcDPCheck = valeur;
						//							if (info == "livret") trans.livretCheck = valeur;
						//							break;
						//						}
						//					}

						//					break;
						//				}
						//			}

						//			foreach (var item in App.appData.transitSelected.ToArray())
						//			{
						//				if (item.transit == transit)
						//				{
						//					Application.Current.Dispatcher.Invoke(() =>
						//					{
						//						App.appData.accesCheck = false;

						//						if (info == "clavier") item.clavierCheck = valeur;
						//						if (info == "sourisSansFil") item.sourisSansFilCheck = valeur;
						//						if (info == "cableSecure") item.cableSecureCheck = valeur;
						//						if (info == "sac") item.sacCheck = valeur;
						//						if (info == "sacBandou") item.sacBandouCheck = valeur;
						//						if (info == "hubUsb") item.hubUsbCheck = valeur;
						//						if (info == "usbcDP") item.usbcDPCheck = valeur;
						//						if (info == "livret") item.livretCheck = valeur;

						//						App.appData.accesCheck = true;
						//					});

						//					break;
						//				}
						//			}
						//		}
						//	}
						//	break;

						case IM_MainComment:
							{
								string serial = br.ReadString();
								string info = br.ReadString();

								lock (lockDB)
								{
									foreach (var item in App.appData.invPostes.ToArray())
									{
										if (item.serial == serial)
										{
											item.comment = info;
											break;
										}
									}
								}
							}
							break;

						case IM_ChangePrivilege:
							{
								string priv = br.ReadString();

								string old = App.appData.privilege;

								if (priv == "0") priv = "Aucun";
								if (priv == "1") priv = "Vue Inventaire";
								if (priv == "2") priv = "Entrepot";
								if (priv == "3") priv = "Lab";
								if (priv == "4") priv = "Entrepot/Lab";
								if (priv == "9") priv = "Administrateur";

								Application.Current.Dispatcher.Invoke(() =>
								{
									App.appData.privilege = priv;
								});
							}
							break;

						case IM_UserList:
							{
								string user = br.ReadString();
								string privilege = br.ReadString();

								if (privilege == "0") privilege = "Aucun";
								if (privilege == "1") privilege = "Vue Inventaire";
								if (privilege == "2") privilege = "Entrepot";
								if (privilege == "3") privilege = "Lab";
								if (privilege == "4") privilege = "Entrepot/Lab";
								if (privilege == "9") privilege = "Administrateur";

								Application.Current.Dispatcher.Invoke(() =>
								{
									if (user != App.appData.UserName) App.appData.userList.Add(new User { username = user, privilege = privilege });
								});
							}
							break;

						case IM_BackupList:
							{
								string file = br.ReadString();
								var result = file.Split("╚");

								List<string> temp = new List<string>();

								foreach (var item in result)
								{
									temp.Add(item);
								}

								Application.Current.Dispatcher.Invoke(() =>
								{
									App.appData.backup = new ObservableCollection<string>(temp);

									foreach (Window window in Application.Current.Windows)
									{
										if (window.GetType() == typeof(Admin))
										{
											(window as Admin).listBackup.ItemsSource = App.appData.backup;
											break;
										}
									}
								});
							}
							break;

						case IM_Privilege:
							{
								string privilege = br.ReadString();

								if (privilege == "0") privilege = "Aucun";
								if (privilege == "1") privilege = "Vue Inventaire";
								if (privilege == "2") privilege = "Entrepot";
								if (privilege == "3") privilege = "Lab";
								if (privilege == "4") privilege = "Entrepot/Lab";
								if (privilege == "9") privilege = "Administrateur";

								Application.Current.Dispatcher.Invoke(() =>
								{
									App.appData.privilege = privilege;
								});
							}
							break;

						//case IM_ConfirmSortiePrep:
						//	{
						//		string semaine = br.ReadString();
						//		string transit = br.ReadString();

						//		Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
						//		{
						//			MessageBox.Show(Application.Current.MainWindow, "Sortie complèté pour le transit " + transit + " de la " + semaine + " et est enlevé de la liste.", "Inventaire Entrepot", MessageBoxButton.OK, MessageBoxImage.Information);
						//		}));
						//	}
						//	break;

						case IM_AjoutEnd:
							{
								bool error = br.ReadBoolean();

								bool setMain = true;

								if (!error)
								{
									Application.Current.Dispatcher.Invoke(() =>
									{
										foreach (Window window in Application.Current.Windows)
										{
											if (window.GetType() == typeof(AjoutInventaire))
											{
												if ((window as AjoutInventaire).tb_erreur.Text == "")
												{
													(window as AjoutInventaire).tb_serial.Text = "";
													(window as AjoutInventaire).tb_emp.Text = "";
													setMain = false;
												}
											}

											if (window.GetType() == typeof(Sortie))
											{
												if ((window as Sortie).tb_erreur.Text == "")
												{
													(window as Sortie).tb_serial.Text = "";
													(window as Sortie).tb_RF.Text = "";
													setMain = false;
												}
											}

											if (window.GetType() == typeof(Emplacement))
											{
												if ((window as Emplacement).tb_erreur.Text == "")
												{
													(window as Emplacement).tb_serial.Text = "";
													(window as Emplacement).tb_autre.Text = "";
													setMain = false;
												}
											}

											if (window.GetType() == typeof(Retour))
											{
												(window as Retour).tb_serial.Text = "";
												(window as Retour).tb_RF.Text = "";
												setMain = false;
											}

											if (window.GetType() == typeof(SendLab))
											{
												(window as SendLab).tb_serial.Text = "";
												setMain = false;
											}

											if (window.GetType() == typeof(WaybillsLog)) setMain = false;

											//if (window.GetType() == typeof(AjoutPreparation))
											//{
											//	(window as AjoutPreparation).Close();
											//	setMain = false;
											//}
										}

										if (setMain) (main as MainWindow).tb_serialLab.Text = "";
									});
								}

								Application.Current.Dispatcher.Invoke(() =>
								{
									App.appData.invPostesView.Refresh();
									App.appData.invPostesViewLab.Refresh();

									if (!(main as MainWindow).btn_LabClone.IsEnabled && (App.appData.privilege == "Lab" || App.appData.privilege == "Entrepot/Lab" || App.appData.privilege == "Administrateur")) (main as MainWindow).btn_LabClone.IsEnabled = true;
								});

								App.appData.enableAjout = true;
								App.appData.count = (main as MainWindow).ListViewData.Items.Count;
								App.appData.countLab = (main as MainWindow).ListLab.Items.Count;
							}
							break;

						case IM_databaseIncoming:
							{
								string database = br.ReadString();

								lock (lockDB)
								{			
									Application.Current.Dispatcher.Invoke(() =>
									{
										App.appData.invPostes = new ObservableCollection<InvPostes>(JsonSerializer.Deserialize<List<InvPostes>>(database));
										App.appData.enableMain = true;
										App.appData.setFilter();

										foreach (Window window in Application.Current.Windows)
										{
											if (window.GetType() == typeof(MainWindow))
											{
												(window as MainWindow).ListViewData.ItemsSource = App.appData.invPostesView;
												(window as MainWindow).ListLab.ItemsSource = App.appData.invPostesViewLab;
												break;
											}
										}

										App.appData.count = (main as MainWindow).ListViewData.Items.Count;
										App.appData.countLab = (main as MainWindow).ListLab.Items.Count;
									});
								}
							}
							break;

						case IM_ForceDisconnect:
							{
								bw.Write(IM_ForceDisconnect);
								bw.Flush();
							}
							break;

						case IM_rapportLab:
							{
								string dataLine = br.ReadString();
								var allData = dataLine.Split("§σ§");
								string[] result;

								List<InvPostes> temp = new List<InvPostes>();

								if (!string.IsNullOrEmpty(dataLine))
								{
									foreach (var item in allData)
									{
										result = item.Split("╚");
										temp.Add(new InvPostes { type = result[0], model = result[1], serial = result[3], statut = result[4], RF = result[5], RFretour = result[6], emplacement = result[7], dateEntry = result[8], dateSortie = result[9], dateRetour = result[10], dateEntryLab = result[11], dateClone = result[12] });
									}
								}

								Application.Current.Dispatcher.Invoke(() =>
								{
									Rapport rapport = new Rapport();
									rapport.lab = true;
									rapport.viewList = new List<InvPostes>(temp);
									rapport.ShowDialog();
								});
							}
							break;

						case IM_WBdate:
							{
								App.appData.WBdate = "Mise à jour du tracking Puro: " + br.ReadString();
							}
							break;

						//case IM_ModifyPreparation:
						//	{
						//		string action = br.ReadString();
						//		string semaine = br.ReadString();
						//		string info = br.ReadString();

						//		var valeur = JsonSerializer.Deserialize<List<Contenu>>(info);
						//		var contenu = valeur[0];

						//		lock (App.appData.lockPrepare)
						//		{
						//			if (action == "add")
						//			{
						//				foreach (var item in App.appData.prepareList.ToArray())
						//				{
						//					if (item.semaine == semaine)
						//					{
						//						Application.Current.Dispatcher.Invoke(() =>
						//						{
						//							item.info.Add(contenu);

						//							if ((main as MainWindow).cb_semaine.SelectedIndex != -1)
						//							{
						//								if ((main as MainWindow).cb_semaine.SelectedItem.ToString() == semaine) App.appData.transits.Add(new Transit { transit = contenu.transit });
						//							}
						//						});

						//						break;
						//					}
						//				}
						//			}
						//		}

						//		if (action == "modify")
						//		{
						//			lock (App.appData.lockPrepare)
						//			{
						//				foreach (var item in App.appData.prepareList.ToArray())
						//				{
						//					if (item.semaine == semaine)
						//					{
						//						foreach (var trans in item.info)
						//						{
						//							if (trans.transit == contenu.transit)
						//							{
						//								Application.Current.Dispatcher.Invoke(() =>
						//								{
						//									trans.mini = contenu.mini;
						//									trans.laptop = contenu.laptop;
						//									trans.sff = contenu.sff;
						//									trans.nip = contenu.nip;
						//									trans.recu = contenu.recu;
						//									trans.lcd22 = contenu.lcd22;
						//									trans.lcd27 = contenu.lcd27;
						//									trans.hubUsb = contenu.hubUsb;
						//									trans.cableSecure = contenu.cableSecure;
						//									trans.clavier = contenu.clavier;
						//									trans.livret = contenu.livret;
						//									trans.sac = contenu.sac;
						//									trans.sacBandou = contenu.sacBandou;
						//									trans.sourisSansFil = contenu.sourisSansFil;
						//									trans.usbcDP = contenu.usbcDP;
						//								});

						//								break;
						//							}
						//						}

						//						break;
						//					}
						//				}
						//			}

						//			Application.Current.Dispatcher.Invoke(() =>
						//			{
						//				(main as MainWindow).UpdateChangeAndColor();
						//			});
						//		}
						//	}
						//	break;

						//case IM_CommentPreparation:
						//	{
						//		string semaine = br.ReadString();
						//		string transit = br.ReadString();
						//		string info = br.ReadString();

						//		lock (App.appData.lockPrepare)
						//		{
						//			foreach (var item in App.appData.prepareList.ToArray())
						//			{
						//				if (item.semaine == semaine)
						//				{
						//					foreach (var tans in item.info)
						//					{
						//						if (tans.transit == transit)
						//						{
						//							tans.comment = info;
						//							break;
						//						}
						//					}

						//					break;
						//				}
						//			}
						//		}

						//		Application.Current.Dispatcher.Invoke(() =>
						//		{
						//			if ((main as MainWindow).listTransit.SelectedIndex != -1)
						//			{
						//				if (((main as MainWindow).listTransit.SelectedItem as Transit).transit == transit) (main as MainWindow).UpdateComment();
						//			}						
						//		});
						//	}
						//	break;

						//case IM_PreparationRF:
						//	{
						//		string semaine = br.ReadString();
						//		string transit = br.ReadString();
						//		string info = br.ReadString();

						//		lock (App.appData.lockPrepare)
						//		{
						//			foreach (var item in App.appData.prepareList.ToArray())
						//			{
						//				if (item.semaine == semaine)
						//				{
						//					foreach (var tans in item.info)
						//					{
						//						if (tans.transit == transit)
						//						{
						//							tans.rf = info;
						//							break;
						//						}
						//					}

						//					break;
						//				}
						//			}
						//		}

						//		Application.Current.Dispatcher.Invoke(() =>
						//		{
						//			if ((main as MainWindow).listTransit.SelectedIndex != -1)
						//			{
						//				if (((main as MainWindow).listTransit.SelectedItem as Transit).transit == transit) (main as MainWindow).UpdateRF();
						//			}
						//		});
						//	}
						//	break;

						//case IM_PreparationSortie:
						//	{
						//		string semaine = br.ReadString();
						//		string transit = br.ReadString();
						//		string info = br.ReadString();

						//		List<InvPostes> temp = new List<InvPostes>();
						//		temp = JsonSerializer.Deserialize<List<InvPostes>>(info, new JsonSerializerOptions { WriteIndented = true });

						//		lock (lockDB)
						//		{
						//			Application.Current.Dispatcher.Invoke(() =>
						//			{
						//				if ((main as MainWindow).listTransit.SelectedIndex != -1)
						//				{
						//					if (((main as MainWindow).listTransit.SelectedItem as Transit).transit == transit) (main as MainWindow).listTransit.UnselectAll();
						//				}
						//			});

						//			foreach (var item in temp)
						//			{
						//				foreach (var stuff in App.appData.invPostes.ToArray())
						//				{
						//					if (item.serial == stuff.serial)
						//					{
						//						Application.Current.Dispatcher.Invoke(() =>
						//						{
						//							stuff.statut = item.statut;
						//							stuff.RF = item.RF;
						//							stuff.dateSortie = item.dateSortie;
						//							stuff.emplacement = item.emplacement;
						//						});

						//						break;
						//					}
						//				}
						//			}
						//		}

						//		lock (App.appData.lockPrepare)
						//		{
						//			foreach (var item in App.appData.prepareList.ToArray())
						//			{
						//				if (item.semaine == semaine)
						//				{
						//					foreach (var tans in item.info)
						//					{
						//						if (tans.transit == transit)
						//						{
						//							item.info.Remove(tans);
						//							break;
						//						}
						//					}

						//					if (item.info.Count == 0)
						//					{
						//						Application.Current.Dispatcher.Invoke(() =>
						//						{
						//							if ((main as MainWindow).cb_semaine.SelectedIndex != -1) (main as MainWindow).cb_semaine.SelectedIndex = -1;
						//						});

						//						App.appData.prepareList.Remove(item);
						//					}

						//					break;
						//				}
						//			}

						//			foreach (var item in App.appData.transits.ToArray())
						//			{
						//				if (item.transit == transit)
						//				{
						//					Application.Current.Dispatcher.Invoke(() =>
						//					{
						//						App.appData.transits.Remove(item);
						//					});

						//					break;
						//				}
						//			}
						//		}
						//	}
						//	break;

						//case IM_PreparationDeleteTransit:
						//	{
						//		string semaine = br.ReadString();
						//		string transit = br.ReadString();

						//		lock (App.appData.lockPrepare)
						//		{
						//			Application.Current.Dispatcher.Invoke(() =>
						//			{
						//				if ((main as MainWindow).listTransit.SelectedIndex != -1)
						//				{
						//					if (((main as MainWindow).listTransit.SelectedItem as Transit).transit == transit) (main as MainWindow).listTransit.UnselectAll();
						//				}
						//			});

						//			foreach (var item in App.appData.prepareList.ToArray())
						//			{
						//				if (item.semaine == semaine)
						//				{
						//					foreach (var tans in item.info)
						//					{
						//						if (tans.transit == transit)
						//						{
						//							item.info.Remove(tans);
						//							break;
						//						}
						//					}

						//					if (item.info.Count == 0)
						//					{
						//						Application.Current.Dispatcher.Invoke(() =>
						//						{
						//							if ((main as MainWindow).cb_semaine.SelectedIndex != -1) (main as MainWindow).cb_semaine.SelectedIndex = -1;
						//							App.appData.semaine.Remove(semaine);
						//						});

						//						App.appData.prepareList.Remove(item);
						//					}

						//					break;
						//				}
						//			}

						//			foreach (var item in App.appData.transits.ToArray())
						//			{
						//				if (item.transit == transit)
						//				{
						//					Application.Current.Dispatcher.Invoke(() =>
						//					{
						//						App.appData.transits.Remove(item);
						//					});

						//					break;
						//				}
						//			}
						//		}
						//	}
						//	break;

						//case IM_PreparationSemaine:
						//	{
						//		string semaine = br.ReadString();

      //                          lock (App.appData.lockPrepare)
      //                          {
      //                              App.appData.prepareList.Add(new Preparation { semaine = semaine });

      //                              Application.Current.Dispatcher.Invoke(() =>
      //                              {
      //                                  App.appData.semaine.Add(semaine);

      //                              });
      //                          }
      //                      }
						//	break;

						//case IM_PreparationTransfert:
						//	{
						//		string oldSemaine = br.ReadString();
						//		string newSemaine = br.ReadString();
						//		string transit = br.ReadString();

						//		if (oldSemaine == "")
						//		{
						//			Application.Current.Dispatcher.Invoke(() =>
						//			{
						//				MessageBox.Show(main as MainWindow, "Transfer pas complété car le transit " + transit + " existe déjà dans : " + newSemaine, "Inventaire Entrepot");
						//			});
						//		}
						//		else
						//		{
						//			lock (App.appData.lockPrepare)
						//			{
						//				Contenu info = new Contenu();

						//				foreach (var item in App.appData.prepareList.ToArray())
						//				{
						//					if (item.semaine == oldSemaine)
						//					{
						//						foreach (var trans in item.info)
						//						{
						//							if (trans.transit == transit)
						//							{
						//								info = trans;
						//								item.info.Remove(trans);
						//								break;
						//							}
						//						}
						//					}
						//				}

						//				foreach (var item in App.appData.prepareList.ToArray())
						//				{
						//					if (item.semaine == newSemaine)
						//					{
						//						info.selected = false;
						//						item.info.Add(info);
						//						break;
						//					}
						//				}

						//				foreach (var item in App.appData.transits.ToArray())
						//				{
						//					if (item.transit == transit)
						//					{
						//						Application.Current.Dispatcher.Invoke(() =>
						//						{
						//							App.appData.transits.Remove(item);
						//						});

						//						break;
						//					}
						//				}
						//			}
						//		}
						//	}
						//	break;

						case IM_RequestLogs:
							{
								string dataLine = br.ReadString();

								lock (lockLog)
								{
									Application.Current.Dispatcher.Invoke(() =>
									{
										App.appData.logsRapport = new ObservableCollection<LogsRapport>(JsonSerializer.Deserialize<List<LogsRapport>>(dataLine));
									});
								}
							}
							break;

						case IM_Comment:
							{
								string rf = br.ReadString();
								string wb = br.ReadString();
								string info = br.ReadString();

								lock (lockWB)
								{
									foreach (var item in App.appData.waybills.ToArray())
									{
										if (item.RF == rf && item.wayb == wb)
										{
											Application.Current.Dispatcher.Invoke(() =>
											{
												item.comment = info;
											});

											break;
										}
									}
								}
							}
							break;

						case IM_CheckWB:
							{
								string wb = br.ReadString();
								int id = br.ReadInt32();

								Application.Current.Dispatcher.Invoke(() =>
								{
									foreach (Window window in Application.Current.Windows)
									{
										if (window.GetType() == typeof(InfoDetail))
										{
											if ((window as InfoDetail).ID == id)
											{
												(window as InfoDetail).ListBills.ItemsSource = JsonSerializer.Deserialize<ObservableCollection<Waybills>>(wb);
												break;
											}
										}
									}
								});

								//List<Waybills> tempWB = new List<Waybills>();
								//    List<string> mois = new List<string>();
								//    tempWB = JsonSerializer.Deserialize<List<Waybills>>(dataLine);
								//    mois.Add("Tous");

								//    foreach (var item in tempWB.ToArray())
								//    {
								//        if (!mois.Contains(item.mois)) mois.Add(item.mois);
								//    }

								//    Application.Current.Dispatcher.Invoke(() =>
								//    {
								//        App.appData.waybills = new ObservableCollection<Waybills>(tempWB);
								//        App.appData.mois = new ObservableCollection<string>(mois);
								//    });


								//Application.Current.Dispatcher.Invoke(() =>
								//{
								//    foreach (Window window in Application.Current.Windows)
								//    {
								//        if (window.GetType() == typeof(WaybillsLog))
								//        {
								//            (window as WaybillsLog).ListBills.ItemsSource = App.appData.waybills;
								//            (window as WaybillsLog).cb_mois.ItemsSource = App.appData.mois;

								//            (window as WaybillsLog).cb_mois.SelectedIndex = 0;
								//            (window as WaybillsLog).cb_jour.SelectedIndex = -1;

								//            App.appData.countWB = (window as WaybillsLog).ListBills.Items.Count;
								//            (window as WaybillsLog).wbScrool.ScrollToBottom();

								//            break;
								//        }
								//    }

								//    App.appData.setWBfilter();
								//});
							}
							break;

						case IM_RequestWaybills:
							{
								string dataLine = br.ReadString();
								string year = br.ReadString();

								lock (lockWB)
								{
									if (dataLine != "")
                                    {
										List<Waybills> tempWB = new List<Waybills>();
										List<string> mois = new List<string>();
										tempWB = JsonSerializer.Deserialize<List<Waybills>>(dataLine);
										mois.Add("Tous");

										foreach (var item in tempWB.ToArray())
										{
											if (!mois.Contains(item.mois)) mois.Add(item.mois);
										}

										Application.Current.Dispatcher.Invoke(() =>
										{
											App.appData.waybills = new ObservableCollection<Waybills>(tempWB);
											App.appData.mois = new ObservableCollection<string>(mois);
										});
									}
								}

								Application.Current.Dispatcher.Invoke(() =>
								{
									foreach (Window window in Application.Current.Windows)
									{
										if (window.GetType() == typeof(WaybillsLog))
										{
											(window as WaybillsLog).ListBills.ItemsSource = App.appData.waybills;
											(window as WaybillsLog).cb_mois.ItemsSource = App.appData.mois;

											(window as WaybillsLog).cb_mois.SelectedIndex = 0;
											(window as WaybillsLog).cb_jour.SelectedIndex = -1;

											App.appData.countWB = (window as WaybillsLog).ListBills.Items.Count;
											(window as WaybillsLog).wbScrool.ScrollToBottom();

											break;
										}
									}

									App.appData.setWBfilter();
								});
							}
							break;

						case IM_ResetModel:
							{
								select = App.appData.typeSearch;
								selectMod = App.appData.modelSearch;

								Application.Current.Dispatcher.Invoke(() =>
								{
									App.appData.types.Clear();
									App.appData.typesModels.Clear();
									App.appData.types.Add("Tous");
								});
							}
							break;

						case IM_Modify:
							{
								string info = br.ReadString();
								var modify = info.Split("§");

								string[] ligne;
								var date = DateTime.Now;

								lock (lockDB)
								{
									foreach (var item in modify)
									{
										ligne = item.Split("╚");

										foreach (var change in App.appData.invPostes.ToArray())
										{
											if (ligne[0] == change.serial)
											{
												Application.Current.Dispatcher.Invoke(() =>
												{
													change.statut = ligne[1];
													change.RF = ligne[2];
													change.dateSortie = ligne[3];
													change.RFretour = ligne[4];
													change.dateRetour = ligne[5];
													change.dateEntryLab = ligne[6];
													change.emplacement = ligne[7];
												});

												break;
											}
										}
									}
								}
							}
							break;

						//case IM_DelAccess:
						//	{
						//		string type = br.ReadString();
						//		string actuel = br.ReadString();
						//		string alerte = br.ReadString();
						//		string max = br.ReadString();

						//		lock (lockAccess)
						//		{
						//			foreach (var item in App.appData.seuilAccess.ToArray())
						//			{
						//				if (item.type == type && item.actuel == actuel && item.seuil == alerte && item.max == max)
						//				{
						//					Application.Current.Dispatcher.Invoke(() =>
						//					{
						//						App.appData.seuilAccess.Remove(item);
						//					});

						//					break;
						//				}
						//			}
						//		}
						//	}
						//	break;

						//case IM_ModifAccess:
						//	{
						//		string type = br.ReadString();
						//		string alerte = br.ReadString();
						//		string max = br.ReadString();

						//		lock (lockAccess)
						//		{
						//			string color = "";
						//			decimal calcul;
						//			string commande = "";

						//			foreach (var item in App.appData.seuilAccess.ToArray())
						//			{
						//				if (item.type == type)
						//				{
						//					if (alerte != "0")
						//					{
						//						calcul = Math.Round(decimal.Parse(item.actuel) / decimal.Parse(alerte), 2);

						//						if (calcul <= (decimal)0.99)
						//						{
						//							color = "1";
						//							commande = "À commander";
						//						}

						//						if (calcul <= (decimal)0.25) color = "2";
						//					}

						//					Application.Current.Dispatcher.Invoke(() =>
						//					{
						//						item.seuil = alerte;
						//						item.max = max;
						//						item.color = color;

						//						if (commande != "") item.commande = commande;
						//					});

						//					break;
						//				}
						//			}
						//		}
						//	}
						//	break;

						//case IM_QteAccess:
						//	{
						//		string type = br.ReadString();
						//		string actuel = br.ReadString();

						//		lock (lockAccess)
						//		{
						//			string color = "";
						//			decimal calcul;

						//			foreach (var item in App.appData.seuilAccess.ToArray())
						//			{
						//				if (item.type == type)
						//				{
						//					if (item.seuil != "0")
						//					{
						//						calcul = Math.Round(decimal.Parse(actuel) / decimal.Parse(item.seuil), 2);

						//						if (calcul <= (decimal)0.50) color = "1";
						//						if (calcul <= (decimal)0.25) color = "2";
						//					}

						//					Application.Current.Dispatcher.Invoke(() =>
						//					{
						//						item.actuel = actuel;
						//						item.color = color;
						//					});

						//					break;
						//				}
						//			}
						//		}
						//	}
						//	break;

						//case IM_AccessAjout:
						//	{
						//		string type = br.ReadString();
						//		string actuel = br.ReadString();
						//		string alerte = br.ReadString();
						//		string max = br.ReadString();

						//		string color = "";
						//		decimal calcul;

						//		if (alerte != "0")
						//		{
						//			calcul = Math.Round(decimal.Parse(actuel) / decimal.Parse(alerte), 2);

						//			if (calcul <= (decimal)0.50) color = "1";
						//			if (calcul <= (decimal)0.25) color = "2";
						//		}

						//		lock (lockAccess)
						//		{
						//			Application.Current.Dispatcher.Invoke(() =>
						//			{
						//				App.appData.seuilAccess.Add(new Seuil { type = type, actuel = actuel, seuil = alerte, max = max, color = color });
						//			});
						//		}
						//	}
						//	break;

						case IM_ServerNotice:
							{
								string action = br.ReadString();
								string from = br.ReadString();

								if (action == "password")
								{
									if (from == "allow")
									{
										App.appData.confirmUserInfo = true;
										App.appData.serverAnswer = true;
									}
									else if (from == "deny")
									{
										App.appData.confirmUserInfo = false;
										App.appData.serverAnswer = true;
									}
								}
							}
							break;

                        case IM_UpdateModele:
                            {
								string type = br.ReadString();
                                string model = br.ReadString();
                                string task = br.ReadString();

                                lock (lockModel)
                                {
                                    if (task == "add")
                                    {
										if (type == "Poste")
                                        {
											Application.Current.Dispatcher.Invoke(() =>
											{
												App.appData.modelPoste.Add(model);
											});
										}

										if (type == "Portable")
										{
											Application.Current.Dispatcher.Invoke(() =>
											{
												App.appData.modelPortable.Add(model);
											});
										}

										if (type == "Serveur")
										{
											Application.Current.Dispatcher.Invoke(() =>
											{
												App.appData.modelServeur.Add(model);
											});
										}
                                    }

                                    if (task == "del")
                                    {
                                        if (type == "Poste")
                                        {
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                App.appData.modelPoste.Remove(model);
                                            });
                                        }

                                        if (type == "Portable")
                                        {
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                App.appData.modelPortable.Remove(model);
                                            });
                                        }

                                        if (type == "Serveur")
                                        {
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                App.appData.modelServeur.Remove(model);
                                            });
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
			catch (IOException) { }
		}

		public void MainComment(string serial, string info)
		{
			bw.Write(IM_MainComment);
			bw.Write(serial);
			bw.Write(info);
			bw.Flush();
		}

		public void requestDelete(string files)
		{
			bw.Write(IM_DeleteFiles);
			bw.Write(files);
			bw.Flush();
		}

		public void requestDeleteUser(string user)
		{
			bw.Write(IM_DeleteUser);
			bw.Write(user);
			bw.Flush();
		}

		public void ModeleRequest()
		{
			bw.Write(IM_ModeleRequest);
			bw.Flush();
		}

		public void RequestDeleteMain(string serial)
		{
			bw.Write(IM_DeleteMain);
			bw.Write(serial);
			bw.Flush();
		}

		public void sendNew(string serial, string emplacement, string type, string model)
		{
			bw.Write(IM_Ajout);
			bw.Write(serial);
			bw.Write(emplacement);
			bw.Write(type);
			bw.Write(model);
			bw.Flush();
		}


		public void sendSortie(string serial, string RF)
		{
			bw.Write(IM_Sortie);
			bw.Write(serial);
			bw.Write(RF);
			bw.Flush();
		}

		public void Years()
		{
			bw.Write(IM_WByear);
			bw.Flush();
		}

		public void SendWaybills(string action, string waybills, string wbRetour, string RF)
		{
			bw.Write(IM_Waybills);
			bw.Write(action);
			bw.Write(waybills);
			bw.Write(wbRetour);
			bw.Write(RF);
			bw.Flush();
		}

		public void changeEmp(string serial, string info)
		{
			bw.Write(IM_Emplacement);
			bw.Write(serial);
			bw.Write(info);
			bw.Flush();
		}

		public void requestDatabase()
		{
			bw.Write(IM_RequestDatabase);
			bw.Flush();
		}

		public void requestModele()
		{
			bw.Write(IM_Modele);
			bw.Flush();
		}

		public void requestUserList()
		{
			bw.Write(IM_RequestUserList);
			bw.Flush();
		}

		public void RequestWB(string year)
		{
			bw.Write(IM_RequestWaybills);
			bw.Write(year);
			bw.Flush();
		}

		public void updateModel(string type, string model, string task)
		{
			bw.Write(IM_UpdateModele);
			bw.Write(type);
			bw.Write(model);
			bw.Write(task);
			bw.Flush();
		}

		public void RequestRetour(string serial, string rf, string magasin, string emp)
		{
			bw.Write(IM_Retour);
			bw.Write(serial);
			bw.Write(rf);
			bw.Write(magasin);
			bw.Write(emp);
			bw.Flush();
		}

		public void CheckWB(int ID, string RF)
		{
			bw.Write(IM_CheckWB);
			bw.Write(ID);
			bw.Write(RF);
			bw.Flush();
		}

		//public void RetourSpecial(string type, string model, string serial, string rf, string emp)
		//{
		//	bw.Write(IM_RetourSpecial);
		//	bw.Write(type);
		//	bw.Write(model);
		//	bw.Write(serial);
		//	bw.Write(rf);
		//	bw.Write(emp);
		//	bw.Flush();
		//}

		public void EnvoieLab(string serial)
		{
			bw.Write(IM_EnvoyerLab);
			bw.Write(serial);
			//bw.Write(check);
			bw.Flush();
		}

		public void ModifyInfo(string info)
		{
			bw.Write(IM_Modify);
			bw.Write(info);
			bw.Flush();
		}

		public void EnvoieComment(string RF, string WB, string info)
		{
			bw.Write(IM_Comment);
			bw.Write(RF);
			bw.Write(WB);
			bw.Write(info);
			bw.Flush();
		}

		//public void EnvoieCommentPreparation(string semaine, string transit, string info)
		//{
		//	bw.Write(IM_CommentPreparation);
		//	bw.Write(semaine);
		//	bw.Write(transit);
		//	bw.Write(info);
		//	bw.Flush();
		//}

		//public void EnvoiePrepRF(string semaine, string transit, string info)
		//{
		//	bw.Write(IM_PreparationRF);
		//	bw.Write(semaine);
		//	bw.Write(transit);
		//	bw.Write(info);
		//	bw.Flush();
		//}

		public void changePivilege(string name, string info)
		{
			bw.Write(IM_ChangePrivilege);
			bw.Write(name);
			bw.Write(info);
			bw.Flush();
		}

		public void requestBackupList()
		{
			bw.Write(IM_BackupList);
			bw.Flush();
		}

		//public void RequestAccess()
		//{
		//	bw.Write(IM_AccessRequest);
		//	bw.Flush();
		//}

		public void RequestLogs()
		{
			bw.Write(IM_RequestLogs);
			bw.Flush();
		}

		public void Magasin(string magasin, string info)
		{
			bw.Write(IM_SetMagasin);
			bw.Write(magasin);
			bw.Write(info);
			bw.Flush();
		}

		//public void RequestSeuil()
		//{
		//	bw.Write(IM_Seuil);
		//	bw.Flush();
		//}

		//public void SeuilAjout(string type, string model)
		//{
		//	bw.Write(IM_SeuilLCDajout);
		//	bw.Write(type);
		//	bw.Write(model);
		//	bw.Flush();

		//	if ((main as MainWindow).checkSeuil) RequestSeuil();
		//}

		//public void SeuilModif(string type, string model)
		//{
		//	bw.Write(IM_SeuilLCDmodif);
		//	bw.Write(type);
		//	bw.Write(model);
		//	bw.Flush();

		//	if ((main as MainWindow).checkSeuil) RequestSeuil();

		//	Application.Current.Dispatcher.Invoke(() =>
		//	{
		//		foreach (Window window in Application.Current.Windows)
		//		{
		//			if (window.GetType() == typeof(Admin))
		//			{
		//				RequestSeuilAdmin();
		//				break;
		//			}
		//		}
		//	});
		//}

		//public void RequestSeuilAdmin()
		//{
		//	bw.Write(IM_SeuilAdminRequest);
		//	bw.Flush();
		//}

		//public void FileUpload(string action, string info)
		//{
		//	bw.Write(IM_FileUpload);
		//	bw.Write(action);
		//	bw.Write(info);
		//	bw.Flush();
		//}

		//public void SeuilAdminModif(string type, string alerte, string max)
		//{
		//	bw.Write(IM_SeuilAdmin);
		//	bw.Write(type);
		//	bw.Write(alerte);
		//	bw.Write(max);
		//	bw.Flush();

		//	if ((main as MainWindow).checkSeuil) RequestSeuil();
		//}

		public void RequestConfirmClone(string serial)
		{
			bw.Write(IM_ConfirmCLone);
			bw.Write(serial);
			bw.Flush();
		}

		//public void RequestDelSeuilLCD(string type, string alerte, string max)
		//{
		//	bw.Write(IM_SeuilDELlcd);
		//	bw.Write(type);
		//	bw.Write(alerte);
		//	bw.Write(max);
		//	bw.Flush();
		//}

		//public void RequestPreparation()
		//{
		//	bw.Write(IM_RequestPreparation);
		//	bw.Flush();
		//}

		//public void PreparationTransfer(string old, string nouv, string transit)
		//{
		//	bw.Write(IM_PreparationTransfert);
		//	bw.Write(old);
		//	bw.Write(nouv);
		//	bw.Write(transit);
		//	bw.Flush();
		//}

		//public void AjoutSemaine(string info)
		//{
		//	bw.Write(IM_PreparationSemaine);
		//	bw.Write(info);
		//	bw.Flush();
		//}

		//public void SelectionPreparation(string semaine, string transit, bool select)
		//{
		//	bw.Write(IM_SelectedPreparation);
		//	bw.Write(semaine);
		//	bw.Write(transit);
		//	bw.Write(select);
		//	bw.Flush();
		//}

		//public void ModifyPreparation(string action, string semaine, string info)
		//{
		//	bw.Write(IM_ModifyPreparation);
		//	bw.Write(action);
		//	bw.Write(semaine);
		//	bw.Write(info);
		//	bw.Flush();
		//}

		//public void DeleteTransit(string semaine, string transit)
		//{
		//	bw.Write(IM_PreparationDeleteTransit);
		//	bw.Write(semaine);
		//	bw.Write(transit);
		//	bw.Flush();
		//}

		//public void CheckPreparation(bool valeur, string semaine, string transit)
		//{
		//	bw.Write(IM_CheckPreparation);
		//	bw.Write(valeur);
		//	bw.Write(semaine);
		//	bw.Write(transit);
		//	bw.Flush();
		//}

		//public void CheckPreparationAcess(string info, bool valeur, string semaine, string transit)
		//{
		//	bw.Write(IM_CheckPreparationAcess);
		//	bw.Write(info);
		//	bw.Write(valeur);
		//	bw.Write(semaine);
		//	bw.Write(transit);
		//	bw.Flush();
		//}

		//public void PreparationSortie(string semaine, string transit, string rf, string emp)
		//{
		//	bw.Write(IM_PreparationSortie);
		//	bw.Write(semaine);
		//	bw.Write(transit);
		//	bw.Write(rf);
		//	bw.Write(emp);
		//	bw.Flush();
		//}

		//public void AjoutPreparation(string type, string item, string semaine, string transit, string serial)
		//{
		//	bw.Write(IM_AjoutPreparation);
		//	bw.Write(type);
		//	bw.Write(item);
		//	bw.Write(semaine);
		//	bw.Write(transit);
		//	bw.Write(serial);
		//	bw.Flush();
		//}

		//public void DeletePreparation(string item, string semaine, string transit, string serial)
		//{
		//	bw.Write(IM_DeletePreparation);
		//	bw.Write(item);
		//	bw.Write(semaine);
		//	bw.Write(transit);
		//	bw.Write(serial);
		//	bw.Flush();
		//}

		//public void DeleteAccess(string type, string actuel, string alerte, string max)
		//{
		//	bw.Write(IM_DelAccess);
		//	bw.Write(type);
		//	bw.Write(actuel);
		//	bw.Write(alerte);
		//	bw.Write(max);
		//	bw.Flush();
		//}

		//public void AccessAjout(string type, string actuel, string alerte, string max)
		//{
		//	bw.Write(IM_AccessAjout);
		//	bw.Write(type);
		//	bw.Write(actuel);
		//	bw.Write(alerte);
		//	bw.Write(max);
		//	bw.Flush();
		//}

		//public void ModifierAccess(string type, string alerte, string max)
		//{
		//	bw.Write(IM_ModifAccess);
		//	bw.Write(type);
		//	bw.Write(alerte);
		//	bw.Write(max);
		//	bw.Flush();
		//}

		//public void QteAccess(string action, string type, string valeur)
		//{
		//	bw.Write(IM_QteAccess);
		//	bw.Write(action);
		//	bw.Write(type);
		//	bw.Write(valeur);
		//	bw.Flush();
		//}

		public void TrackingPuro()
		{
			bw.Write(IM_TrackingPuro);
			bw.Flush();
		}

		public void WBdate()
		{
			bw.Write(IM_WBdate);
			bw.Flush();
		}

		public void ChangePassword()
		{
			bw.Write(IM_ChangeUserInfo);
			bw.Write(convert(App.appData.oldPassword));
			bw.Write(convert(App.appData.Password));
			bw.Flush();
		}

		public void NetChangeON()
		{
			NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkChanges);
			NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChanges);
		}

		public void NetChangeOFF()
		{
			NetworkChange.NetworkAddressChanged -= new NetworkAddressChangedEventHandler(NetworkChanges);
			NetworkChange.NetworkAvailabilityChanged -= new NetworkAvailabilityChangedEventHandler(NetworkChanges);
		}

		private void NetworkChanges(object sender, EventArgs e)
		{
			if (!App.appData.netChange) App.appData.netChange = true;
		}

		void CloseConn() // Close connection.
		{
			NetChangeOFF();

			try
			{
				_conn = false;
				client.Close();
				if (br != null) br.Close();
				if (bw != null) bw.Close();
				if (ssl != null) ssl.Close();
				if (netStream != null) netStream.Close();
				OnDisconnected();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem when trying to close the connection: " + ex.Message);
			}
		}

		virtual protected void OnDisconnected()
		{
			if (Disconnected != null) Disconnected(this, EventArgs.Empty);
		}

		virtual protected void OnLoginOK()
		{
			if (LoginOK != null) LoginOK(this, EventArgs.Empty);
		}
		virtual protected void OnRegisterOK()
		{
			if (RegisterOK != null) RegisterOK(this, EventArgs.Empty);
		}

		virtual protected void OnLoginFailed(IMErrorEventArgs e)
		{
			if (LoginFailed != null) LoginFailed(this, e);
		}
		virtual protected void OnRegisterFailed(IMErrorEventArgs e)
		{
			if (RegisterFailed != null) RegisterFailed(this, e);
		}

		public static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true; // Allow untrusted certificates.
		}
	}
}
