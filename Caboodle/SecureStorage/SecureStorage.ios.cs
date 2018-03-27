using System;
using System.Threading.Tasks;

using Foundation;
using Security;

namespace Microsoft.Caboodle
{
    public static partial class SecureStorage
    {
        static Task<string> PlatformGetAsync(string key)
        {
            var kc = new KeyChain();

            return Task.FromResult(kc.ValueForKey(key, Alias));
        }

        static Task PlatformSetAsync(string key, string data)
        {
            var kc = new KeyChain();
            kc.SetValueForKey(data, key, Alias);

            return Task.CompletedTask;
        }
    }

    class KeyChain
    {
        static SecRecord ExistingRecordForKey(string key, string service)
        {
            return new SecRecord(SecKind.GenericPassword)
            {
                Account = key,
                Service = service,
                Label = key,
            };
        }

        public string ValueForKey(string key, string service)
        {
            var record = ExistingRecordForKey(key, service);
            SecStatusCode resultCode;
            var match = SecKeyChain.QueryAsRecord(record, out resultCode);

            if (resultCode == SecStatusCode.Success)
                return NSString.FromData(match.ValueData, NSStringEncoding.UTF8);
            else
                return string.Empty;
        }

        public void SetValueForKey(string value, string key, string service)
        {
            var record = ExistingRecordForKey(key, service);
            if (string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(ValueForKey(key, service)))
                    RemoveRecord(record);

                return;
            }

            // if the key already exists, remove it
            if (!string.IsNullOrEmpty(ValueForKey(key, service)))
                RemoveRecord(record);

            var result = SecKeyChain.Add(CreateRecordForNewKeyValue(key, value, service));
            if (result != SecStatusCode.Success)
                throw new Exception($"Error adding record: {result}");
        }

        SecRecord CreateRecordForNewKeyValue(string key, string value, string service)
        {
            return new SecRecord(SecKind.GenericPassword)
            {
                Account = key,
                Service = service,
                Label = key,
                ValueData = NSData.FromString(value, NSStringEncoding.UTF8),
            };
        }

        bool RemoveRecord(SecRecord record)
        {
            var result = SecKeyChain.Remove(record);
            if (result != SecStatusCode.Success)
                throw new Exception(string.Format($"Error removing record: {result}"));

            return true;
        }
    }
}
