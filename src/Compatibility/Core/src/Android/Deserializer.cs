using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal class Deserializer : IDeserializer
	{
		const string PropertyStoreFile = "PropertyStore.maui";

		static string GetFilePath()
			=> Path.Combine(Essentials.FileSystem.CacheDirectory, PropertyStoreFile);

		public Task<IDictionary<string, object>> DeserializePropertiesAsync()
		{
			// Deserialize property dictionary to local storage
			// Make sure to use Internal
			return Task.Run(() =>
			{
				var path = GetFilePath();

				if (!File.Exists(path))
					return null;

				using var stream = File.OpenRead(path);
				using var xmlReader = XmlReader.Create(stream);
				using var reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);

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
				var path = GetFilePath();

				using var stream = File.Create(path);
				using var xmlWriter = XmlWriter.Create(stream);
				using var writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);

				try
				{
					var dcs = new DataContractSerializer(typeof(Dictionary<string, object>));
					dcs.WriteObject(writer, properties);
					writer.Flush();
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