using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace entrepotServer
{
	public class Client
	{
		Program prog;

		private static readonly Object lockBackup = new Object();
		private static readonly Object lockDelete = new Object();
		private static readonly Object lockLog = new Object();
		private static readonly Object lockSeuil = new Object();
		private readonly Object lockAccess = new Object();

		public TcpClient client;
		public NetworkStream netStream;  // Raw-data stream of connection.
		public SslStream ssl;
		public BinaryReader br;
		public BinaryWriter bw;
		UserInfo userInfo = new UserInfo();  // Information about current user.

		Stopwatch stopWatch = new Stopwatch();

		public const int IM_Test = 2012;  //check de reponse

		public const int IM_Hello = 2012;      // Hello
		public const byte IM_OK = 0;           // OK
		public const byte IM_Login = 1;        // Login
		public const byte IM_Register = 2;     // Register
		public const byte IM_TooUsername = 3;  // Too long username
		public const byte IM_TooPassword = 4;  // Too long password
		public const byte IM_Exists = 5;       // Already exists
		public const byte IM_NoExists = 6;     // Doesn't exists
		public const byte IM_WrongPass = 7;    // Wrong password
		public const byte IM_Ajout = 8;    // client a envoyer des data a ajouter
		public const byte IM_Update = 9;    // serveur envoyer au client un update
		public const byte IM_Doublon = 10;    // serveur envoyer au client un update
		public const byte IM_RequestDatabase = 11;    // client au login demande la database au serveur
		public const byte IM_databaseIncoming = 12;    // serveur envoye la database au client
		public const byte IM_ResetModel = 13;    // find de l'envoie de la data base
		public const byte IM_AjoutEnd = 14;    // find de l'envoie de la data base
		public const byte IM_Privilege = 15;    // envoie le privilege
		public const byte IM_RequestUserList = 16; //Request the user list.
		public const byte IM_UserList = 17; //serveur sending users
		public const byte IM_ChangePrivilege = 18; //demande de changement de privilege
		public const byte IM_Sortie = 19; //demande de sortie de stuff
		public const byte IM_Modele = 20; //demande de les modeles
		public const byte IM_UpdateModele = 21; //update model (ajout ou delete)
		public const byte IM_Emplacement = 22; //demande de changement d emplacement
		public const byte IM_Retour = 23; //demande de retour d equipement
		public const byte IM_RetourAjout = 24; // lors de retour, si equipement existant pas, les ajoutes
		public const byte IM_EnvoyerLab = 25; // envoyer poste au lab
		public const byte IM_BackupList = 26; // demande list des backup
		public const byte IM_DeleteFiles = 27; // files to delete
		public const byte IM_ConfirmCLone = 28; // lab clonage fini et valider
		public const byte IM_ModeleMoniteur = 29; // si ajout a fair en lier avec clonage valider
		public const byte IM_Waybills = 30; // ajout et retrait de waybill
		public const byte IM_ServerNotice = 31; // kick notice to user
		public const byte IM_Comment = 32; // commentaire WB
		public const byte IM_ChangeUserInfo = 33; //client request un password change
		public const byte IM_RequestWaybills = 34; //login waybills
		public const byte IM_AjoutMoniteur = 35; // ajout separer pour moniteurs
		public const byte IM_DeleteMain = 36; // delete une entry
		public const byte IM_DeleteUser = 37;
		public const byte IM_WByear = 38; // les anners des wb
		public const byte IM_rapportLab = 39; // rapport lab
		public const byte IM_Seuil = 40; // seuils
		public const byte IM_SeuilAdmin = 41; // seuils admin modif
		public const byte IM_SeuilAdminRequest = 42; // admin tab infos
		public const byte IM_Modify = 43; // modify infos
		public const byte IM_SeuilLCDajout = 44; // ajout moniteur seuil
		public const byte IM_SeuilLCDmodif = 45; // modif moniteur seuil
		public const byte IM_SeuilDELlcd = 46; // delete moniteur seuil
		public const byte IM_AccessAjout = 47; // ajout accessoir seuil
		public const byte IM_AccessRequest = 48; // request access seuil au login
		public const byte IM_DelAccess = 49; // request access seuil au login
		public const byte IM_ModifAccess = 50; // modif des accessoire
		public const byte IM_QteAccess = 51; // ajout/ retrai Access qté
		public const byte IM_RetourSpecial = 52; // retour equipement special (admin)
		public const byte IM_FileUpload = 53; // file upload
		public const byte IM_AjoutPreparation = 54; // ajout de material a un transit
		public const byte IM_DeletePreparation = 55; // delete de material a un transit
		public const byte IM_CheckPreparation = 56; // delete de material a un transit
		public const byte IM_PreparationSortie = 57; // sortie de material a un transit
		public const byte IM_ConfirmSortiePrep = 58; // sortie de material a un transit confirmer
		public const byte IM_PreparationDeleteTransit = 59; // delete transit
		public const byte IM_ModifyPreparation = 60; // modify / add a un transit
		public const byte IM_CheckPreparationAcess = 61; //check accessoire
		public const byte IM_CommentPreparation = 62; //check accessoire
		public const byte IM_SelectedPreparation = 63; //selection de preparation
		public const byte IM_PreparationSemaine = 64; //ajout semaine prep manuel
		public const byte IM_PreparationTransfert = 65; //transfer transit
		public const byte IM_PreparationRF = 66; //prep RF
		public const byte IM_MainComment = 67; // comment main
		public const byte IM_UploadNotice = 68; // fix error

		public const byte IM_TrackingPuro = 91; // requete pur updater trasking puro
		public const byte IM_UpdateExisting = 92; // envoie au client general DB update
		public const byte IM_ForceDisconnect = 93;
		public const byte IM_RequestPreparation = 94;
		public const byte IM_NewPreparation = 95; //envoie au client la preparation quand le serveur a traiter le file drop
		public const byte IM_ForceQuit = 96; // kick out tous les usagers ( force close le client au complet)
		public const byte IM_RequestLogs = 97; // demande de logs au login du client
		public const byte IM_Logs = 98; // update logs des clients
		public const byte IM_KeepAlive = 99; //Try to keep the TCP connection alive for online user.
		public const byte IM_WBdate = 100; // update du la date du dernier tracking puro

		public Client(Program p, TcpClient c)
		{
			prog = p;
			client = c;

			// Handle client in another thread.
			//	(new Thread(new ThreadStart(SetupConn))).Start();
			if (!prog.IsAliveRunning)
			{
				prog.IsAliveRunning = true;
				Task.Run(KeepAlive);
			}

			Task.Run(SetupConn);
		}

		void KeepAlive()
		{
			prog.msg("KeepAlive thread started.");
			AutoResetEvent autoResetEvent = new AutoResetEvent(false);

			while (true)
			{
				foreach (string UserKey in prog.users.Keys)
				{
					if (prog.users.TryGetValue(UserKey, out UserInfo user))
					{
						if (user.LoggedIn)
						{
							Write(user, IM_KeepAlive);
							prog.msg("KeepAlive sent to : " + user.UserName);
						}
					}
				}

				autoResetEvent.WaitOne(1800000);
			}
		}

		private void Write(UserInfo userinfo, params object[] command)
		{
			lock (userinfo.LOCK)
			{
				foreach (object o in command)
				{
					Type t = o.GetType();

					if (t.Equals(typeof(string)))
					{
						userinfo.Connection.bw.Write((string)o);
					}
					else if (t.Equals(typeof(byte)))
					{
						userinfo.Connection.bw.Write((byte)o);
					}
					else if (t.Equals(typeof(bool)))
					{
						userinfo.Connection.bw.Write((bool)o);
					}
					else
					{
						prog.msg("***WARNING*** Unknown Type for BinaryWriter.write in methode Write.");
					}
				}

				userinfo.Connection.bw.Flush();
			}
		}

		void SetupConn()  // Setup connection and login or register.
		{
			stopWatch.Start();
			Crypto crypto = new Crypto();

			try
			{
				prog.msg("--------------------------------");
				prog.msg("New connection attempt detected");
				netStream = client.GetStream();
				ssl = new SslStream(netStream, false);
				prog.msg("Trying to establish a secure connection!");
				ssl.AuthenticateAsServer(prog.cert, false, SslProtocols.None, true);
				prog.msg("Secure connection established!");
				// Now we have encrypted connection.
				br = new BinaryReader(ssl, Encoding.UTF8);
				bw = new BinaryWriter(ssl, Encoding.UTF8);

				// Say "hello".
				bw.Write(IM_Hello);
				bw.Flush();
				int hello = br.ReadInt32();

				if (hello == IM_Hello)
				{
					// Hello packet is OK. Time to wait for login or register.
					byte logMode = br.ReadByte();
					string userName = br.ReadString();
					string password = br.ReadString();

					if (userName.Length < 30) // Isn't username too long?
					{
						if (password.Length < 20)  // Isn't password too long?
						{
							if (logMode == IM_Register)  // Register mode
							{
								if (!prog.users.ContainsKey(userName))  // User already exists?
								{
									userInfo.UserName = userName;
									userInfo.Password = crypto.Encrypt(password);
									userInfo.Connection = this;
									// We give the lowest privilege 0 to new user. 3 is the highest.
									userInfo.Privilege = "0";

									prog.users.Add(userName, userInfo);  // Add new user
									bw.Write(IM_OK);
									bw.Flush();
									prog.msg("Registered new user " + userName);
									prog.SaveUsers();
									UpdateLogs("Création d'un nouvel usagé: " + userName, "", "", "");
									prog.userInfo.Usersonline.Add(userName);
									userInfo.LoggedIn = true;

									bw.Write(IM_Privilege);
									bw.Write(userInfo.Privilege);
									bw.Flush();

                                    // ID0 always mainchat.
                                    //  PushConversation("0");
                                    //  PushActiveConversation();
                                    Receiver();  // Listen to client in loop.
                                }
                                else 
                                {
									bw.Write(IM_Exists);
									bw.Flush();
								}       
                            }
                            else if (logMode == IM_Login)  // Login mode
							{
								if (prog.users.TryGetValue(userName, out userInfo))  // User exists?
								{
									if (crypto.Encrypt(password) == userInfo.Password)  // Is password OK?
									{
										// If user is logged in yet, disconnect him.
										if (userInfo.LoggedIn)
										{
											userInfo.Connection.bw.Write(IM_ForceDisconnect);
											userInfo.Connection.bw.Flush();
											//userInfo.Connection.client.Close();
										}

										while (userInfo.LoggedIn)
										{
											//loop until user is logged off
										}

										userInfo.Connection = this;
										prog.userInfo.Usersonline.Add(userName);
										bw.Write(IM_OK);
										bw.Flush();
										userInfo.LoggedIn = true;
										prog.msg("User logged in " + userInfo.UserName);
										prog.msg("--------------------------------");

										bw.Write(IM_Privilege);
										bw.Write(userInfo.Privilege);
										bw.Flush();

										//    PushConversation("0");
										//    PushActiveConversation();
										Receiver();  // Listen to client in loop.

										//When poping out of receiver, either a IO error occured or client disconnected.
										prog.userInfo.Usersonline.Remove(userInfo.UserName);
									}
									else
									{
										bw.Write(IM_WrongPass);
										bw.Flush();
									}
								}
								else
								{
									bw.Write(IM_NoExists);
									bw.Flush();
									prog.msg("User does not exist: " + userName);
								}
							}
						}
						else
						{
							bw.Write(IM_TooPassword);
							bw.Flush();
						}
					}
					else
                    {
						bw.Write(IM_TooUsername);
						bw.Flush();
					}
				}
				CloseConn();
			}
			catch (Exception e)
			{
				prog.msg(e.ToString());
				CloseConn();
			}
			stopWatch.Stop();
		}

		private void updateClientModel()
		{
			foreach (var UserKey in prog.users.Keys)
			{
				if (prog.users.TryGetValue(UserKey, out UserInfo user))
				{
					if (user.LoggedIn)
					{
						user.Connection.bw.Write(IM_ResetModel);
						user.Connection.bw.Flush();
					}
				}
			}

			lock (Program.appData.lockModel)
			{
				string type = "";
				List<string> model = new List<string>();

				foreach (var item in Program.appData.typesModels)
				{
					type = item.type;

					foreach (var mod in item.modeles)
					{
						model.Add(mod);
					}

					foreach (var UserKey in prog.users.Keys)
					{
						if (prog.users.TryGetValue(UserKey, out UserInfo user))
						{
							if (user.LoggedIn)
							{
								user.Connection.bw.Write(IM_Modele);
								user.Connection.bw.Write(type);
								user.Connection.bw.Write(String.Join("╚", model));
								user.Connection.bw.Flush();
							}
						}
					}

					model.Clear();
				}
			}
		}

		void Receiver()  // Receive all incoming packets.
		{
			try
			{
				while (client.Client.Connected)  // While we are connected.
				{
					byte data = br.ReadByte();

					switch (data)
					{
						case IM_Ajout:
							{
								string serial = br.ReadString();
								string emplacement = br.ReadString();
								bool doublonCheck = true;
								bool add = false;
								bool found = false;
								bool foundType = false;

								string message = "";

								var dateEntry = DateTime.Now.ToShortDateString();

								var result = serial.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
								List<string> doublon = new List<string>();
								List<string> erreur = new List<string>();
								List<string> itemAdd = new List<string>();
								List<InvPostes> temp = new List<InvPostes>();
								bool newStuff = false;

								lock (Program.appData.lockDB)
								{
									foreach (var tooAdd in result)
									{
										doublonCheck = true;

										foreach (var item in Program.appData.invPostes.ToArray())
										{
											if (item.serial == tooAdd || item.asset == tooAdd)
											{
												doublon.Add(item.serial + " " + item.asset);
												doublonCheck = false;
												break;
											}
										}

										if (doublonCheck) itemAdd.Add(tooAdd);
									}

									foreach (var item in itemAdd.ToArray())
									{
										add = false;

										foreach (var exist in Program.appData.fichierAchat.ToArray())
										{
											foundType = false;

											if (item == exist.serial || item == exist.asset)
											{
												Program.appData.invPostes.Add(new InvPostes { type = exist.type, model = exist.model, serial = exist.serial, asset = exist.asset, dateEntry = dateEntry, statut = "En Stock", emplacement = emplacement, infoAjout = userInfo.UserName });
												temp.Add(new InvPostes { type = exist.type, model = exist.model, serial = exist.serial, asset = exist.asset, dateEntry = dateEntry, statut = "En Stock", emplacement = emplacement, infoAjout = userInfo.UserName });

												lock (Program.appData.lockModel)
												{
													foreach (var types in Program.appData.typesModels.ToArray())
													{

														if (types.type == exist.type)
														{
															foundType = true;

															if (!types.modeles.Contains(exist.model))
															{
																types.modeles.Add(exist.model);
																newStuff = true;
															}

															break;
														}
													}

													if (!foundType)
													{
														Program.appData.typesModels.Add(new TypeModel { type = exist.type, modeles = new List<string> { exist.model } });
														newStuff = true;
													}
												}

												add = true;
												break;
											}
										}

										if (!add) erreur.Add(item);
									}

									if (temp.Count != 0)
									{
										prog.SaveDatabase();

										var jsonString = JsonSerializer.Serialize(temp);

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_Update);
													user.Connection.bw.Write(jsonString);
													user.Connection.bw.Flush();
												}
											}
										}
									}

									if (newStuff) updateClientModel();

									if (erreur.Count != 0) message = "*Avertissement* Aucune Modification pour les numeros de série restant, introuvable dans le fichier globale. Voir avec Patrick Massouh.";

									if (erreur.Count != 0 && doublon.Count == 0)
									{
										found = true;

										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, erreur));
										bw.Flush();
									}
									else if (erreur.Count == 0 && doublon.Count != 0)
									{
										message = "*Avertissement* Aucune Modification pour les numeros de série restant car ils existent deja dans l'inventaire.";
										found = true;
										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, doublon));
										bw.Flush();
									}
									else if (erreur.Count != 0 && doublon.Count != 0)
									{
										message += Environment.NewLine + "*Avertissement* Les numeros de serie suivant sont deja dans l'inventaire et n'ont pas été mofifiés :" + Environment.NewLine + String.Join(", ", doublon);
										found = true;
										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, erreur));
										bw.Flush();
									}
								}

								bw.Write(IM_AjoutEnd);
								bw.Write(found);
								bw.Flush();

								if (temp.Count != 0) UpdateLogs(userInfo.UserName + " - Ajout à l'inventaire: " + temp.Count.ToString() + " entrée(s).", "Ajout d'équipement", temp.Count.ToString(), userInfo.UserName);
							}
							break;

						case IM_AjoutMoniteur:
							{
								string serial = br.ReadString();
								string emplacement = br.ReadString();
								string model = br.ReadString();
								bool doublonCheck = true;
								bool add = true;
								bool found = false;
								bool foundType = false;

								string message = "";

								var dateEntry = DateTime.Now.ToShortDateString();

								var result = serial.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
								List<string> doublon = new List<string>();
								List<string> erreur = new List<string>();
								List<string> itemAdd = new List<string>();
								List<InvPostes> temp = new List<InvPostes>();
								bool newStuff = false;

								lock (Program.appData.lockDB)
								{
									foreach (var tooAdd in result)
									{
										doublonCheck = true;

										foreach (var item in Program.appData.invPostes.ToArray())
										{
											if (item.serial == tooAdd)
											{
												doublon.Add(item.serial);
												doublonCheck = false;
												break;
											}
										}

										if (doublonCheck) itemAdd.Add(tooAdd);
									}

									foreach (var item in itemAdd.ToArray())
									{
										add = true;

										foreach (var exist in Program.appData.fichierAchat.ToArray())
										{
											foundType = false;

											if (item == exist.serial)
											{
												erreur.Add(item);
												add = false;
												break;
											}
										}

										if (add)
										{
											Program.appData.invPostes.Add(new InvPostes { type = "Moniteur", model = model, serial = item, dateEntry = dateEntry, statut = "En Stock", emplacement = emplacement, infoAjout = userInfo.UserName });
											temp.Add(new InvPostes { type = "Moniteur", model = model, serial = item, dateEntry = dateEntry, statut = "En Stock", emplacement = emplacement, infoAjout = userInfo.UserName });

											lock (Program.appData.lockModel)
											{
												foreach (var types in Program.appData.typesModels.ToArray())
												{
													if (types.type == "Moniteur")
													{
														foundType = true;

														if (!types.modeles.Contains(model))
														{
															types.modeles.Add(model);
															newStuff = true;
														}

														break;
													}
												}

												if (!foundType)
												{
													Program.appData.typesModels.Add(new TypeModel { type = "Moniteur", modeles = new List<string> { model } });
													newStuff = true;
												}
											}
										}
									}

									if (temp.Count != 0)
									{
										prog.SaveDatabase();

										var jsonString = JsonSerializer.Serialize(temp);

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_Update);
													user.Connection.bw.Write(jsonString);
													user.Connection.bw.Flush();
												}
											}
										}
									}

									if (newStuff) updateClientModel();

									if (erreur.Count != 0) message = "*Avertissement* Aucune Modification pour les numeros de série restant, car ils ne sont probablement pas de Moniteurs. Entrez des moniteurs SEULEMENT si l'option Moniteur est coché.";

									if (erreur.Count != 0 && doublon.Count == 0)
									{
										found = true;

										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, erreur));
										bw.Flush();
									}
									else if (erreur.Count == 0 && doublon.Count != 0)
									{
										message = "*Avertissement* Aucune Modification pour les numeros de série restant car ils existent deja dans l'inventaire.";
										found = true;
										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, doublon));
										bw.Flush();
									}
									else if (erreur.Count != 0 && doublon.Count != 0)
									{
										message += Environment.NewLine + "*Avertissement* Les numeros de serie suivant sont deja dans l'inventaire et n'ont pas été mofifiés :" + Environment.NewLine + String.Join(", ", doublon);
										found = true;
										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, erreur));
										bw.Flush();
									}
								}

								bw.Write(IM_AjoutEnd);
								bw.Write(found);
								bw.Flush();

								if (temp.Count != 0) UpdateLogs(userInfo.UserName + " - Ajout à l'inventaire: " + temp.Count.ToString() + " entrée(s).", "Ajout d'équipement", temp.Count.ToString(), userInfo.UserName);
							}
							break;

						case IM_Sortie:
							{
								string serial = br.ReadString();
								string RF = br.ReadString();
								string emp = br.ReadString();

								var result = serial.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
								List<string> erreur = new List<string>();
								List<string> notHere = new List<string>();
								List<InvPostes> toAdd = new List<InvPostes>();
								var dateSortie = DateTime.Now.ToShortDateString();
								string message = "";
								bool found = false;
								bool check = false;

								string statut = "";
								string fullDate = "";
								string newRF = "";

								lock (Program.appData.lockDB)
								{
									foreach (var stuff in result)
									{
										check = false;

										foreach (var item in Program.appData.invPostes.ToArray())
										{
											if (stuff == item.serial || stuff == item.asset)
											{
												if (item.statut == "Sortie" || item.statut == "Au Lab" || item.statut == "Réservé") erreur.Add(item.serial);
												else
												{
													statut = "Sortie";

													if (!string.IsNullOrEmpty(item.dateSortie)) fullDate = item.dateSortie + Environment.NewLine + dateSortie;
													else fullDate = dateSortie;

													if (!string.IsNullOrEmpty(item.RF)) newRF = item.RF + Environment.NewLine + RF;
													else newRF = RF;

													item.dateSortie = fullDate;
													item.statut = statut;
													item.emplacement = emp;
													item.RF = newRF;
													item.xcolor = "";
													item.infoSortie.Add(dateSortie + " - " + RF + " - " + userInfo.UserName);

													toAdd.Add(item);
												}

												check = true;
												break;
											}
										}

										if (!check) notHere.Add(stuff);
									}

									if (toAdd.Count != 0)
									{
										prog.SaveDatabase();

										var jsonString = JsonSerializer.Serialize(toAdd);

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_UpdateExisting);
													user.Connection.bw.Write(jsonString);
													user.Connection.bw.Flush();
												}
											}
										}
									}

									if (notHere.Count != 0) message = "*Avertissement* Les numeros de serie suivant restant ne sont pas en inventaire, Tentez de les ajouters avant de réessayer la sortie pour ceux-ci.";

									if (notHere.Count != 0 && erreur.Count == 0)
									{
										found = true;

										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, notHere));
										bw.Flush();
									}
									else if (erreur.Count != 0 && notHere.Count == 0)
									{
										message = "*Avertissement* Les numeros de serie restant sont déja sortie ou sont toujours Au Lab ou sont 'Réservé' pour la préparation.";
										found = true;
										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, erreur));
										bw.Flush();
									}
									else if (erreur.Count != 0 && notHere.Count != 0)
									{
										message += Environment.NewLine + "*Avertissement* Les numeros de serie suivant sont deja 'Sortie' ou sont toujours 'Au Lab' ou sont 'Réservé' pour la préparation et n'ont pas été mofifiés :" + Environment.NewLine + String.Join(", ", erreur);
										found = true;
										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, notHere));
										bw.Flush();
									}

									bw.Write(IM_AjoutEnd);
									bw.Write(found);
									bw.Flush();

									if (toAdd.Count != 0) UpdateLogs(userInfo.UserName + " - Sortie de " + toAdd.Count.ToString() + " équipement(s) sur le " + RF + ".", "Sortie d'équipement", toAdd.Count.ToString(), userInfo.UserName);
								}
							}
							break;

						case IM_Retour:
							{
								string serial = br.ReadString();
								string RF = br.ReadString();
								string emp = br.ReadString();

								var result = serial.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
								List<string> erreur = new List<string>();
								List<string> notHere = new List<string>();
								List<InvPostes> temp = new List<InvPostes>();
								List<InvPostes> info = new List<InvPostes>();
								List<string> problem = new List<string>();
								bool newStuff = false;
								bool error = false;
								var dateEntry = DateTime.Now.ToShortDateString();
								string message2 = "";
								bool add = true;

								lock (Program.appData.lockDB)
								{
									foreach (var stuff in result)
									{
										add = true;

										foreach (var item in Program.appData.invPostes.ToArray())
										{
											if (stuff == item.serial || stuff == item.asset)
											{
												if (item.type != "Moniteur" && (item.statut == "En Stock" || item.statut == "Au Lab" || item.statut == "Réservé"))
												{
													erreur.Add(stuff);
												}
                                                else
												{
													if (string.IsNullOrEmpty(item.RFretour)) item.RFretour = RF;
													else item.RFretour = item.RFretour + Environment.NewLine + RF;

													if (string.IsNullOrEmpty(item.dateRetour)) item.dateRetour = dateEntry;
													else item.dateRetour = item.dateRetour + Environment.NewLine + dateEntry;

													item.statut = "En Stock";
													item.emplacement = emp;

													if (emp != "R2GO" || emp != "OPER")
													{
														if (!string.IsNullOrEmpty(item.dateClone) || !string.IsNullOrEmpty(item.dateCloneValid))
														{
															item.dateClone = "";
															item.dateCloneValid = "";
														}
													}

													item.infoRetour.Add(dateEntry + " - " + RF + " - " + userInfo.UserName);
													info.Add(item);
												}

                                                add = false;
												break;
											}
										}

										if (add) notHere.Add(stuff);
									}

									if (info.Count != 0)
                                    {
										var jsonString = JsonSerializer.Serialize(info);

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_UpdateExisting);
													user.Connection.bw.Write(jsonString);
													user.Connection.bw.Flush();
												}
											}
										}
									}

									if (notHere.Count != 0)
									{
										bool foundType = false;
										bool found = false;

										foreach (var item in notHere)
										{
											found = false;

											foreach (var exist in Program.appData.fichierAchat.ToArray())
											{
												foundType = false;

												if (item == exist.serial || item == exist.asset)
												{
													Program.appData.invPostes.Add(new InvPostes { type = exist.type, model = exist.model, serial = exist.serial, asset = exist.asset, dateEntry = dateEntry, statut = "En Stock", emplacement = emp, RFretour = RF, dateRetour = dateEntry, infoAjout = userInfo.UserName, infoRetour = new List<string>() { dateEntry + " - " + RF + " - " + userInfo.UserName } });
													temp.Add(new InvPostes { type = exist.type, model = exist.model, serial = exist.serial, asset = exist.asset, dateEntry = dateEntry, statut = "En Stock", emplacement = emp, RFretour = RF, dateRetour = dateEntry, infoAjout = userInfo.UserName, infoRetour = new List<string>() { dateEntry + " - " + RF + " - " + userInfo.UserName } });
													found = true;

													lock (Program.appData.lockModel)
													{
														foreach (var types in Program.appData.typesModels.ToArray())
														{

															if (types.type == exist.type)
															{
																foundType = true;

																if (!types.modeles.Contains(exist.model))
																{
																	types.modeles.Add(exist.model);
																	newStuff = true;
																}

																break;
															}
														}

														if (!foundType)
														{
															Program.appData.typesModels.Add(new TypeModel { type = exist.type, modeles = new List<string> { exist.model } });
															newStuff = true;
														}
													}

													break;
												}
											}

											if (!found) problem.Add(item);
										}
									}

									prog.SaveDatabase();

									if (temp.Count != 0)
									{
										var jsonString = JsonSerializer.Serialize(temp);

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_Update);
													user.Connection.bw.Write(jsonString);
													user.Connection.bw.Flush();
												}
											}
										}
									}

									if (newStuff) updateClientModel();

									if (erreur.Count != 0 && problem.Count == 0)
									{
										message2 = "*Avertissement* Modification effectué, mais les numeros de série suivant n'étaient pas en état 'Sortie' dans l'inventraire: ";
										error = true;

										bw.Write(IM_Doublon);
										bw.Write(message2 + String.Join(",", erreur));
										bw.Write("");
										bw.Flush();
									}

									if (erreur.Count == 0 && problem.Count != 0)
									{
										message2 = "*Avertissement* Auncune modification, les numéros de série restant sont introuvable dans l'inventaire et le fichier globale, voir avec Patrick Massouh.";
										error = true;

										bw.Write(IM_Doublon);
										bw.Write(message2);
										bw.Write(String.Join(Environment.NewLine, problem));
										bw.Flush();
									}

									if (erreur.Count != 0 && problem.Count != 0)
									{
										string message = "*Avertissement* Modification effectué, mais les numeros de série suivant n'étaient pas en etat 'Sortie' dans l'inventraire :";
										message2 = "*Avertissement* Auncune modification, les numéros de série restant sont introuvable dans l'inventaire et le fichier globale, voir avec Patrick Massouh.";
										error = true;

										bw.Write(IM_Doublon);
										bw.Write(message2 + Environment.NewLine + message + String.Join(",", erreur));
										bw.Write(String.Join(Environment.NewLine, problem));
										bw.Flush();
									}

									bw.Write(IM_AjoutEnd);
									bw.Write(error);
									bw.Flush();

									if (temp.Count != 0 || info.Count != 0) UpdateLogs(userInfo.UserName + " - Retour de " + (temp.Count + info.Count).ToString() + " équipement(s) sur le " + RF + ".", "Retour d'équipement", (temp.Count + info.Count).ToString(), userInfo.UserName);
								}
							}
							break;

						case IM_EnvoyerLab:
							{
								string serial = br.ReadString();
								bool check = br.ReadBoolean();

								var result = serial.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

								List<string> notHere = new List<string>();
								List<string> sortie = new List<string>();

								List<InvPostes> listFound = new List<InvPostes>();

								string time = DateTime.Now.ToShortDateString();
								string message = "";
								string listSN = "";
								bool exist = false;
								bool erreur = false;
								
								lock (Program.appData.lockDB)
								{
									foreach (var stuff in result)
									{
										exist = false;

										foreach (var item in Program.appData.invPostes.ToArray())
										{
											if ((stuff == item.serial || stuff == item.asset))
											{
												if (item.emplacement != "Sortie") item.emplacement = "Au Lab";
												else 
                                                {
													sortie.Add(stuff);
													exist = true;
													break;
												}

												if (!string.IsNullOrEmpty(item.dateEntryLab))
												{
													item.dateEntryLab = item.dateEntryLab + Environment.NewLine + time;
												}
												else item.dateEntryLab = time;

												item.infoEnvoieClone.Add(time + " - " + userInfo.UserName);

												exist = true;
												listFound.Add(item);
												break;
											}
										}

										if (!exist) notHere.Add(stuff);
									}

									prog.SaveDatabase();

									if (listFound.Count != 0)
									{
										var jsonString = JsonSerializer.Serialize(listFound);

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_UpdateExisting);
													user.Connection.bw.Write(jsonString);
													user.Connection.bw.Flush();
												}
											}
										}
									}

									if (notHere.Count != 0 && sortie.Count == 0)
									{
										erreur = true;
										message = "*Avertissement* Aucune Modification pour les numeros de série restant car il ne sont pas dans l'inventaire. Tentez de les ajouters avant de réessayer l'envoie au Lab pour ceux-ci.";
										listSN = String.Join(Environment.NewLine, notHere);

										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(listSN);
										bw.Flush();
									}

									if (notHere.Count == 0 && sortie.Count != 0)
									{
										erreur = true;
										message = "*Avertissement* Aucune Modification pour les numeros de série restant car ils sont 'Sortie' dans l'inventaire.";
										listSN = String.Join(Environment.NewLine, sortie);

										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(listSN);
										bw.Flush();
									}
									
									if (notHere.Count != 0 && sortie.Count != 0)
									{
										erreur = true;
										message = "*Avertissement* Aucune Modification pour les numeros de série restant car il ne sont pas dans l'inventaire. Tentez de les ajouters avant de réessayer l'envoie au Lab pour ceux-ci.";
										string message2 = "*Avertissement* Aucune Modification pour les numeros de série suivant car ils sont 'Sortie' dans l'inventaire: " + string.Join(",", sortie);
										listSN = String.Join(Environment.NewLine, notHere);

										bw.Write(IM_Doublon);
										bw.Write(message + Environment.NewLine + message2);
										bw.Write(listSN);
										bw.Flush();
									}

									bw.Write(IM_AjoutEnd);
									bw.Write(erreur);
									bw.Flush();

									if (check && listFound.Count != 0)
									{
										List<string> send = new List<string>();

										foreach (var item in listFound.ToArray())
										{
											send.Add(item.type + "╚" + item.model + "╚" + item.asset + "╚" + item.serial + "╚" + item.statut + "╚" + item.RF + "╚" + item.RFretour + "╚" + item.emplacement + "╚" + item.dateEntry + "╚" + item.dateSortie + "╚" + item.dateRetour + "╚" + item.dateEntryLab + "╚" + item.dateClone + "╚" + item.dateCloneValid);
										}

										bw.Write(IM_rapportLab);
										bw.Write(String.Join("§σ§", send));
										bw.Flush();
									}

									if (listFound.Count != 0) UpdateLogs(userInfo.UserName + " - Envoie au Lab pour clonage: " + listFound.Count.ToString() + " équipement(s).", "Envoie au Lab", listFound.Count.ToString(), userInfo.UserName);
								}
							}
							break;

						case IM_RetourSpecial:
							{
								string type = br.ReadString();
								string model = br.ReadString();
								string serial = br.ReadString();
								string RF = br.ReadString();
								string emp = br.ReadString();

								var result = serial.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

								List<InvPostes> temp = new List<InvPostes>();
								bool newStuff = false;
								bool foundType = false;
								var dateEntry = DateTime.Now.ToShortDateString();
								bool exist = false;
								string checkup = "";

								lock (Program.appData.lockDB)
								{
									foreach (var stuff in result)
									{
										exist = false;

										foreach (var item in Program.appData.invPostes.ToArray())
                                        {
											if (item.serial == stuff || item.asset == stuff)
                                            {
												exist = true;
												break;
                                            }
                                        }

										if (!exist)
                                        {
											if (RF != "") checkup = dateEntry;
											else checkup = "";

											Program.appData.invPostes.Add(new InvPostes { serial = stuff, type = type, statut = "En Stock", model = model, dateEntry = dateEntry, RFretour = RF, emplacement = emp, dateRetour = checkup, infoAjout = userInfo.UserName, infoRetour = new List<string>() { dateEntry + " - " + userInfo.UserName } });
											temp.Add(new InvPostes { serial = stuff, type = type, statut = "En Stock", model = model, dateEntry = dateEntry, RFretour = RF, emplacement = emp, dateRetour = dateEntry });
										}
									}

									prog.SaveDatabase();

									lock (Program.appData.lockModel)
									{
										foreach (var types in Program.appData.typesModels.ToArray())
										{
											if (types.type == type)
											{
												foundType = true;

												if (!types.modeles.Contains(model))
												{
													types.modeles.Add(model);
													newStuff = true;
												}

												break;
											}
										}

										if (!foundType)
										{
											Program.appData.typesModels.Add(new TypeModel { type = type, modeles = new List<string> { model } });
											newStuff = true;
										}
									}

									var jsonString = JsonSerializer.Serialize(temp);

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_Update);
												user.Connection.bw.Write(jsonString);
												user.Connection.bw.Flush();
											}
										}
									}

									if (newStuff) updateClientModel();

									bw.Write(IM_AjoutEnd);
									bw.Write(false);
									bw.Flush();

									UpdateLogs(userInfo.UserName + " - Retour Spécial de " + temp.Count.ToString() + " équipement(s) sur le " + RF + ".", "Retour Spécial", temp.Count.ToString(), userInfo.UserName);
								}
							}
							break;

						case IM_Emplacement:
							{
								string serial = br.ReadString();
								string emp = br.ReadString();

								var result = serial.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
								List<string> erreur = new List<string>();
								List<string> notHere = new List<string>();
								List<InvPostes> toSend = new List<InvPostes>();
								var time = DateTime.Now.ToShortDateString();
								
								string message = "";
								bool found = false;
								bool check = false;

								lock (Program.appData.lockDB)
								{
									foreach (var stuff in result)
									{
										check = false;

										foreach (var item in Program.appData.invPostes.ToArray())
										{
											if (stuff == item.serial || stuff == item.asset)
											{
												if (item.statut == "Sortie")
												{
													erreur.Add(stuff);
												}
												else
												{
													item.infoEmp.Add(time + " - Ancien: " + item.emplacement + ", Nouveau: " + emp + " - "  + userInfo.UserName);
													item.emplacement = emp;

													if (emp == "QUANTUM" || emp.ToLower().Contains("repair")) item.xcolor = "";

													toSend.Add(item);
												}

												check = true;
												break;
											}
										}

										if (!check) notHere.Add(stuff);
									}

									prog.SaveDatabase();

									if (toSend.Count != 0)
									{
										var jsonString = JsonSerializer.Serialize(toSend);

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_UpdateExisting);
													user.Connection.bw.Write(jsonString);
													user.Connection.bw.Flush();
												}
											}
										}
									}

									if (notHere.Count != 0 && erreur.Count == 0)
									{
										found = true;
										message = "*Aucune modification*, les numeros de serie Restant car ils ne sont pas en inventaire.";

										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, notHere));
										bw.Flush();
									}

									if (erreur.Count != 0 && notHere.Count == 0)
									{
										message = "*Aucune modification*, ca les numeros de serie Restant sont 'Sortie'.";
										found = true;
										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(String.Join(Environment.NewLine, erreur));
										bw.Flush();
									}

									if (erreur.Count != 0 && notHere.Count != 0)
									{
										message = "*Aucune modification*, les numeros de serie Restant car ils ne sont pas en inventaire.";
										string message2 = "*Aucune modification*, ca les numeros de serie suivant sont 'Sortie' :" + Environment.NewLine + String.Join(", ", erreur);
										found = true;
										bw.Write(IM_Doublon);
										bw.Write(message + Environment.NewLine + message2);
										bw.Write(String.Join(Environment.NewLine, notHere));
										bw.Flush();
									}

									bw.Write(IM_AjoutEnd);
									bw.Write(found);
									bw.Flush();

									if (erreur.Count == 0 && notHere.Count == 0 && result.Count() != 0) UpdateLogs(userInfo.UserName + " - Changement d'emplacement: " + result.Count().ToString() + " équipement(s) Déplacé a l'emplacement: " + emp + ".", "Changement d'emplacement", result.Count().ToString(), userInfo.UserName);
								}
							}
							break;

						case IM_ConfirmCLone:
							{
								string serial = br.ReadString();

								var result = serial.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

								bool check = false;
								bool erreur = false;

								lock (Program.appData.lockDB)
								{
									DateTime date = DateTime.Now;
									List<string> notHere = new List<string>();
									List<InvPostes> modif = new List<InvPostes>();

									foreach (var stuff in result)
									{
										check = false;

										foreach (var item in Program.appData.invPostes.ToArray())
										{
											if ((stuff == item.serial || stuff == item.asset) && item.emplacement == "Au Lab")
											{
												item.emplacement = "R2GO";
												item.xcolor = "";

												if (!string.IsNullOrEmpty(item.dateClone)) item.dateClone = item.dateClone + Environment.NewLine + date.ToShortDateString();
												else item.dateClone = date.ToShortDateString();

												if (!string.IsNullOrEmpty(item.dateCloneValid)) item.dateCloneValid = item.dateCloneValid + Environment.NewLine + date.AddDays(25).ToShortDateString();
												else item.dateCloneValid = date.AddDays(25).ToShortDateString();

												item.infoValidClone.Add(date.ToShortDateString() + " - " + userInfo.UserName);

												modif.Add(item);
												check = true;
												break;
											}
										}

										if (!check) notHere.Add(stuff);
									}

									prog.SaveDatabase();

									if (notHere.Count != 0)
									{
										erreur = true;
										string message = "*Avertissement* Aucune Modification pour les numeros de série restant car il ne sont pas 'Au Lab'.";
										string listSN = String.Join(Environment.NewLine, notHere);

										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(listSN);
										bw.Flush();
									}

									if (modif.Count != 0)
									{
										var jsonString = JsonSerializer.Serialize(modif);

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_UpdateExisting);
													user.Connection.bw.Write(jsonString);
													user.Connection.bw.Flush();
												}
											}
										}
									}

									bw.Write(IM_AjoutEnd);
									bw.Write(erreur);
									bw.Flush();

									if (modif.Count != 0) UpdateLogs(userInfo.UserName + " - Confirmation du Clonage: " + modif.Count.ToString() + " équipement(s).", "Validation clonage", modif.Count.ToString(), userInfo.UserName);
								}
							}
							break;

						case IM_Modify:
							{
								string info = br.ReadString();
								var modify = info.Split("§");

								string[] ligne;

								lock (Program.appData.lockDB)
								{
									foreach (var item in modify)
									{
										ligne = item.Split("╚");

										foreach (var change in Program.appData.invPostes.ToArray())
										{
											if (ligne[0] == change.serial)
											{
												change.statut = ligne[1];
												change.RF = ligne[2];
												change.dateSortie = ligne[3];
												change.RFretour = ligne[4];
												change.dateRetour = ligne[5];
												change.dateEntryLab = ligne[6];

												if (ligne[7].ToLower() == "au lab") change.emplacement = "Au Lab";
												else change.emplacement = ligne[7];

												break;
											}
										}
									}

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_Modify);
												user.Connection.bw.Write(info);
												user.Connection.bw.Flush();
											}
										}
									}

									prog.SaveDatabase();

									if (modify.Count() != 0) UpdateLogs(userInfo.UserName + " - Modification de " + modify.Count().ToString() + " entrée(s) dans la database.", "Modification database", modify.Count().ToString(), userInfo.UserName);
								}
							}
							break;

						case IM_CheckPreparation:
							{
								bool valeur = br.ReadBoolean();
								string semaine = br.ReadString();
								string transit = br.ReadString();

								lock (Program.appData.lockPrepare)
								{
									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == semaine)
										{
											foreach (var trans in item.info)
											{
												if (trans.transit == transit)
												{
													trans.ready = valeur;
													break;
												}
											}
										}
									}

									prog.SavePreparation();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												lock (Program.appData.lockWrite)
												{
													user.Connection.bw.Write(IM_CheckPreparation);
													user.Connection.bw.Write(valeur);
													user.Connection.bw.Write(semaine);
													user.Connection.bw.Write(transit);
													user.Connection.bw.Flush();
												}
											}
										}
									}
								}

								if (valeur) UpdateLogs(userInfo.UserName + " - Le transit " + transit + " de la " + semaine + " à été coché.", "Préparation", "", userInfo.UserName);
								else UpdateLogs(userInfo.UserName + " - Le transit " + transit + " de la " + semaine + " à été décoché.", "Préparation", "", userInfo.UserName);
							}
							break;

						case IM_SelectedPreparation:
							{
								string semaine = br.ReadString();
								string transit = br.ReadString();
								bool valeur = br.ReadBoolean();

								lock (Program.appData.lockPrepare)
								{

									int count = 0;

									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == semaine)
										{
											foreach (var trans in item.info)
											{
												if (trans.transit == transit)
												{
													foreach (var update in Program.appData.selectedTransit.ToArray())
													{
														if (update.transit == transit)
														{
															if (valeur)
															{
																update.count++;
																trans.selected = valeur;
															}
															else
															{
																update.count--;

																if (update.count <= 0)
																{
																	update.count = 0;
																	trans.selected = valeur;
																}
															}

															count = update.count;
															break;
														}
													}

													break;
												}
											}
										}
									}

									prog.SavePreparation();
									
									if ((valeur && count > 0) || (!valeur && count == 0))
									{
										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													lock (Program.appData.lockWrite)
                                                    {
														user.Connection.bw.Write(IM_SelectedPreparation);
														user.Connection.bw.Write(semaine);
														user.Connection.bw.Write(transit);
														user.Connection.bw.Write(valeur);
														user.Connection.bw.Flush();
													}
												}
											}
										}
									}
								}
							}
							break;

						case IM_CheckPreparationAcess:
							{
								string info = br.ReadString();
								bool valeur = br.ReadBoolean();
								string semaine = br.ReadString();
								string transit = br.ReadString();

								lock (Program.appData.lockPrepare)
								{
									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == semaine)
										{
											foreach (var trans in item.info)
											{
												if (trans.transit == transit)
												{
													if (info == "clavier") trans.clavierCheck = valeur;
													if (info == "sourisSansFil") trans.sourisSansFilCheck = valeur;
													if (info == "cableSecure") trans.cableSecureCheck = valeur;
													if (info == "sac") trans.sacCheck = valeur;
													if (info == "sacBandou") trans.sacBandouCheck = valeur;
													if (info == "hubUsb") trans.hubUsbCheck = valeur;
													if (info == "usbcDP") trans.usbcDPCheck = valeur;
													if (info == "livret") trans.livretCheck = valeur;
													break;
												}
											}
										}
									}

									prog.SavePreparation();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												lock (Program.appData.lockWrite)
                                                {
													user.Connection.bw.Write(IM_CheckPreparationAcess);
													user.Connection.bw.Write(info);
													user.Connection.bw.Write(valeur);
													user.Connection.bw.Write(semaine);
													user.Connection.bw.Write(transit);
													user.Connection.bw.Flush();
												}
											}
										}
									}
								}
							}
							break;

						case IM_PreparationSortie:
							{
								string semaine = br.ReadString();
								string transit = br.ReadString();
								string rf = br.ReadString();
								string emp = br.ReadString();

								var dateSortie = DateTime.Now.ToShortDateString();

								string fullDate = "";
								string newRF = "";

								List<InvPostes> temp = new List<InvPostes>();

								lock (Program.appData.lockPrepare)
								{
									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == semaine)
										{
											foreach (var trans in item.info)
											{
												if (trans.transit == transit)
												{
													lock (Program.appData.lockDB)
													{
														foreach (var serial in trans.SNmini)
														{
															foreach (var element in Program.appData.invPostes.ToArray())
															{
																if (element.serial == serial.Split(" - ")[1])
																{
																	element.statut = "Sortie";
																	element.emplacement = emp;
																	element.xcolor = "";

																	if (!string.IsNullOrEmpty(element.dateSortie)) fullDate = element.dateSortie + Environment.NewLine + dateSortie;
																	else fullDate = dateSortie;

																	element.dateSortie = fullDate;

																	if (!string.IsNullOrEmpty(element.RF)) newRF = element.RF + Environment.NewLine + rf;
																	else newRF = rf;

																	element.RF = newRF;
																	element.infoSortie.Add(dateSortie + " - " + rf + " - " + userInfo.UserName);

																	temp.Add(element);
																	break;
																}
															}
														}

														foreach (var serial in trans.SNsff)
														{
															foreach (var element in Program.appData.invPostes.ToArray())
															{
																if (element.serial == serial.Split(" - ")[1])
																{
																	element.statut = "Sortie";
																	element.emplacement = emp;
																	element.xcolor = "";

																	if (!string.IsNullOrEmpty(element.dateSortie)) fullDate = element.dateSortie + Environment.NewLine + dateSortie;
																	else fullDate = dateSortie;

																	element.dateSortie = fullDate;

																	if (!string.IsNullOrEmpty(element.RF)) newRF = element.RF + Environment.NewLine + rf;
																	else newRF = rf;

																	element.RF = newRF;
																	element.infoSortie.Add(dateSortie + " - " + rf + " - " + userInfo.UserName);

																	temp.Add(element);
																	break;
																}
															}
														}

														foreach (var serial in trans.SNlaptop)
														{
															foreach (var element in Program.appData.invPostes.ToArray())
															{
																if (element.serial == serial.Split(" - ")[1])
																{
																	element.statut = "Sortie";
																	element.emplacement = emp;
																	element.xcolor = "";

																	if (!string.IsNullOrEmpty(element.dateSortie)) fullDate = element.dateSortie + Environment.NewLine + dateSortie;
																	else fullDate = dateSortie;

																	element.dateSortie = fullDate;

																	if (!string.IsNullOrEmpty(element.RF)) newRF = element.RF + Environment.NewLine + rf;
																	else newRF = rf;

																	element.RF = newRF;
																	element.infoSortie.Add(dateSortie + " - " + rf + " - " + userInfo.UserName);

																	temp.Add(element);
																	break;
																}
															}
														}

														foreach (var serial in trans.SNlcd22)
														{
															foreach (var element in Program.appData.invPostes.ToArray())
															{
																if (element.serial == serial.Split(" - ")[1])
																{
																	element.statut = "Sortie";
																	element.emplacement = emp;
																	element.xcolor = "";

																	if (!string.IsNullOrEmpty(element.dateSortie)) fullDate = element.dateSortie + Environment.NewLine + dateSortie;
																	else fullDate = dateSortie;

																	element.dateSortie = fullDate;

																	if (!string.IsNullOrEmpty(element.RF)) newRF = element.RF + Environment.NewLine + rf;
																	else newRF = rf;

																	element.RF = newRF;
																	element.infoSortie.Add(dateSortie + " - " + rf + " - " + userInfo.UserName);

																	temp.Add(element);
																	break;
																}
															}
														}

														foreach (var serial in trans.SNlcd27)
														{
															foreach (var element in Program.appData.invPostes.ToArray())
															{
																if (element.serial == serial.Split(" - ")[1])
																{
																	element.statut = "Sortie";
																	element.emplacement = emp;
																	element.xcolor = "";

																	if (!string.IsNullOrEmpty(element.dateSortie)) fullDate = element.dateSortie + Environment.NewLine + dateSortie;
																	else fullDate = dateSortie;

																	element.dateSortie = fullDate;

																	if (!string.IsNullOrEmpty(element.RF)) newRF = element.RF + Environment.NewLine + rf;
																	else newRF = rf;

																	element.RF = newRF;
																	element.infoSortie.Add(dateSortie + " - " + rf + " - " + userInfo.UserName);

																	temp.Add(element);
																	break;
																}
															}
														}

														foreach (var serial in trans.SNnip)
														{
															foreach (var element in Program.appData.invPostes.ToArray())
															{
																if (element.serial == serial.Split(" - ")[1])
																{
																	element.statut = "Sortie";
																	element.emplacement = emp;
																	element.xcolor = "";

																	if (!string.IsNullOrEmpty(element.dateSortie)) fullDate = element.dateSortie + Environment.NewLine + dateSortie;
																	else fullDate = dateSortie;

																	element.dateSortie = fullDate;

																	if (!string.IsNullOrEmpty(element.RF)) newRF = element.RF + Environment.NewLine + rf;
																	else newRF = rf;

																	element.RF = newRF;
																	element.infoSortie.Add(dateSortie + " - " + rf + " - " + userInfo.UserName);

																	temp.Add(element);
																	break;
																}
															}
														}

														foreach (var serial in trans.SNrecu)
														{
															foreach (var element in Program.appData.invPostes.ToArray())
															{
																if (element.serial == serial.Split(" - ")[1])
																{
																	element.statut = "Sortie";
																	element.emplacement = emp;
																	element.xcolor = "";

																	if (!string.IsNullOrEmpty(element.dateSortie)) fullDate = element.dateSortie + Environment.NewLine + dateSortie;
																	else fullDate = dateSortie;

																	element.dateSortie = fullDate;

																	if (!string.IsNullOrEmpty(element.RF)) newRF = element.RF + Environment.NewLine + rf;
																	else newRF = rf;

																	element.RF = newRF;
																	element.infoSortie.Add(dateSortie + " - " + rf + " - " + userInfo.UserName);

																	temp.Add(element);
																	break;
																}
															}
														}

														prog.SaveDatabase();
													}

													item.info.Remove(trans);
													UpdateLogs(userInfo.UserName + " - Le transit " + transit + " de la " + semaine + " à été sortie sous la demande: " + rf + " " + "(" + temp.Count.ToString() + " équipements).", "Préparation", temp.Count.ToString(), userInfo.UserName);
													break;
												}
											}

											if (item.info.Count == 0) 
											{
												Program.appData.listPreparation.Remove(item);
												UpdateLogs("Serveur" + " - " + semaine + " enlevée (tous les transits ont été sortie).", "Préparation", "", "Serveur");
											}

											break;
										}

										prog.SavePreparation();
									}

									var jsonString = JsonSerializer.Serialize(temp);

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												lock (Program.appData.lockWrite)
                                                {
													user.Connection.bw.Write(IM_PreparationSortie);
													user.Connection.bw.Write(semaine);
													user.Connection.bw.Write(transit);
													user.Connection.bw.Write(jsonString);
													user.Connection.bw.Flush();
												}
											}
										}
									}

									bw.Write(IM_ConfirmSortiePrep);
									bw.Write(semaine);
									bw.Write(transit);
									bw.Flush();
								}
							}
							break;

						case IM_ForceDisconnect:
							{
								CloseConn();
							}
							break;

						case IM_WBdate:
							{
								if (!string.IsNullOrEmpty(Program.appData.dateWB))
                                {
									bw.Write(IM_WBdate);
									bw.Write(Program.appData.dateWB);
									bw.Flush();
								}
							}
							break;

						case IM_TrackingPuro:
							{
								if (!Program.runAPI) _= prog.puroTracking();
							}
							break;

						case IM_AjoutPreparation:
							{
								string type = br.ReadString();
								string detail = br.ReadString();
								string semaine = br.ReadString();
								string transit = br.ReadString();
								string serial = br.ReadString();

								var result = serial.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

								List<string> toAdd = new List<string>();

								List<string> notFound = new List<string>();
								List<string> erreur = new List<string>();

								bool found = false;
								bool error = false;

								lock (Program.appData.lockDB)
								{
									foreach (var stuff in result)
									{
										found = false;

										foreach (var item in Program.appData.invPostes.ToArray())
										{
											if (item.type == type && (item.serial == stuff || item.asset == serial))
											{
												if (item.statut == "Réservé" || item.statut == "Sortie")
												{
													erreur.Add(stuff);
													found = true;
													break;
												}
												else if (item.statut == "En Stock")
												{
													item.statut = "Réservé";
													item.emplacement = transit;

													if (detail == "nip") toAdd.Add(item.model + " - " + item.serial + " - " + item.asset);
													else toAdd.Add(item.model + " - " + item.serial);
													found = true;
													break;
												}
											}
										}

										if (!found) notFound.Add(stuff);
									}

									if (toAdd.Count != 0)
									{
										lock (Program.appData.lockPrepare)
										{
											foreach (var item in Program.appData.listPreparation.ToArray())
											{
												if (item.semaine == semaine)
												{
													foreach (var trans in item.info)
													{
														if (trans.transit == transit)
														{
															if (detail == "mini") foreach (var SN in toAdd) trans.SNmini.Add(SN);
															if (detail == "sff") foreach (var SN in toAdd) trans.SNsff.Add(SN);
															if (detail == "laptop") foreach (var SN in toAdd) trans.SNlaptop.Add(SN);
															if (detail == "lcd22") foreach (var SN in toAdd) trans.SNlcd22.Add(SN);
															if (detail == "lcd27") foreach (var SN in toAdd) trans.SNlcd27.Add(SN);
															if (detail == "nip") foreach (var SN in toAdd) trans.SNnip.Add(SN);
															if (detail == "recu") foreach (var SN in toAdd) trans.SNrecu.Add(SN);

															break;
														}
													}

													break;
												}
											}

											prog.SavePreparation();
										}

										prog.SaveDatabase();

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_AjoutPreparation);
													user.Connection.bw.Write(detail);
													user.Connection.bw.Write(semaine);
													user.Connection.bw.Write(transit);
													user.Connection.bw.Write(String.Join("§", toAdd));
													user.Connection.bw.Flush();
												}
											}
										}
									}

									if (notFound.Count != 0 || erreur.Count != 0) error = true;

									if (notFound.Count != 0 && erreur.Count == 0)
									{
										string message = "*Avertissement* Les numeros de série restant n'ont pas été ajouté à la préparation car il ne sont pas dans l'inventaire ou pas le bon type de matériel.";
										string listSN = String.Join(Environment.NewLine, notFound);

										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(listSN);
										bw.Flush();
									}

									if (notFound.Count == 0 && erreur.Count != 0)
									{
										string message = "*Avertissement* Les numeros de série restant n'ont pas été ajouté à la préparation car ils sont soit déjà 'Sortie' ou soit 'Réservé'.";
										string listSN = String.Join(Environment.NewLine, erreur);

										bw.Write(IM_Doublon);
										bw.Write(message);
										bw.Write(listSN);
										bw.Flush();
									}

									if (notFound.Count != 0 && erreur.Count != 0)
									{
										string message = "*Avertissement* Les numeros de série restant n'ont pas été ajouté à la préparation car ils sont soit déjà 'Sortie' ou soit 'Réservé'.";
										string msg2 = "*Avertissement* Les numeros de série suivant n'ont pas été ajouté à la préparation car il ne sont pas dans l'inventaire ou pas le bon type de matériel: " + string.Join(",", notFound);
										string listSN = String.Join(Environment.NewLine, notFound);

										bw.Write(IM_Doublon);
										bw.Write(message + Environment.NewLine + msg2);
										bw.Write(listSN);
										bw.Flush();
									}

									bw.Write(IM_AjoutEnd);
									bw.Write(error);
									bw.Flush();

									if (toAdd.Count != 0) UpdateLogs(userInfo.UserName + " - Ajout de " + toAdd.Count.ToString() + " équipements (" + detail +  ") pour le transit " + transit + " de la " + semaine + ".", "Préparation", toAdd.Count.ToString(), userInfo.UserName);
								}
							}
							break;

						case IM_DeletePreparation:
							{
								string detail = br.ReadString();
								string semaine = br.ReadString();
								string transit = br.ReadString();
								string serial = br.ReadString();

								var serialData = serial.Split("§");

								lock (Program.appData.lockDB)
								{
									foreach (var SN in serialData)
									{
										foreach (var item in Program.appData.invPostes.ToArray())
										{
                                            if (SN == item.serial && item.statut != "Sortie")
                                            {
                                                item.statut = "En Stock";

												if (item.type == "Moniteur" || item.type == "Clavier nip" || item.type.ToLower().Contains("imprimante")) item.emplacement = "ALL";
												else item.emplacement = "R2GO";

                                                break;
                                            }
                                        }
									}

									prog.SaveDatabase();
								}

								lock (Program.appData.lockPrepare)
								{
									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == semaine)
										{
											foreach (var trans in item.info)
											{
												if (trans.transit == transit)
												{
													if (detail == "mini")
													{
														foreach (var SN in serialData)
														{
															foreach (var del in trans.SNmini.ToArray())
															{
																if (del.Contains(SN)) trans.SNmini.Remove(del);
															}
														}
													}

													if (detail == "sff")
													{
														foreach (var SN in serialData)
														{
															foreach (var del in trans.SNsff.ToArray())
															{
																if (del.Contains(SN)) trans.SNsff.Remove(del);
															}
														}
													}

													if (detail == "laptop")
													{
														foreach (var SN in serialData)
														{
															foreach (var del in trans.SNlaptop.ToArray())
															{
																if (del.Contains(SN)) trans.SNlaptop.Remove(del);
															}
														}
													}

													if (detail == "lcd22")
													{
														foreach (var SN in serialData)
														{
															foreach (var del in trans.SNlcd22.ToArray())
															{
																if (del.Contains(SN)) trans.SNlcd22.Remove(del);
															}
														}
													}

													if (detail == "lcd27")
													{
														foreach (var SN in serialData)
														{
															foreach (var del in trans.SNlcd27.ToArray())
															{
																if (del.Contains(SN)) trans.SNlcd27.Remove(del);
															}
														}
													}

													if (detail == "nip")
													{
														foreach (var SN in serialData)
														{
															foreach (var del in trans.SNnip.ToArray())
															{
																if (del.Contains(SN)) trans.SNnip.Remove(del);
															}
														}
													}

													if (detail == "recu")
													{
														foreach (var SN in serialData)
														{
															foreach (var del in trans.SNrecu.ToArray())
															{
																if (del.Contains(SN)) trans.SNrecu.Remove(del);
															}
														}
													}

													break;
												}
											}

											break;
										}
									}

									prog.SavePreparation();
								}

								foreach (var UserKey in prog.users.Keys)
								{
									if (prog.users.TryGetValue(UserKey, out UserInfo user))
									{
										if (user.LoggedIn)
										{
											lock (Program.appData.lockWrite)
                                            {
												user.Connection.bw.Write(IM_DeletePreparation);
												user.Connection.bw.Write(detail);
												user.Connection.bw.Write(semaine);
												user.Connection.bw.Write(transit);
												user.Connection.bw.Write(serial);
												user.Connection.bw.Flush();
											}
										}
									}
								}

								UpdateLogs(userInfo.UserName + " - Retrait de " + serialData.Count().ToString() + " équipements (" + detail + ") pour le transit " + transit + " de la " + semaine + ".", "Préparation", serialData.Count().ToString(), userInfo.UserName);
							}
							break;

						case IM_PreparationDeleteTransit:
							{
								string semaine = br.ReadString();
								string transit = br.ReadString();

								lock (Program.appData.lockPrepare)
								{
									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == semaine)
										{
											foreach (var tans in item.info)
											{
												if (tans.transit == transit)
												{
													item.info.Remove(tans);
													UpdateLogs(userInfo.UserName + " - Supression du transit " + transit + "  de la " + semaine + ".", "Préparation", "", userInfo.UserName);
													break;
												}
											}

											if (item.info.Count == 0) 
											{
												Program.appData.listPreparation.Remove(item);
												UpdateLogs("Serveur" + " - " + semaine + " enlevée (le dernier transit de cette semaine a été supprimé).", "Préparation", "", "Serveur");
											}

											break;
										}
									}

									prog.SavePreparation();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												lock (Program.appData.lockWrite)
                                                {
													user.Connection.bw.Write(IM_PreparationDeleteTransit);
													user.Connection.bw.Write(semaine);
													user.Connection.bw.Write(transit);
													user.Connection.bw.Flush();
												}
											}
										}
									}
								}
							}
							break;

						case IM_CommentPreparation:
							{
								string semaine = br.ReadString();
								string transit = br.ReadString();
								string info = br.ReadString();

								lock (Program.appData.lockPrepare)
								{
									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == semaine)
										{
											foreach (var tans in item.info)
											{
												if (tans.transit == transit)
												{
													tans.comment = info;
													UpdateLogs(userInfo.UserName + " - Ajout/Modification de commentaire pour le transit " + transit + "  de la " + semaine + ".", "Préparation", "", userInfo.UserName);
													break;
												}
											}

											break;
										}
									}

									prog.SavePreparation();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_CommentPreparation);
												user.Connection.bw.Write(semaine);
												user.Connection.bw.Write(transit);
												user.Connection.bw.Write(info);
												user.Connection.bw.Flush();
											}
										}
									}
								}
							}
							break;

						case IM_PreparationRF:
							{
								string semaine = br.ReadString();
								string transit = br.ReadString();
								string info = br.ReadString();

								lock (Program.appData.lockPrepare)
								{
									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == semaine)
										{
											foreach (var tans in item.info)
											{
												if (tans.transit == transit)
												{
													tans.rf = info;
													UpdateLogs(userInfo.UserName + " - Ajout / Modification de RF pour le transit " + transit + "  de la " + semaine + ".", "Préparation", "", userInfo.UserName);
													break;
												}
											}

											break;
										}
									}

									prog.SavePreparation();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												lock (Program.appData.lockWrite)
                                                {
													user.Connection.bw.Write(IM_PreparationRF);
													user.Connection.bw.Write(semaine);
													user.Connection.bw.Write(transit);
													user.Connection.bw.Write(info);
													user.Connection.bw.Flush();
												}
											}
										}
									}
								}
							}
							break;

						case IM_MainComment:
							{
								string serial = br.ReadString();
								string info = br.ReadString();

								lock (Program.appData.lockDB)
								{
									foreach (var item in Program.appData.invPostes.ToArray())
									{
										if (item.serial == serial)
                                        {
											item.comment = info;
											break;
                                        }
									}

									prog.SaveDatabase();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_MainComment);
												user.Connection.bw.Write(serial);
												user.Connection.bw.Write(info);
												user.Connection.bw.Flush();
											}
										}
									}
								}
								
								UpdateLogs(userInfo.UserName + " - Ajout/Modification de commentaire pour : " + serial, "Commentaire", info, userInfo.UserName);
							}
							break;

						case IM_ModeleMoniteur:
							{
								lock (Program.appData.lockModel)
								{
									if (Program.appData.modeleMoniteur.Count != 0)
									{
										var modele = string.Join("╚", Program.appData.modeleMoniteur);

										bw.Write(IM_ModeleMoniteur);
										bw.Write(modele);
										bw.Flush();
									}
								}
							}
							break;

						case IM_ModifyPreparation:
							{
								string action = br.ReadString();
								string semaine = br.ReadString();
								string info = br.ReadString();

								var valeur = JsonSerializer.Deserialize<List<Contenu>>(info);
								var contenu = valeur[0];

								lock (Program.appData.lockPrepare)
								{
									if (action == "add")
									{
										foreach (var item in Program.appData.listPreparation.ToArray())
										{
											if (item.semaine == semaine)
											{
												item.info.Add(contenu);
												break;
											}
										}
									}

									if (action == "modify")
									{
										foreach (var item in Program.appData.listPreparation)
										{
											if (item.semaine == semaine)
											{
												foreach (var trans in item.info)
												{
													if (trans.transit == contenu.transit)
													{
														trans.mini = contenu.mini;
														trans.laptop = contenu.laptop;
														trans.sff = contenu.sff;
														trans.nip = contenu.nip;
														trans.recu = contenu.recu;
														trans.lcd22 = contenu.lcd22;
														trans.lcd27 = contenu.lcd27;
														trans.hubUsb = contenu.hubUsb;
														trans.cableSecure = contenu.cableSecure;
														trans.clavier = contenu.clavier;
														trans.livret = contenu.livret;
														trans.sac = contenu.sac;
														trans.sacBandou = contenu.sacBandou;
														trans.sourisSansFil = contenu.sourisSansFil;
														trans.usbcDP = contenu.usbcDP;
														break;
													}
												}

												break;
											}
										}
									}

									prog.SavePreparation();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_ModifyPreparation);
												user.Connection.bw.Write(action);
												user.Connection.bw.Write(semaine);
												user.Connection.bw.Write(info);
												user.Connection.bw.Flush();
											}
										}
									}
								}
							}
							break;

						case IM_RequestPreparation:
							{
								lock (Program.appData.lockPrepare)
								{
									if (Program.appData.listPreparation.Count != 0)
									{
										var jsonString = JsonSerializer.Serialize(Program.appData.listPreparation);

										bw.Write(IM_RequestPreparation);
										bw.Write(jsonString);
										bw.Flush();
									}
								}
							}
							break;

						case IM_Comment:
							{
								string rf = br.ReadString();
								string wb = br.ReadString();
								string info = br.ReadString();

								lock (Program.appData.lockWB)
								{
									foreach (var item in Program.appData.waybills.ToArray())
									{
										if (item.RF == rf && item.wayb == wb)
										{
											item.comment = info;
											break;
										}
									}

									prog.SaveWB();
								}					

								foreach (var UserKey in prog.users.Keys)
								{
									if (prog.users.TryGetValue(UserKey, out UserInfo user))
									{
										if (user.LoggedIn)
										{
											lock (Program.appData.lockWrite)
                                            {
												user.Connection.bw.Write(IM_Comment);
												user.Connection.bw.Write(rf);
												user.Connection.bw.Write(wb);
												user.Connection.bw.Write(info);
												user.Connection.bw.Flush();
											}
										}
									}
								}
							}
							break;

						case IM_PreparationSemaine:
							{
								string semaine = br.ReadString();

								lock (Program.appData.lockPrepare)
								{
									bool exist = false;

									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == semaine)
										{
											exist = true;
											break;
										}
									}

									if (!exist)
									{
										Program.appData.listPreparation.Add(new Preparation { semaine = semaine });
										prog.SavePreparation();

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													lock (Program.appData.lockWrite)
                                                    {
														user.Connection.bw.Write(IM_PreparationSemaine);
														user.Connection.bw.Write(semaine);
														user.Connection.bw.Flush();
													}
												}
											}
										}
									}
								}
							}
							break;

						case IM_PreparationTransfert:
							{
								string oldSemaine = br.ReadString();
								string newSemaine = br.ReadString();
								string transit = br.ReadString();

								lock (Program.appData.lockPrepare)
								{
									Contenu info = new Contenu();
									bool exist = false;

									foreach (var item in Program.appData.listPreparation.ToArray())
									{
										if (item.semaine == newSemaine)
										{
											foreach (var trans in item.info)
											{
												if (trans.transit == transit)
												{
													exist = true;
													break;
												}
											}
										}
									}

									if (!exist)
									{
										foreach (var item in Program.appData.listPreparation.ToArray())
										{
											if (item.semaine == oldSemaine)
											{
												foreach (var trans in item.info)
												{
													if (trans.transit == transit)
													{
														info = trans;
														item.info.Remove(trans);
														break;
													}
												}
											}
										}

										foreach (var item in Program.appData.listPreparation.ToArray())
										{
											if (item.semaine == newSemaine)
											{
												info.selected = false;
												item.info.Add(info);
												break;
											}
										}

										foreach (var item in Program.appData.selectedTransit.ToArray())
										{
											if (item.transit == transit)
											{
												item.count = 0;
												break;
											}
										}

										prog.SavePreparation();

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													lock (Program.appData.lockWrite)
                                                    {
														user.Connection.bw.Write(IM_PreparationTransfert);
														user.Connection.bw.Write(oldSemaine);
														user.Connection.bw.Write(newSemaine);
														user.Connection.bw.Write(transit);
														user.Connection.bw.Flush();
													}
												}
											}
										}
									}

									if (exist)
									{
										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													lock (Program.appData.lockWrite)
                                                    {
														user.Connection.bw.Write(IM_PreparationTransfert);
														user.Connection.bw.Write("");
														user.Connection.bw.Write(newSemaine);
														user.Connection.bw.Write(transit);
														user.Connection.bw.Flush();
													}
												}
											}
										}
									}
								}
							}
							break;

						case IM_SeuilLCDajout:
							{
								string type = br.ReadString();
								string model = br.ReadString();

								var lcd = model.Split(";");

								lock (lockSeuil)
								{
									Program.appData.moniteurSeuil.Add(new seuilMoniteurAdmin { type = type, modele = new List<string>(lcd), max = "0", alerte = "0" });
									prog.SaveSeuilMoniteur();
								}
							}
							break;

						case IM_SeuilLCDmodif:
							{
								string type = br.ReadString();
								string model = br.ReadString();

								var lcd = model.Split(";");

								lock (lockSeuil)
								{
									foreach (var item in Program.appData.moniteurSeuil.ToArray())
									{
										if (item.type == type)
										{
											item.type = type;
											item.modele = new List<string>(lcd);
											break;
										}
									}

									prog.SaveSeuilMoniteur();
								}					
							}
                            break;

                        case IM_FileUpload:
                            {
                                string name = br.ReadString();
                                string contenu = br.ReadString();
								bool error = false;

                                lock (Program.appData.lockUpload)
                                {
                                    if (name.ToLower().Contains("semaine") || name.ToLower().Contains("shipping"))
                                    {
                                        prog.msg("Loading new semaine/shipping preparation...");

                                        lock (Program.appData.lockPrepare)
                                        {
                                            List<Preparation> send = new List<Preparation>();
                                            bool exist = false;

                                            foreach (var item in Program.appData.listPreparation.ToArray())
                                            {
                                                if (item.semaine == name)
                                                {
                                                    using (StringReader file = new StringReader(contenu))
                                                    {
                                                        string ln;
                                                        string[] valeur = null;
                                                        bool add = true;

                                                        send.Add(new Preparation { semaine = name });

                                                        try 
														{
															while ((ln = file.ReadLine()) != null)
															{
																if (ln.Contains(";")) valeur = ln.Split(";");
																else valeur = ln.Split(",");

																if (!valeur[0].ToLower().Contains("ps"))
																{
																	foreach (var trans in item.info)
																	{
																		add = true;

																		if (trans.transit == valeur[0])
																		{
																			add = false;
																			break;
																		}
																	}

																	if (add)
																	{
																		if (!string.IsNullOrWhiteSpace(valeur[0]))
																		{
																			item.info.Add(new Contenu { transit = valeur[0], mini = valeur[1], sff = valeur[2], laptop = valeur[3], lcd22 = valeur[4], lcd27 = valeur[5], clavier = valeur[6], sourisSansFil = valeur[7], cableSecure = valeur[8], sac = valeur[9], sacBandou = valeur[10], hubUsb = valeur[11], usbcDP = valeur[12], nip = valeur[13], livret = valeur[14], recu = valeur[15] });
																			send[send.Count - 1].info.Add(new Contenu { transit = valeur[0], mini = valeur[1], sff = valeur[2], laptop = valeur[3], lcd22 = valeur[4], lcd27 = valeur[5], clavier = valeur[6], sourisSansFil = valeur[7], cableSecure = valeur[8], sac = valeur[9], sacBandou = valeur[10], hubUsb = valeur[11], usbcDP = valeur[12], nip = valeur[13], livret = valeur[14], recu = valeur[15] });
																			Program.appData.selectedTransit.Add(new SelectedTransit { transit = valeur[0] });
																		}
																	}
																}
															}
														}
														catch (Exception ex)
														{
															error = true;
															prog.msg(ex.Message);
															break;
                                                        }
                                                    }

                                                    exist = true;
                                                    break;
                                                }
                                            }

                                            if (!exist && !error)
                                            {
                                                using (StringReader file = new StringReader(contenu))
                                                {
                                                    string ln;
                                                    string[] valeur = null;

                                                    Program.appData.listPreparation.Add(new Preparation { semaine = name });
                                                    send.Add(new Preparation { semaine = name });

                                                    try
                                                    {
                                                        while ((ln = file.ReadLine()) != null)
                                                        {
                                                            if (ln.Contains(";")) valeur = ln.Split(";");
                                                            else valeur = ln.Split(",");

                                                            if (!valeur[0].ToLower().Contains("ps"))
                                                            {
                                                                if (!string.IsNullOrWhiteSpace(valeur[0]))
                                                                {
                                                                    Program.appData.listPreparation[Program.appData.listPreparation.Count - 1].info.Add(new Contenu { transit = valeur[0], mini = valeur[1], sff = valeur[2], laptop = valeur[3], lcd22 = valeur[4], lcd27 = valeur[5], clavier = valeur[6], sourisSansFil = valeur[7], cableSecure = valeur[8], sac = valeur[9], sacBandou = valeur[10], hubUsb = valeur[11], usbcDP = valeur[12], nip = valeur[13], livret = valeur[14] });
                                                                    send[send.Count - 1].info.Add(new Contenu { transit = valeur[0], mini = valeur[1], sff = valeur[2], laptop = valeur[3], lcd22 = valeur[4], lcd27 = valeur[5], clavier = valeur[6], sourisSansFil = valeur[7], cableSecure = valeur[8], sac = valeur[9], sacBandou = valeur[10], hubUsb = valeur[11], usbcDP = valeur[12], nip = valeur[13], livret = valeur[14] });
                                                                    Program.appData.selectedTransit.Add(new SelectedTransit { transit = valeur[0] });
                                                                }
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        error = true;
														prog.msg(ex.Message);
                                                    }
                                                }
                                            }

											if (error)
                                            {
												bw.Write(IM_UploadNotice);
												bw.Flush();
												break;
                                            }

                                            prog.SavePreparation();
											prog.msg("Loading new semaine preparation/shipping Completed");

											var jsonString = JsonSerializer.Serialize(send);

											foreach (var UserKey in prog.users.Keys)
											{
												if (prog.users.TryGetValue(UserKey, out UserInfo user))
												{
													if (user.LoggedIn)
													{
														user.Connection.bw.Write(IM_NewPreparation);
														user.Connection.bw.Write(jsonString);
														user.Connection.bw.Flush();
													}
												}
											}

											UpdateLogs(userInfo.UserName + " - Ajout de " + name + ".", "Préparation", "", userInfo.UserName);
										}
									}

									if (name.ToLower() == "inventaire")
									{
										lock (Program.appData.lockDB)
										{
											lock (Program.appData.lockModel)
											{
												Program.appData.fichierAchat.Clear();

												prog.msg("Loading new Full Inventaire File...");

												try
												{
													using (StringReader file = new StringReader(contenu))
													{
														string ln;
														string[] valeur;
														string asset = "";
														string model = "";

														while ((ln = file.ReadLine()) != null)
														{
															if (!ln.Contains("EndUser"))
															{
																if (ln.Contains(";")) valeur = ln.Split(";");
																else valeur = ln.Split(",");

																asset = "";

																if (valeur[3].ToLower().StartsWith("b")) asset = valeur[3];
																else if (!valeur[3].ToLower().StartsWith("b") && valeur[2].ToLower().StartsWith("b")) asset = valeur[2];

																if (valeur[1] == "Clavier nip")
																{
																	foreach (var item in Program.appData.nip.ToArray())
																	{
																		if (valeur[6].ToUpper() == item.serial)
																		{
																			asset = item.asset;
																			break;
																		}
																	}
																}

																if (valeur[5].Contains("�")) model = valeur[5].Replace("�", "ç");
																else model = valeur[5];

																Program.appData.fichierAchat.Add(new Achat { type = valeur[1], model = valeur[5], asset = asset, serial = valeur[6].ToUpper() });
															}
														}
													}
												}
												catch (Exception ex)
												{
													prog.msg(ex.Message);
												}
											}
										}

										prog.SaveInventaireDesjardins();
										prog.nipAsset();

										if (Program.appData.nip.Count != 0)
										{
											foreach (var item in Program.appData.nip.ToArray())
											{
												Parallel.ForEach(Program.appData.fichierAchat.ToArray(), (main, state) =>
												{
													if (item.serial == main.serial)
													{
														main.asset = item.asset;
														state.Break();
													}
												});
											}
										}

										prog.msg("Loading new Full Inventaire Completed");
									}

									if (name.ToLower() == "nip")
									{
										lock (Program.appData.lockDB)
										{
											lock (Program.appData.lockModel)
											{
												Program.appData.nip.Clear();
												prog.msg("Loading new NIP File...");

												try
												{
													using (StringReader file = new StringReader(contenu))
													{
														string ln;
														string[] valeur;

														while ((ln = file.ReadLine()) != null)
														{
															if (ln.Contains(";")) valeur = ln.Split(";");
															else valeur = ln.Split(",");

															Program.appData.nip.Add(new Achat { type = "Clavier nip", asset = valeur[1], serial = valeur[0].ToUpper() });

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
												}
												catch (Exception ex)
												{
													prog.msg(ex.Message);
												}
											}
										}

										prog.msg("Loading new NIP file Completed");
										prog.SaveInventaireDesjardinsNIP();
										prog.SaveInventaireDesjardins();
										prog.nipAsset();
									}
								}
							}
							break;

						case IM_SeuilAdminRequest:
							{
								Seuil[] temp;
								seuilMoniteurAdmin[] tempLCD;

								List<string> final = new List<string>();
								List<string> model = new List<string>();

								lock (lockSeuil)
								{
									temp = Program.appData.seuilPoste.ToArray();
									tempLCD = Program.appData.moniteurSeuil.ToArray();
								}

								string posteAlerte = "0";
								string posteMax = "0";

								string lapAlerte = "0";
								string lapMax = "0";

								string tabAlerte = "0";
								string tabMax = "0";

								foreach (Seuil item in temp)
								{
									if (item.type == "Postes")
									{
										posteAlerte = item.alerte;
										posteMax = item.max;
									}

									if (item.type == "Portables")
									{
										lapAlerte = item.alerte;
										lapMax = item.max;
									}

									if (item.type == "Tablettes")
									{
										tabAlerte = item.alerte;
										tabMax = item.max;
									}
								}

								foreach (var item in tempLCD)
								{
									foreach (string info in item.modele)
									{
										model.Add(info);
									}

									final.Add(item.type + ";" + String.Join(",", model) + ";" + item.alerte + ";" + item.max);
									model.Clear();
								}

								bw.Write(IM_SeuilAdminRequest);
								bw.Write(posteAlerte);
								bw.Write(posteMax);
								bw.Write(lapAlerte);
								bw.Write(lapMax);
								bw.Write(tabAlerte);
								bw.Write(tabMax);
								bw.Write(String.Join("|", final));
								bw.Flush();
							}
							break;

						case IM_Seuil:
							{
								InvPostes[] temp;
								seuilMoniteurAdmin[] moniteurTemp;
								List<string> moniteurTempSend = new List<string>();
								List<string> final = new List<string>();

								string maxPoste = "0";
								string alertePoste = "0";
								string maxLap = "0";
								string alerteLap = "0";
								string maxTab = "0";
								string alerteTab = "0";

								lock (Program.appData.lockDB)
								{
									temp = Program.appData.invPostes.ToArray();
								}

								lock (lockSeuil)
								{
									moniteurTemp = Program.appData.moniteurSeuil.ToArray();

									foreach (var item in Program.appData.seuilPoste)
									{
										if (item.type.Contains("Poste"))
										{
											maxPoste = item.max;
											alertePoste = item.alerte;
										}

										if (item.type.Contains("Portable"))
										{
											maxLap = item.max;
											alerteLap = item.alerte;
										}

										if (item.type.Contains("Tablette"))
										{
											maxTab = item.max;
											alerteTab = item.alerte;
										}
									}
								}

								int posteActuel = 0;
								int laptopActuel = 0;
								int tabActuel = 0;

								foreach (var item in temp)
								{
									if (item.type == "Poste de table" && item.statut == "En Stock" && (!item.emplacement.Contains("REPAIR DEPOT") && item.emplacement != "QUANTUM")) posteActuel++;
									if (item.type == "Portable" && item.statut == "En Stock" && (!item.emplacement.Contains("REPAIR DEPOT") && item.emplacement != "QUANTUM")) laptopActuel++;
									if (item.type == "Tablette" && item.statut == "En Stock" && (!item.emplacement.Contains("REPAIR DEPOT") && item.emplacement != "QUANTUM")) tabActuel++;
								}

								final.Add("Poste de Table" + "," + posteActuel.ToString() + "," + alertePoste + "," + maxPoste);
								final.Add("Portable" + "," + laptopActuel.ToString() + "," + alerteLap + "," + maxLap);
								final.Add("Tablette" + "," + tabActuel.ToString() + "," + alerteTab + "," + maxTab);

								int moniteurActuel = 0;

								foreach (var item in moniteurTemp)
								{
									moniteurActuel = 0;

									foreach (var model in item.modele)
									{
										foreach (var main in temp)
										{
											if (model == main.model && main.statut == "En Stock" && (!main.emplacement.Contains("REPAIR DEPOT") && main.emplacement != "QUANTUM")) moniteurActuel++;
										}
									}

									final.Add(item.type + "," + moniteurActuel + "," + item.alerte + "," + item.max);
								}

								bw.Write(IM_Seuil);
								bw.Write(String.Join(";", final));
								bw.Flush();
							}
							break;

						case IM_SeuilAdmin:
							{
								string type = br.ReadString();
								string alerte = br.ReadString();
								string max = br.ReadString();

								bool found = false;

								lock (lockSeuil)
								{
									foreach (var item in Program.appData.seuilPoste.ToArray())
									{
										if (item.type == type)
										{
											item.alerte = alerte;
											item.max = max;
											found = true;
											break;
										}
									}

									if (!found)
									{
										foreach (var item in Program.appData.moniteurSeuil.ToArray())
										{
											if (item.type == type)
											{
												item.alerte = alerte;
												item.max = max;
												break;
											}
										}

										prog.SaveSeuilMoniteur();
									}

									if (found) prog.SaveSeuilPoste();
								}
							}
							break;

						case IM_SeuilDELlcd:
							{
								string type = br.ReadString();
								string alerte = br.ReadString();
								string max = br.ReadString();

								lock (lockSeuil)
								{
									foreach (var item in Program.appData.moniteurSeuil.ToArray())
									{
										if (item.type == type && item.alerte == alerte && item.max == max)
										{
											Program.appData.moniteurSeuil.Remove(item);
											break;
										}
									}

									prog.SaveSeuilMoniteur();
								}
							}
							break;

						case IM_DelAccess:
							{
								string type = br.ReadString();
								string actuel = br.ReadString();
								string alerte = br.ReadString();
								string max = br.ReadString();

								lock (lockAccess)
								{
									foreach (var item in Program.appData.seuilAccess.ToArray())
									{
										if (item.type == type && item.actuel == actuel && item.alerte == alerte && item.max == max)
										{
											Program.appData.seuilAccess.Remove(item);
											break;
										}
									}

									prog.SaveSeuilAccess();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_DelAccess);
												user.Connection.bw.Write(type);
												user.Connection.bw.Write(actuel);
												user.Connection.bw.Write(alerte);
												user.Connection.bw.Write(max);
											}
										}
									}
								}

								UpdateLogs(userInfo.UserName + " - Supression d'un accessoire (" + type + ").", "Seuil", "", userInfo.UserName);
							}
							break;

						case IM_QteAccess:
							{
								string action = br.ReadString();
								string type = br.ReadString();
								string qte = br.ReadString();

								int qteInt = int.Parse(qte);
								int result;

								string newQTE = "";


								lock (lockAccess)
								{
									foreach (var item in Program.appData.seuilAccess.ToArray())
									{
										if (item.type == type)
										{
											if (action == "add")
											{
												item.actuel = (int.Parse(item.actuel) + qteInt).ToString();
												newQTE = item.actuel;
												break;
											}
											else if (action == "del")
											{
												result = int.Parse(item.actuel) - qteInt;
												if (result < 0) result = 0;
												item.actuel = result.ToString();
												newQTE = item.actuel;

												break;
											}
										}
									}

									prog.SaveSeuilAccess();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_QteAccess);
												user.Connection.bw.Write(type);
												user.Connection.bw.Write(newQTE);
											}
										}
									}
								}

								if (action == "add") UpdateLogs(userInfo.UserName + " - Ajout de " + qte + " " + type + ".", "Seuil", qte, userInfo.UserName);
								else UpdateLogs(userInfo.UserName + " - Retrait de " + qte + " " + type + ".", "Seuil", qte, userInfo.UserName);
							}
							break;

						case IM_ModifAccess:
							{
								string type = br.ReadString();
								string alerte = br.ReadString();
								string max = br.ReadString();

								lock (lockAccess)
								{
									foreach (var item in Program.appData.seuilAccess.ToArray())
									{
										if (item.type == type)
										{
											item.alerte = alerte;
											item.max = max;
											break;
										}
									}

									prog.SaveSeuilAccess();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_ModifAccess);
												user.Connection.bw.Write(type);
												user.Connection.bw.Write(alerte);
												user.Connection.bw.Write(max);
											}
										}
									}
								}

								if (max != "0") UpdateLogs(userInfo.UserName + " - Modification de la Qté Max pour " + type + " (" + max + ").", "Seuil", max, userInfo.UserName);
								if (alerte != "0") UpdateLogs(userInfo.UserName + " - Modification de la Qté Alerte pour " + type + " (" + alerte + ").", "Seuil", alerte, userInfo.UserName);
							}
							break;

						case IM_AccessAjout:
							{
								string type = br.ReadString();
								string actuel = br.ReadString();
								string alerte = br.ReadString();
								string max = br.ReadString();

								bool add = true;

								lock (lockAccess)
								{
									foreach (var item in Program.appData.seuilAccess.ToArray())
									{
										if (item.type == type)
										{
											add = false;
											break;
										}
									}

									if (add)
									{
										Program.appData.seuilAccess.Add(new Seuil { type = type, actuel = actuel, alerte = alerte, max = max });
										prog.SaveSeuilAccess();

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_AccessAjout);
													user.Connection.bw.Write(type);
													user.Connection.bw.Write(actuel);
													user.Connection.bw.Write(alerte);
													user.Connection.bw.Write(max);
													user.Connection.bw.Flush();
												}
											}
										}
									}
								}

								UpdateLogs(userInfo.UserName + " - Ajout d'un nouvel accessoire (" + type + ").", "Seuil", "", userInfo.UserName);
							}
							break;

						case IM_AccessRequest:
							{
								List<string> temp = new List<string>();

								lock (lockAccess)
								{
									if (Program.appData.seuilAccess.Count != 0)
									{
										foreach (var item in Program.appData.seuilAccess.ToArray())
										{
											temp.Add(item.type + ";" + item.actuel + ";" + item.alerte + ";" + item.max);
										}

										bw.Write(IM_AccessRequest);
										bw.Write(string.Join("|", temp));
									}
								}
							}
							break;

						case IM_WByear:
							{
								var stuff = string.Join("╚", Program.appData.yearList);

								bw.Write(IM_WByear);
								bw.Write(stuff);
								bw.Flush();
							}
							break;

						case IM_RequestDatabase:
                            {
                                lock (Program.appData.lockDB)
                                {
                                    var jsonString = JsonSerializer.Serialize(Program.appData.invPostes);
                                    bw.Write(IM_databaseIncoming);
                                    bw.Write(jsonString);
                                    bw.Flush();
                                }
                            }
							break;

						case IM_RequestLogs:
							{
								lock (lockLog)
								{
									if (Program.appData.logsRapport.Count != 0)
									{
										var jsonString = JsonSerializer.Serialize(Program.appData.logsRapport);

										bw.Write(IM_RequestLogs);
										bw.Write(jsonString);
										bw.Flush();
									}
								}
							}
							break;

						case IM_DeleteMain:
							{
								string serial = br.ReadString();

								var result = serial.Split(",");

								lock (Program.appData.lockDB)
								{
									foreach (var delete in result)
									{
										foreach (var item in Program.appData.invPostes.ToArray())
										{
											if (item.serial == delete)
											{
												Program.appData.invPostes.Remove(item);
												break;
											}
										}
									}

									prog.SaveDatabase();

									foreach (var UserKey in prog.users.Keys)
									{
										if (prog.users.TryGetValue(UserKey, out UserInfo user))
										{
											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_DeleteMain);
												user.Connection.bw.Write(serial);
												user.Connection.bw.Flush();
											}
										}
									}

									UpdateLogs(userInfo.UserName + " - Supression de " + result.Count().ToString() + " entrée(s) dans la database.", "Supression database", result.Count().ToString(), userInfo.UserName);
								}
							}
							break;

						case IM_DeleteUser:
							{
								string username = br.ReadString();
								int count = 0;

								try
								{
									while (count != 5)
									{
										foreach (var UserKey in prog.users.Keys.ToArray())
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.UserName == username)
												{
													if (user.LoggedIn) user.Connection.CloseConn();

													prog.users.Remove(UserKey);
													break;
												}
											}
										}

										break;
									}
								}
								catch
								{
									Thread.Sleep(10);
									count++;
								}

								prog.SaveUsers();

								UpdateLogs(userInfo.UserName + " - Supression de l'usager: " + username, "", "", userInfo.UserName);
							}
							break;

						case IM_RequestWaybills:
							{
								string year = br.ReadString();

								lock (Program.appData.lockWB)
								{
									if (year == DateTime.Now.Year.ToString())
									{
										if (Program.appData.waybills.Count != 0)
										{
											var jsonString = JsonSerializer.Serialize(Program.appData.waybills);

											bw.Write(IM_RequestWaybills);
											bw.Write(jsonString);
											bw.Write(year);
											bw.Flush();
										}
										else
										{
											bw.Write(IM_RequestWaybills);
											bw.Write(" ");
											bw.Write(" ");
											bw.Flush();
										}
									}
									else
									{
										List<Waybills> temp = new List<Waybills>();
										string path = @".\waybills\" + year + ".wb";

										try
										{
											XmlSerializer xs = new XmlSerializer(typeof(List<Waybills>));
											using (StreamReader rd = new StreamReader(path))
											{
												temp = xs.Deserialize(rd) as List<Waybills>;
											}

											var jsonString = JsonSerializer.Serialize(temp);

											bw.Write(IM_RequestWaybills);
											bw.Write(jsonString);
											bw.Write(year);
											bw.Flush();
										}
										catch (Exception ex)
										{
											Console.WriteLine(ex.Message);
										}
									}

									prog.msg(userInfo.UserName + " - Resquested Waybill.");
								}
							}
							break;

						case IM_Waybills:
							{
								string action = br.ReadString();
								string waybills = br.ReadString();
								string waybillsRetour = br.ReadString();
								string rf = br.ReadString();

								if (action == "add")
								{
									var result = waybills.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
									var resultretour = waybillsRetour.ToUpper().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

									if (!string.IsNullOrEmpty(waybills)) waybills = waybills.Replace(" ", "");
									if (!string.IsNullOrEmpty(waybillsRetour)) waybillsRetour = waybillsRetour.Replace(" ", "");

									CultureInfo ci = new CultureInfo("fr-FR");
									var month = DateTime.Now.ToString("MMMM", ci);
									var day = DateTime.Now.Day.ToString();

									lock (Program.appData.lockWB)
									{
										Program.appData.waybills.Add(new Waybills { RF = rf, wayb = waybills, wbRetour = waybillsRetour, mois = month, jour = day });

										prog.SaveWB();

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_Waybills);
													user.Connection.bw.Write("add");
													user.Connection.bw.Write(waybills);
													user.Connection.bw.Write(waybillsRetour);
													user.Connection.bw.Write(rf);
													user.Connection.bw.Write(month);
													user.Connection.bw.Write(day);
													user.Connection.bw.Flush();
												}
											}
										}

										bw.Write(IM_AjoutEnd);
										bw.Write(true);
										bw.Flush();
									}

									if (result.Count() != 0) UpdateLogs(userInfo.UserName + " - Création de " + result.Count().ToString() + " Waybill(s) sur le " + rf + ".", "Création waybill", result.Count().ToString(), userInfo.UserName);
									if (resultretour.Count() != 0) UpdateLogs(userInfo.UserName + " - Création de " + resultretour.Count().ToString() + " Waybill(s) de Retour sur le " + rf + ".", "Création waybill", resultretour.Count().ToString(), userInfo.UserName);

								}
								else if (action == "del")
								{
									lock (Program.appData.lockWB)
									{
										foreach (var item in Program.appData.waybills.ToArray())
										{
											if (item.wayb == waybills && item.wbRetour == waybillsRetour && item.RF == rf) Program.appData.waybills.Remove(item);
										}

										prog.SaveWB();

										foreach (var UserKey in prog.users.Keys)
										{
											if (prog.users.TryGetValue(UserKey, out UserInfo user))
											{
												if (user.LoggedIn)
												{
													user.Connection.bw.Write(IM_Waybills);
													user.Connection.bw.Write("del");
													user.Connection.bw.Write(waybills);
													user.Connection.bw.Write(waybillsRetour);
													user.Connection.bw.Write(rf);
													user.Connection.bw.Write(" ");
													user.Connection.bw.Write(" ");
													user.Connection.bw.Flush();
												}
											}
										}

										UpdateLogs(userInfo.UserName + " - Supression d'une entrée de Waybill: " + rf, "", "", userInfo.UserName);
									}
								}

								prog.msg(userInfo.UserName + " - Added new Waybill.");
							}
							break;

						case IM_ChangePrivilege:
							{
								string username = br.ReadString();
								string priv = br.ReadString();

								foreach (var UserKey in prog.users.Keys)
								{
									if (prog.users.TryGetValue(UserKey, out UserInfo user))
									{
										if (user.UserName == username)
										{
											user.Privilege = priv;

											if (user.LoggedIn)
											{
												user.Connection.bw.Write(IM_ChangePrivilege);
												user.Connection.bw.Write(priv);
												bw.Flush();
											}

											break;
										}
									}
								}

								prog.SaveUsers();

								string info = "";

								if (priv == "0") info = "Aucun";
								if (priv == "1") info = "Vue Inventaire";
								if (priv == "2") info = "Entrepot";
								if (priv == "3") info = "Lab";
								if (priv == "4") info = "Entrepot/Lab";
								if (priv == "9") info = "Administrateur";

								UpdateLogs(userInfo.UserName + " - Changement de Privilege pour " + username + ". Nouveau privilège: " + info + ".", "Privilège", info, userInfo.UserName);
							}
							break;

						case IM_BackupList:
							{
								List<string> allFiles = new List<string>();
								string[] temp;

								lock (lockBackup)
								{
									try
									{
										temp = Directory.EnumerateFiles(Program.appData.backupFolder, "*.*", SearchOption.TopDirectoryOnly).ToArray();

										foreach (string item in temp)
										{
											allFiles.Add(Path.GetFileName(item));
										}

										bw.Write(IM_BackupList);
										bw.Write(string.Join("╚", allFiles));
										bw.Flush();
									}
									catch { }
								}
							}
							break;

						case IM_RequestUserList:
							{
								foreach (var UserKey in prog.users.Keys)
								{
									if (prog.users.TryGetValue(UserKey, out UserInfo user))
									{
										bw.Write(IM_UserList);
										bw.Write(user.UserName);
										bw.Write(user.Privilege);
										bw.Flush();
									}
								}
							}
							break;

						case IM_DeleteFiles:
							{
								string files = br.ReadString();
								var fileArray = files.Split("╚");

								lock (lockDelete)
								{
									try
									{
										foreach (string file in fileArray)
										{
											File.Delete(Program.appData.backupFolder + Path.DirectorySeparatorChar + file);
										}
									}
									catch (Exception ex)
									{
										Console.WriteLine(ex.Message);
									}
								}
							}
							break;

						case IM_UpdateModele:
							{
								string model = br.ReadString();
								string task = br.ReadString();

								lock (Program.appData.lockModel)
								{
									if (task == "add")
									{
										if (!Program.appData.modeleMoniteur.Contains(model)) Program.appData.modeleMoniteur.Add(model);
									}

									if (task == "del")
									{
										if (Program.appData.modeleMoniteur.Contains(model)) Program.appData.modeleMoniteur.Remove(model);
									}

									prog.SaveModelMoniteur();
								}

								foreach (var UserKey in prog.users.Keys)
								{
									if (prog.users.TryGetValue(UserKey, out UserInfo user))
									{
										if (user.LoggedIn)
										{
											user.Connection.bw.Write(IM_UpdateModele);
											user.Connection.bw.Write(model);
											user.Connection.bw.Write(task);
											user.Connection.bw.Flush();
										}
									}
								}
							}
							break;

						case IM_Modele:
							{
								updateClientModel();
							}
							break;

						case IM_ChangeUserInfo:
							{
								string oldPass = br.ReadString();
								string newPass = br.ReadString();

								Crypto crypto = new Crypto();

								foreach (string UserKey in prog.users.Keys)
								{
									if (prog.users.TryGetValue(UserKey, out UserInfo user))
									{
										if (user.LoggedIn && user.UserName == userInfo.UserName)
										{
											if (crypto.Encrypt(oldPass) == userInfo.Password)
											{
												user.Password = crypto.Encrypt(newPass);
												prog.msg("User: " + userInfo.UserName + " Successfully changed his password.");
												serverConfirmation("password", userInfo.UserName, "allow");
												prog.SaveUsers();
												break;
											}
											else
											{
												prog.msg("User: " + userInfo.UserName + " Failed to change his password");
												serverConfirmation("password", userInfo.UserName, "deny");
												break;
											}
										}
									}
								}
							}
							break;
					}
				}

				if (userInfo.LoggedIn)
				{
					CloseConn();
				}
			}
			catch (IOException)
			{
				prog.msg("IOException in receiver");
			}
		}

		private void UpdateLogs(string info, string type, string amount, string nom)
		{
			var baseDate = DateTime.Now;
			string data = baseDate.ToString() + " - " + info;

			lock (lockLog)
			{
				Program.appData.logsRapport.Add(new LogsRapport { info = data, type = type, amount = amount, date = baseDate.ToShortDateString(), nom = nom });

				if (Program.appData.logsRapport.Count() >= 100001) Program.appData.logsRapport.RemoveAt(0);
				prog.SaveLog();
			}

			foreach (string UserKey in prog.users.Keys)
			{
				if (prog.users.TryGetValue(UserKey, out UserInfo user))
				{
					if (user.LoggedIn)
					{
						user.Connection.bw.Write(IM_Logs);
						user.Connection.bw.Write(data);
						user.Connection.bw.Write(type);
						user.Connection.bw.Write(amount);
						user.Connection.bw.Write(nom);
						user.Connection.bw.Write(baseDate.ToShortDateString());
						user.Connection.bw.Flush();
					}
				}
			}
		}

		private void serverConfirmation(string action, string userName, string confirm)
		{
			foreach (string UserKey in prog.users.Keys)
			{
				if (prog.users.TryGetValue(UserKey, out UserInfo user))
				{
					if (user.LoggedIn && user.UserName == userName)
					{
						Write(user, IM_ServerNotice, action, confirm);
						break;
					}
				}
			}
		}

		void CloseConn() // Close connection.
		{
			try
			{
				if (userInfo != null)
				{
					prog.userInfo.Usersonline.Remove(userInfo.UserName);
				}

				client.Close();
				br.Close();
				bw.Close();
				ssl.Close();
				netStream.Close();

				if (userInfo != null && userInfo.LoggedIn)
				{
					userInfo.LoggedIn = false;
					prog.msg("User logged out " + userInfo.UserName);
				}

				prog.msg("End of connection!");
				prog.msg("------------------");
			}
			catch
			{
				prog.msg("End of connection! with exceptions");
				prog.msg("----------------------------------");
			}
		}
	}
}
