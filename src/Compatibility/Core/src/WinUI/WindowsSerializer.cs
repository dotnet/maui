using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal sealed class WindowsSerializer : IDeserializer
	{
		const string PropertyStoreFile = "PropertyStore.forms";

		public async Task<IDictionary<string, object>> DeserializePropertiesAsync()
		{
			try
			{
				StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(PropertyStoreFile).DontSync();
				using (Stream stream = (await file.OpenReadAsync().DontSync()).AsStreamForRead())
				{
					if (stream.Length == 0)
						return new Dictionary<string, object>(4);

					try
					{
						var serializer = new DataContractSerializer(typeof(IDictionary<string, object>));
						return (IDictionary<string, object>)serializer.ReadObject(stream);
					}
					catch (Exception e)
					{
						Debug.WriteLine("Could not deserialize properties: " + e.Message);
						Log.Warning("Microsoft.Maui.Controls.Compatibility PropertyStore", $"Exception while reading Application properties: {e}");
					}

					return null;
				}
			}
			catch (FileNotFoundException)
			{
				return new Dictionary<string, object>(4);
			}
		}

		public async Task SerializePropertiesAsync(IDictionary<string, object> properties)
		{
			StorageFile file = await ApplicationData.Current.RoamingFolder.CreateFileAsync(PropertyStoreFile, CreationCollisionOption.ReplaceExisting).DontSync();
			using (StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync().DontSync())
			{
				try
				{
					Stream stream = transaction.Stream.AsStream();
					var serializer = new DataContractSerializer(typeof(IDictionary<string, object>));
					serializer.WriteObject(stream, properties);
					await transaction.CommitAsync().DontSync();
				}
				catch (Exception e)
				{
					Debug.WriteLine("Could not move new serialized property file over old: " + e.Message);
				}
			}
		}
	}
}