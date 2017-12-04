using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;
using System.Diagnostics;
using System.IO;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Tizen
{
	internal class Deserializer : IDeserializer
	{
		const string PropertyStoreFile = "PropertyStore.forms";

		public Task<IDictionary<string, object>> DeserializePropertiesAsync()
		{
			// Deserialize property dictionary to local storage
			// Make sure to use Internal
			return Task.Run(() =>
			{
				var store = new TizenIsolatedStorageFile();
				Stream stream = null;
				try
				{
					stream = store.OpenFile(PropertyStoreFile, System.IO.FileMode.OpenOrCreate);
					if (stream.Length == 0)
					{
						return null;
					}
					using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
					{
						stream = null;
						var dcs = new DataContractSerializer(typeof(Dictionary<string, object>));
						return (IDictionary<string, object>)dcs.ReadObject(reader);
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine("Could not deserialize properties: " + e.Message);
					Internals.Log.Warning("Xamarin.Forms PropertyStore", $"Exception while reading Application properties: {e}");
				}
				finally
				{
					if (stream != null)
					{
						stream.Dispose();
					}
				}

				return null;
			});
		}

		public Task SerializePropertiesAsync(IDictionary<string, object> properties)
		{
			properties = new Dictionary<string, object>(properties);
			// Serialize property dictionary to local storage
			// Make sure to use Internal
			return Task.Run(() =>
			{
				var success = false;
				var store = new TizenIsolatedStorageFile();
				Stream stream = null;
				try
				{
					stream = store.OpenFile(PropertyStoreFile + ".tmp", System.IO.FileMode.OpenOrCreate);
					using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
					{
						stream = null;
						var dcs = new DataContractSerializer(typeof(Dictionary<string, object>));
						dcs.WriteObject(writer, properties);
						writer.Flush();
						success = true;
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine("Could not serialize properties: " + e.Message);
					Internals.Log.Warning("Xamarin.Forms PropertyStore", $"Exception while writing Application properties: {e}");
				}
				finally
				{
					if (stream != null)
					{
						stream.Dispose();
					}
				}

				if (!success)
					return;

				try
				{
					if (store.FileExists(PropertyStoreFile))
						store.DeleteFile(PropertyStoreFile);
					store.MoveFile(PropertyStoreFile + ".tmp", PropertyStoreFile);
				}
				catch (Exception e)
				{
					Debug.WriteLine("Could not move new serialized property file over old: " + e.Message);
					Internals.Log.Warning("Xamarin.Forms PropertyStore", $"Exception while writing Application properties: {e}");
				}
			});
		}
	}
}
