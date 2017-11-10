using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.GTK
{
    internal sealed class GtkSerializer : IDeserializer
    {
        const string PropertyStoreFile = "PropertyStore.forms";

        public Task<IDictionary<string, object>> DeserializePropertiesAsync()
        {
            try
            {
                var store = IsolatedStorageFile.GetStore(
                    IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly,
                    null, null);

                if (store.FileExists(PropertyStoreFile))
                {
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(PropertyStoreFile, System.IO.FileMode.Open, store))
                    {
                        using (StreamReader reader = new StreamReader(isoStream))
                        {
                            var content = reader.ReadToEnd();

                            if (content.Length == 0)
                                return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>(4));


                            using (Stream stream = new MemoryStream())
                            {
                                byte[] data = System.Text.Encoding.UTF8.GetBytes(content);
                                stream.Write(data, 0, data.Length);
                                stream.Position = 0;
                                DataContractSerializer deserializer = new DataContractSerializer(typeof(IDictionary<string, object>));
                                var result = (IDictionary<string, object>)deserializer.ReadObject(stream);
                                return Task.FromResult(result);
                            }
                        }
                    }
                }

                return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>(4));
            }
            catch (FileNotFoundException)
            {
                return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>(4));
            }
        }

        public Task SerializePropertiesAsync(IDictionary<string, object> properties)
        {
            try
            {
                var store = IsolatedStorageFile.GetStore(
                    IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly,
                    null, null);

                var file = store.CreateFile(PropertyStoreFile);

                using (IsolatedStorageFileStream isoStream = file)
                {
                    var serializer = new DataContractSerializer(typeof(IDictionary<string, object>));
                    serializer.WriteObject(isoStream, properties);
                }

                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not move new serialized property file over old: " + e.Message);

                return Task.FromResult(false);
            }
        }
    }
}