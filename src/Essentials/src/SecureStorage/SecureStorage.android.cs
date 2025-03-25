using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Security.Crypto;
using Java.Security;
using Javax.Crypto;
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

		static void DeleteSharedPreferences()
		{
			// Open an editor to the preferences we can clear, using the alias for storing encrypted values
			var editPreferences = Application.Context.GetSharedPreferences(Alias, FileCreationMode.Private).Edit();
			// Commit is synchronous here so we can be sure it's done before trying to create the encrypted preferences again
			editPreferences?.Clear()?.Commit();
		}

		ISharedPreferences GetEncryptedSharedPreferences()
		{
			try
			{
				return CreateEncryptedSharedPreferences();
			}
			catch (System.Exception ex)
			when (ex is InvalidProtocolBufferException or Android.Security.KeyStoreException or KeyStoreException or BadPaddingException)
			{
				// If we encounter any of these exceptions, it's likely due to a corrupt key or bad migration between devices
				// There isn't much to do at this point except try to delete the shared preferences so we can recreate them
				try
				{
					System.Diagnostics.Debug.WriteLine(
						"Unable get encrypted shared preferences, which is likely due to corrupt encryption key or bad app cache backup/restore. Removing all keys and returning null.");
					System.Diagnostics.Debug.WriteLine(ex);

					// Delete the shared preferences
					DeleteSharedPreferences();

					// Try to return a new instance now that we've deleted the old
					return CreateEncryptedSharedPreferences();
				}
				catch (System.Exception ex2)
				{
					// If we still can't create things, we'll have to give up and return null
					// TODO: Use Logger here?
					System.Diagnostics.Debug.WriteLine("Still unable to create encrypted shared preferences after attempting to deleting them. Returning null.");
					System.Diagnostics.Debug.WriteLine(ex2);
				}
				return null;
			}
		}

		ISharedPreferences CreateEncryptedSharedPreferences()
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
	}
}
