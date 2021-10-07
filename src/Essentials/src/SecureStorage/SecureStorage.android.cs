using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Security.Crypto;
using Javax.Crypto;

namespace Microsoft.Maui.Essentials
{
	public static partial class SecureStorage
	{

		static readonly object locker = new object();

		static Task<string> PlatformGetAsync(string key)
		{
			string decryptedData = null;

			try
			{
				lock (locker)
				{
					decryptedData = GetEncryptedSharedPreferences().GetString(key, null);
				}
			}
			catch (AEADBadTagException)
			{
				System.Diagnostics.Debug.WriteLine($"Unable to decrypt key, {key}, which is likely due to an app uninstall. Removing old key and returning null.");
				Remove(key);
			}

			return Task.FromResult(decryptedData);
		}

		static Task PlatformSetAsync(string key, string data)
		{
			lock (locker)
			{
				using (var editor = GetEncryptedSharedPreferences().Edit())
				{
					if (data == null)
					{
						editor.Remove(key);
					}
					else
					{
						editor.PutString(key, data);
					}
					editor.Apply();
				}
			}

			return Task.CompletedTask;
		}

		static bool PlatformRemove(string key)
		{
			lock (locker)
			{
				using (var editor = GetEncryptedSharedPreferences().Edit())
				{
					editor.Remove(key).Apply();
				}
			}

			return true;
		}

		static void PlatformRemoveAll()
		{
			lock (locker)
			{
				using (var editor = GetEncryptedSharedPreferences().Edit())
				{
					editor.Clear().Apply();
				}
			}
		}

		static ISharedPreferences GetEncryptedSharedPreferences()
		{
			var context = Application.Context;

			MasterKey prefsMainKey = new MasterKey.Builder(context, Alias)
				.SetKeyScheme(MasterKey.KeyScheme.Aes256Gcm)
				.Build();

			var sharedPreferences = EncryptedSharedPreferences.Create(
				context,
				Alias,
				prefsMainKey,
				EncryptedSharedPreferences.PrefKeyEncryptionScheme.Aes256Siv,
				EncryptedSharedPreferences.PrefValueEncryptionScheme.Aes256Gcm
			);

			return sharedPreferences;
		}
	}
}
