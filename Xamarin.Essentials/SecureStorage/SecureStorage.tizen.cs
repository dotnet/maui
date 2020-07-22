using System.Text;
using System.Threading.Tasks;
using Tizen.Security.SecureRepository;

namespace Xamarin.Essentials
{
    public partial class SecureStorage
    {
        static Task<string> PlatformGetAsync(string key)
        {
            try
            {
                // The null parameter here is not the default value as you might expect, but
                // a password
                return Task.FromResult(Encoding.UTF8.GetString(DataManager.Get(key, null)));
            }
            catch (System.InvalidOperationException)
            {
                // The DataManager.Get call throws an exception if key does not exist. Not logging
                // anything since this is an expected and normal situation (if the key did not exist).
                return null;
            }
            catch
            {
                Tizen.Log.Error(Platform.CurrentPackage.Label, "Failed to load data.");
                throw;
            }
        }

        static Task PlatformSetAsync(string key, string data)
        {
            try
            {
                try
                {
                    // Remove the key in case it already exists, otherwise DataManager.Save will throw an exception.
                    // There is no way to check if a key exists without throwing an exception, so there is
                    // no point in checking whether the key exists prior to attempting to remove it.
                    DataManager.RemoveAlias(key);
                }
                catch
                {
                    // Not logging anything since this is an expected and normal situation (if the key did not exist).
                }

                DataManager.Save(key, Encoding.UTF8.GetBytes(data), new Policy());

                return Task.CompletedTask;
            }
            catch
            {
                Tizen.Log.Error(Platform.CurrentPackage.Label, "Failed to save data.");
                throw;
            }
        }

        static void PlatformRemoveAll()
        {
            try
            {
                foreach (var key in DataManager.GetAliases())
                {
                    DataManager.RemoveAlias(key);
                }
            }
            catch
            {
                Tizen.Log.Info(Platform.CurrentPackage.Label, "No save data.");
            }
        }

        static bool PlatformRemove(string key)
        {
            try
            {
                DataManager.RemoveAlias(key);
                return true;
            }
            catch
            {
                Tizen.Log.Info(Platform.CurrentPackage.Label, "Failed to remove data.");
                return false;
            }
        }
    }
}
