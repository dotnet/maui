using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	internal class Deserializer : Internals.IDeserializer
	{
		const string PropertyStoreFile = "PropertyStore.forms";

		public Task<IDictionary<string, object>> DeserializePropertiesAsync() => Task.Factory.StartNew(DeserializeProperties);

		[RequiresUnreferencedCode(TrimmerConstants.SerializerTrimmerWarning)]
		IDictionary<string, object> DeserializeProperties()
		{
			// Deserialize property dictionary to local storage
			// Make sure to use Internal
			using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
			{
				if (!store.FileExists(PropertyStoreFile))
					return null;

				using (IsolatedStorageFileStream stream = store.OpenFile(PropertyStoreFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
				using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
				{
					if (stream.Length == 0)
						return null;

					try
					{
						var dcs = new DataContractSerializer(typeof(Dictionary<string, object>));
						return (IDictionary<string, object>)dcs.ReadObject(reader);
					}
					catch (Exception e)
					{
						Debug.WriteLine("Could not deserialize properties: " + e.Message);
						Internals.Log.Warning("Microsoft.Maui.Controls.Compatibility PropertyStore", $"Exception while reading Application properties: {e}");
					}
				}
			}

			return null;
		}

		[RequiresUnreferencedCode(TrimmerConstants.SerializerTrimmerWarning)]
		public Task SerializePropertiesAsync(IDictionary<string, object> properties)
		{
			properties = new Dictionary<string, object>(properties);

			// No need to write 0 properties if no file exists
			if (properties.Count <= 0)
				return Task.CompletedTask;

			return Task.Factory.StartNew(SerializeProperties, properties);
		}

		[RequiresUnreferencedCode(TrimmerConstants.SerializerTrimmerWarning)]
		void SerializeProperties(object properties)
		{
			using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
			{
				// No need to write 0 properties if no file exists
				if (properties.Count == 0 && !store.FileExists(PropertyStoreFile))
				{
					return;
				}
				using (IsolatedStorageFileStream stream = store.OpenFile(PropertyStoreFile + ".tmp", System.IO.FileMode.OpenOrCreate))
				using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
				{
					try
					{
						var dcs = new DataContractSerializer(typeof(Dictionary<string, object>));
						dcs.WriteObject(writer, properties);
						writer.Flush();
					}
					catch (Exception e)
					{
						Debug.WriteLine("Could not serialize properties: " + e.Message);
						Internals.Log.Warning("Microsoft.Maui.Controls.Compatibility PropertyStore", $"Exception while writing Application properties: {e}");
						return;
					}
				}

				try
				{
					if (store.FileExists(PropertyStoreFile))
						store.DeleteFile(PropertyStoreFile);
					store.MoveFile(PropertyStoreFile + ".tmp", PropertyStoreFile);
				}
				catch (Exception e)
				{
					Debug.WriteLine("Could not move new serialized property file over old: " + e.Message);
					Internals.Log.Warning("Microsoft.Maui.Controls.Compatibility PropertyStore", $"Exception while writing Application properties: {e}");
				}
			}
		}
	}
}