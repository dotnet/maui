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
		async Task<string> PlatformGetAsync(string key)
		{
			return await Task.Run(() =>
			{
				try
				{
					ISharedPreferences sharedPreferences = GetEncryptedSharedPreferences();
					if (sharedPreferences != null)
						return sharedPreferences.GetString(key, null);

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

		async Task PlatformSetAsync(string key, string data)
		{
			await Task.Run(() =>
			{
				using ISharedPreferencesEditor editor = GetEncryptedSharedPreferences()?.Edit();
				if (data is null)
					editor?.Remove(key);
				else
					editor?.PutString(key, data);

				editor?.Apply();
			});
		}

		bool PlatformRemove(string key)
		{
			using ISharedPreferencesEditor editor = GetEncryptedSharedPreferences()?.Edit();
			editor?.Remove(key)?.Apply();
			return true;
		}

		void PlatformRemoveAll()
		{
			using var editor = PreferencesImplementation.GetSharedPreferences(Alias).Edit();
			editor?.Clear()?.Apply();
		}

		ISharedPreferences _prefs;
		ISharedPreferences GetEncryptedSharedPreferences()
		{
			if (_prefs is not null)
			{
				return _prefs;
			}

			try
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
					EncryptedSharedPreferences.PrefValueEncryptionScheme.Aes256Gcm);

				return _prefs = sharedPreferences;
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
