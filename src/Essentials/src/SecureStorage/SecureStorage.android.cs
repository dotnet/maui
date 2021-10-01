using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Security;
using Android.Security.Keystore;
using AndroidX.Security.Crypto;
using Java.Security;
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
			var runtimeApiLevel = SdkInt;
			
			string fileName;
			string prefsMainKey;
			var context = Application.Context;


			if (runtimeApiLevel >= 23)
			{
				prefsMainKey = GetOrCreateMasterKeyApi23OrNewer();
				fileName = "MAUI";
			}
			else
			{
				prefsMainKey = GetOrCreateMasterKeyApi22OrOlder(context);
				fileName = "MAUI_Legacy";
			}

			var sharedPreferences = EncryptedSharedPreferences.Create(
				fileName,
				prefsMainKey,
				context,
				EncryptedSharedPreferences.PrefKeyEncryptionScheme.Aes256Siv,
				EncryptedSharedPreferences.PrefValueEncryptionScheme.Aes256Gcm
			);

			return sharedPreferences;
		}

		static string GetOrCreateMasterKeyApi23OrNewer()
			=> MasterKeys.GetOrCreate(MasterKeys.Aes256GcmSpec);

		static string GetOrCreateMasterKeyApi22OrOlder(Context context)
		{
			// older versions don't support symmetric keys. but we still want to always use the KeyStore

			var asymmetricAlias = $"dotnet.MAUI.asymmetric";

			// Force to english for known bug in date parsing:
			// https://issuetracker.google.com/issues/37095309
			SetLocale(context, Java.Util.Locale.English);

			// Otherwise we create a new key
			var generator = KeyPairGenerator.GetInstance(
			  KeyProperties.KeyAlgorithmRsa,
			  "AndroidKeyStore"); // Const value for secure keystore

			// certificates have issue date and expiration date. related to rsa
			var end = DateTime.UtcNow.AddYears(30);
			var startDate = new Java.Util.Date();

#pragma warning disable CS0618 // Type or member is obsolete
			var endDate = new Java.Util.Date(end.Year, end.Month, end.Day);

			// generates the key pair stuff
			var builder = new KeyPairGeneratorSpec.Builder(context)
			  .SetAlias(asymmetricAlias)
			  .SetSerialNumber(Java.Math.BigInteger.ValueOf(Math.Abs(asymmetricAlias.GetHashCode())))
			  .SetSubject(new Javax.Security.Auth.X500.X500Principal($"CN={asymmetricAlias}")) // CN is common name
			  .SetStartDate(startDate)
			  .SetEndDate(endDate);

			generator.Initialize(builder.Build());
#pragma warning restore CS0618

			var keyPair = generator.GenerateKeyPair();
			return keyPair.Public.ToString();
		}

		static int SdkInt
		  => (int)Android.OS.Build.VERSION.SdkInt;

		static void SetLocale(Context context, Java.Util.Locale locale)
		{
			Java.Util.Locale.Default = locale;
			var resources = context.Resources;
			var config = resources.Configuration;

#pragma warning disable CS0618 // Type or member is obsolete
#if __ANDROID_24__
			if (SdkInt >= 24)
				config.SetLocale(locale);
			else
#endif
				config.Locale = locale;
			resources.UpdateConfiguration(config, resources.DisplayMetrics);
#pragma warning restore CS0618 // Type or member is obsolete
		}

	}
}
