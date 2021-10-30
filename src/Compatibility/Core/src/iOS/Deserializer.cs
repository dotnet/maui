using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Maui.Controls.Internals;

#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	[Preserve(AllMembers = true)]
	internal class Deserializer : IDeserializer
	{
		const string PropertyStoreFile = "PropertyStore.maui";
		const string PropertyStoreKey = "Properties";

		string LoadSerialized()
		{
			using (var ud = new Foundation.NSUserDefaults(PropertyStoreFile, Foundation.NSUserDefaultsType.SuiteName))
				return ud.StringForKey(PropertyStoreKey);
		}

		void SaveSerialized(string str)
		{
			if (string.IsNullOrEmpty(str))
				return;

			using (var ud = new Foundation.NSUserDefaults(PropertyStoreFile, Foundation.NSUserDefaultsType.SuiteName))
				ud.SetString(str, PropertyStoreKey);
		}

		public Task<IDictionary<string, object>> DeserializePropertiesAsync()
		{
			// Deserialize property dictionary to local storage
			// Make sure to use Internal
			return Task.Run(() =>
			{
				var str = LoadSerialized();

				if (string.IsNullOrEmpty(str))
					return null;

				using var stringReader = new StringReader(str);
				using var reader = XmlReader.Create(stringReader);

				try
				{
					var dcs = new DataContractSerializer(typeof(Dictionary<string, object>));
					return (IDictionary<string, object>)dcs.ReadObject(reader);
				}
				catch (Exception e)
				{
					Debug.WriteLine("Could not deserialize properties: " + e.Message);
					Log.Warning("Microsoft.Maui.Controls.Compatibility PropertyStore", $"Exception while reading Application properties: {e}");
				}

				return null;
			});
		}

		public Task SerializePropertiesAsync(IDictionary<string, object> properties)
		{
			properties = new Dictionary<string, object>(properties);

			// No need to write 0 properties if no file exists
			if (properties.Count <= 0)
				return Task.CompletedTask;

			// Serialize property dictionary to local storage
			// Make sure to use Internal
			return Task.Run(() =>
			{
				using var stringWriter = new StringWriter();
				using var xmlWriter = XmlWriter.Create(stringWriter);

				try
				{
					var dcs = new DataContractSerializer(typeof(Dictionary<string, object>));
					dcs.WriteObject(xmlWriter, properties);
					xmlWriter.Flush();

					var str = stringWriter.ToString();

					SaveSerialized(str);
				}
				catch (Exception e)
				{
					Debug.WriteLine("Could not serialize properties: " + e.Message);
					Log.Warning("Microsoft.Maui.Controls.Compatibility PropertyStore", $"Exception while writing Application properties: {e}");
					return;
				}
			});
		}
	}
}