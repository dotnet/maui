using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Security.Crypto;
using Javax.Crypto;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public static partial class SecureStorage
	{

		static readonly object locker = new object();

		static Task<string> PlatformGetAsync(string key)
		{
			return Task.Run(() =>
			{
				try
				{
					lock (locker)
					{
						return GetEncryptedSharedPreferences().GetString(key, null);
					}
				}
				catch (AEADBadTagException)
				{
					System.Diagnostics.Debug.WriteLine($"Unable to decrypt key, {key}, which is likely due to an app uninstall. Removing old key and returning null.");
					Remove(key);

					return null;
				}
				catch (Java.Lang.SecurityException)
				{
					System.Diagnostics.Debug.WriteLine($"Unable to decrypt key, {key}, which is likely due to key corruption. Removing old key and returning null.");
					Remove(key);

					return null;
				}
			});
		}

		static Task PlatformSetAsync(string key, string data)
		{
			return Task.Run(() =>
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
			});
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
				using (var editor = PreferencesImplementation.GetSharedPreferences(Alias).Edit())
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
