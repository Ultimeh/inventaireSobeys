using System.Threading;
using System.Windows;

namespace Client
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static Mutex _mutex = null;
		
		protected override void OnStartup(StartupEventArgs e)
        {
	     	const string appName = "Inventaire Sobeys";
			bool createdNew;

			_mutex = new Mutex(true, appName, out createdNew);

			if (!createdNew)
			{
				MessageBox.Show(appName + " est déjà ouvert.");
				Application.Current.Shutdown();
			}
			base.OnStartup(e);
		}

		public static AppData appData = new AppData();
	}
}
