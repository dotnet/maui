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
		readonly object locker = new();

		Task<string> PlatformGetAsync(string key)
		{
			return Task.Run(() =>
			{
				try
				{
					lock (locker)
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
				lock (locker)
				{
					using ISharedPreferencesEditor editor = GetEncryptedSharedPreferences()?.Edit();
					if (data == null)
						editor?.Remove(key);
					else
						editor?.PutString(key, data);

					editor?.Apply();
				}
			});
		}

		bool PlatformRemove(string key)
		{
			lock (locker)
			{
				using ISharedPreferencesEditor editor = GetEncryptedSharedPreferences()?.Edit();
				editor?.Remove(key)?.Apply();
			}

			return true;
		}

		void PlatformRemoveAll()
		{
			lock (locker)
			{
				using var editor = PreferencesImplementation.GetSharedPreferences(Alias).Edit();
				editor?.Clear()?.Apply();
			}
		}

		ISharedPreferences GetEncryptedSharedPreferences()
		{
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

				return sharedPreferences;
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