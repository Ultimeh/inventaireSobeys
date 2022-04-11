using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace entrepotServer
{
	public class UserInfo
	{
		[JsonIgnore] volatile public bool LoggedIn;      // Is logged in and connected?
		[JsonIgnore] volatile public bool IsAvail;      // Is logged in and connected?
		[JsonIgnore] volatile public Client Connection;  // Connection info
		[JsonIgnore] public readonly object LOCK = new object();
		private List<string> usersonline = new List<string>();

		//public UserInfo(string user, string pass, Client conn, List<string> ids,string status)
		//{
		//    UserName = user;
		//    Password = pass;
		//    LoggedIn = true;
		//    Connection = conn;
		//    IDs = ids;
		//    Status = status;
		//}
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Privilege { get; set; }
		[JsonIgnore] public List<string> Usersonline { get { return usersonline; } set { usersonline = value; } }
	}
}
