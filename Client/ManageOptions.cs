using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Client
{
    class ManageOptions
    {
        string path = @"c:\Inventaire Sobeys Settings\Options.cfg";

		public void loadOptions()
		{
			if (!File.Exists(path)) return;

			try
			{
				XmlSerializer xs = new XmlSerializer(typeof(Options));
				using (StreamReader rd = new StreamReader(path))
				{
					App.appData.settings = xs.Deserialize(rd) as Options;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Problem detected with 'Options.cfg' in the 'settings' folder." + Environment.NewLine + "The file was not saved properly or you may have edited the file incorrectly.", "Loading Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		public void saveOption()
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Options));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xs.Serialize(wr, App.appData.settings);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Problem detected when trying to save the options.");
            }
        }
    }
}
