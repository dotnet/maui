using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Security.Crypto;
using Java.Security;
using Xamarin.Google.Crypto.Tink.Shaded.Protobuf;

namespace Microsoft.Maui.Storage
{
	partial class SecureStorageImplementation : ISecureStorage
	{
		Task<string> PlatformGetAsync(string key)
		{
			return Task.Run(() =>
			{
				try
				{
					var prefs = GetEncryptedSharedPreferences();
					if (prefs != null)
					{
						return prefs.GetString(key, null);
					}

					// TODO: Use Logger here?
					System.Diagnostics.Debug.WriteLine(
						$"Unable to decrypt key, {key}, which is likely due to key corruption. Removing old key and returning null.");
					PlatformRemove(key);
					return null;
				}
				catch (GeneralSecurityException)
				{
					// TODO: Use Logger here?
					System.Diagnostics.Debug.WriteLine(
						$"Unable to decrypt key, {key}, which is likely due to an app uninstall. Removing old key and returning null.");
					PlatformRemove(key);
					return null;
				}
				catch (Java.Lang.SecurityException)
				{
					// TODO: Use Logger here?
					System.Diagnostics.Debug.WriteLine(
						$"Unable to decrypt key, {key}, which is likely due to an app uninstall. Removing old key and returning null.");
					PlatformRemove(key);
					return null;
				}
			});
		}

		Task PlatformSetAsync(string key, string data)
		{
			return Task.Run(() =>
			{
				using var prefs = GetEncryptedSharedPreferences();
				using var editor = prefs?.Edit();
				if (data is null)
				{
					editor?.Remove(key);
				}
				else
				{
					editor?.PutString(key, data);
				}

				editor?.Apply();
			});
		}

		bool PlatformRemove(string key)
		{
			using var prefs = GetEncryptedSharedPreferences();
			using var editor = prefs?.Edit();
			editor?.Remove(key)?.Apply();
			return true;
		}

		void PlatformRemoveAll()
		{
			using var prefs = GetEncryptedSharedPreferences();
			using var editor = prefs?.Edit();
			editor?.Clear()?.Apply();
		}

		ISharedPreferences GetEncryptedSharedPreferences()
		{
			try
			{
				var context = Application.Context;

				var prefsMainKey = new MasterKey.Builder(context, Alias)
					.SetKeyScheme(MasterKey.KeyScheme.Aes256Gcm)
					.Build();

				return EncryptedSharedPreferences.Create(
					context,
					Alias,
					prefsMainKey,
					EncryptedSharedPreferences.PrefKeyEncryptionScheme.Aes256Siv,
					EncryptedSharedPreferences.PrefValueEncryptionScheme.Aes256Gcm);
			}
			catch (InvalidProtocolBufferException)
			{
				// TODO: Use Logger here?
				System.Diagnostics.Debug.WriteLine(
					"Unable get encrypted shared preferences, which is likely due to an app uninstall. Removing all keys and returning null.");
				PlatformRemoveAll();
				return null;
			}
		}
	}
}
