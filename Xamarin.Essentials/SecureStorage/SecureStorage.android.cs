using System;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Security;
using Android.Security.Keystore;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;

namespace Xamarin.Essentials
{
    public static partial class SecureStorage
    {
        static Task<string> PlatformGetAsync(string key)
        {
            var context = Platform.AppContext;

            string defaultEncStr = null;
            var encStr = Preferences.Get(Utils.Md5Hash(key), defaultEncStr, Alias);

            string decryptedData = null;
            if (!string.IsNullOrEmpty(encStr))
            {
                var encData = Convert.FromBase64String(encStr);
                var ks = new AndroidKeyStore(context, Alias, AlwaysUseAsymmetricKeyStorage);
                decryptedData = ks.Decrypt(encData);
            }

            return Task.FromResult(decryptedData);
        }

        static Task PlatformSetAsync(string key, string data)
        {
            var context = Platform.AppContext;

            var ks = new AndroidKeyStore(context, Alias, AlwaysUseAsymmetricKeyStorage);
            var encryptedData = ks.Encrypt(data);

            var encStr = Convert.ToBase64String(encryptedData);
            Preferences.Set(Utils.Md5Hash(key), encStr, Alias);

            return Task.CompletedTask;
        }

        static bool PlatformRemove(string key)
        {
            var context = Platform.AppContext;

            key = Utils.Md5Hash(key);
            Preferences.Remove(key, Alias);

            return false;
        }

        static void PlatformRemoveAll() =>
            Preferences.Clear(Alias);

        internal static bool AlwaysUseAsymmetricKeyStorage { get; set; } = false;
    }

    class AndroidKeyStore
    {
        const string androidKeyStore = "AndroidKeyStore"; // this is an Android const value
        const string aesAlgorithm = "AES";
        const string cipherTransformationAsymmetric = "RSA/ECB/PKCS1Padding";
        const string cipherTransformationSymmetric = "AES/GCM/NoPadding";
        const string prefsMasterKey = "SecureStorageKey";
        const int initializationVectorLen = 12; // Android supports an IV of 12 for AES/GCM

        internal AndroidKeyStore(Context context, string keystoreAlias, bool alwaysUseAsymmetricKeyStorage)
        {
            alwaysUseAsymmetricKey = alwaysUseAsymmetricKeyStorage;
            appContext = context;
            alias = keystoreAlias;

            keyStore = KeyStore.GetInstance(androidKeyStore);
            keyStore.Load(null);
        }

        Context appContext;
        string alias;
        KeyStore keyStore;
        bool alwaysUseAsymmetricKey;

        bool useSymmetric = false;
        string useSymmetricPreferenceKey = "essentials_use_symmetric";

        ISecretKey GetKey()
        {
            // check to see if we need to get our key from past-versions or newer versions.
            // we want to use symmetric if we are >= 23 or we didn't set it previously.

            useSymmetric = Preferences.Get(useSymmetricPreferenceKey, Platform.HasApiLevel(BuildVersionCodes.M), SecureStorage.Alias);

            // If >= API 23 we can use the KeyStore's symmetric key
            if (useSymmetric && !alwaysUseAsymmetricKey)
                return GetSymmetricKey();

            // NOTE: KeyStore in < API 23 can only store asymmetric keys
            // specifically, only RSA/ECB/PKCS1Padding
            // So we will wrap our symmetric AES key we just generated
            // with this and save the encrypted/wrapped key out to
            // preferences for future use.
            // ECB should be fine in this case as the AES key should be
            // contained in one block.

            // Get the asymmetric key pair
            var keyPair = GetAsymmetricKeyPair();

            var existingKeyStr = Preferences.Get(prefsMasterKey, null, alias);

            if (!string.IsNullOrEmpty(existingKeyStr))
            {
                var wrappedKey = Convert.FromBase64String(existingKeyStr);

                var unwrappedKey = UnwrapKey(wrappedKey, keyPair.Private);
                var kp = unwrappedKey.JavaCast<ISecretKey>();

                return kp;
            }
            else
            {
                var keyGenerator = KeyGenerator.GetInstance(aesAlgorithm);
                var defSymmetricKey = keyGenerator.GenerateKey();

                var wrappedKey = WrapKey(defSymmetricKey, keyPair.Public);

                Preferences.Set(prefsMasterKey, Convert.ToBase64String(wrappedKey), alias);

                return defSymmetricKey;
            }
        }

        // API 23+ Only
        ISecretKey GetSymmetricKey()
        {
            Preferences.Set(useSymmetricPreferenceKey, true, SecureStorage.Alias);

            var existingKey = keyStore.GetKey(alias, null);

            if (existingKey != null)
            {
                var existingSecretKey = existingKey.JavaCast<ISecretKey>();
                return existingSecretKey;
            }

            var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, androidKeyStore);
            var builder = new KeyGenParameterSpec.Builder(alias, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                .SetBlockModes(KeyProperties.BlockModeGcm)
                .SetEncryptionPaddings(KeyProperties.EncryptionPaddingNone)
                .SetRandomizedEncryptionRequired(false);

            keyGenerator.Init(builder.Build());

            return keyGenerator.GenerateKey();
        }

        KeyPair GetAsymmetricKeyPair()
        {
            // set that we generated keys on pre-m device.
            Preferences.Set(useSymmetricPreferenceKey, false, SecureStorage.Alias);

            var asymmetricAlias = $"{alias}.asymmetric";

            var privateKey = keyStore.GetKey(asymmetricAlias, null)?.JavaCast<IPrivateKey>();
            var publicKey = keyStore.GetCertificate(asymmetricAlias)?.PublicKey;

            // Return the existing key if found
            if (privateKey != null && publicKey != null)
                return new KeyPair(publicKey, privateKey);

            var originalLocale = Platform.GetLocale();
            try
            {
                // Force to english for known bug in date parsing:
                // https://issuetracker.google.com/issues/37095309
                Platform.SetLocale(Java.Util.Locale.English);

                // Otherwise we create a new key
                var generator = KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, androidKeyStore);

                var end = DateTime.UtcNow.AddYears(20);
                var startDate = new Java.Util.Date();
#pragma warning disable CS0618 // Type or member is obsolete
                var endDate = new Java.Util.Date(end.Year, end.Month, end.Day);
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618
                var builder = new KeyPairGeneratorSpec.Builder(Platform.AppContext)
                    .SetAlias(asymmetricAlias)
                    .SetSerialNumber(Java.Math.BigInteger.One)
                    .SetSubject(new Javax.Security.Auth.X500.X500Principal($"CN={asymmetricAlias} CA Certificate"))
                    .SetStartDate(startDate)
                    .SetEndDate(endDate);

                generator.Initialize(builder.Build());
#pragma warning restore CS0618

                return generator.GenerateKeyPair();
            }
            finally
            {
                Platform.SetLocale(originalLocale);
            }
        }

        byte[] WrapKey(IKey keyToWrap, IKey withKey)
        {
            var cipher = Cipher.GetInstance(cipherTransformationAsymmetric);
            cipher.Init(CipherMode.WrapMode, withKey);
            return cipher.Wrap(keyToWrap);
        }

        IKey UnwrapKey(byte[] wrappedData, IKey withKey)
        {
            var cipher = Cipher.GetInstance(cipherTransformationAsymmetric);
            cipher.Init(CipherMode.UnwrapMode, withKey);
            var unwrapped = cipher.Unwrap(wrappedData, KeyProperties.KeyAlgorithmAes, KeyType.SecretKey);
            return unwrapped;
        }

        internal byte[] Encrypt(string data)
        {
            var key = GetKey();

            // Generate initialization vector
            var iv = new byte[initializationVectorLen];
            var sr = new SecureRandom();
            sr.NextBytes(iv);

            Cipher cipher;

            // Attempt to use GCMParameterSpec by default
            try
            {
                cipher = Cipher.GetInstance(cipherTransformationSymmetric);
                cipher.Init(CipherMode.EncryptMode, key, new GCMParameterSpec(128, iv));
            }
            catch (Java.Security.InvalidAlgorithmParameterException)
            {
                // If we encounter this error, it's likely an old bouncycastle provider version
                // is being used which does not recognize GCMParameterSpec, but should work
                // with IvParameterSpec, however we only do this as a last effort since other
                // implementations will error if you use IvParameterSpec when GCMParameterSpec
                // is recognized and expected.
                cipher = Cipher.GetInstance(cipherTransformationSymmetric);
                cipher.Init(CipherMode.EncryptMode, key, new IvParameterSpec(iv));
            }

            var decryptedData = Encoding.UTF8.GetBytes(data);
            var encryptedBytes = cipher.DoFinal(decryptedData);

            // Combine the IV and the encrypted data into one array
            var r = new byte[iv.Length + encryptedBytes.Length];
            Buffer.BlockCopy(iv, 0, r, 0, iv.Length);
            Buffer.BlockCopy(encryptedBytes, 0, r, iv.Length, encryptedBytes.Length);

            return r;
        }

        internal string Decrypt(byte[] data)
        {
            if (data.Length < initializationVectorLen)
                return null;

            var key = GetKey();

            // IV will be the first 16 bytes of the encrypted data
            var iv = new byte[initializationVectorLen];
            Buffer.BlockCopy(data, 0, iv, 0, initializationVectorLen);

            Cipher cipher;

            // Attempt to use GCMParameterSpec by default
            try
            {
                cipher = Cipher.GetInstance(cipherTransformationSymmetric);
                cipher.Init(CipherMode.DecryptMode, key, new GCMParameterSpec(128, iv));
            }
            catch (Java.Security.InvalidAlgorithmParameterException)
            {
                // If we encounter this error, it's likely an old bouncycastle provider version
                // is being used which does not recognize GCMParameterSpec, but should work
                // with IvParameterSpec, however we only do this as a last effort since other
                // implementations will error if you use IvParameterSpec when GCMParameterSpec
                // is recognized and expected.
                cipher = Cipher.GetInstance(cipherTransformationSymmetric);
                cipher.Init(CipherMode.DecryptMode, key, new IvParameterSpec(iv));
            }

            // Decrypt starting after the first 16 bytes from the IV
            var decryptedData = cipher.DoFinal(data, initializationVectorLen, data.Length - initializationVectorLen);

            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}
