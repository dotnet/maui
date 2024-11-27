using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Foundation;
using Security;

namespace Microsoft.Maui.Storage
{
	partial class SecureStorageImplementation : ISecureStorage, IPlatformSecureStorage
	{
		public SecAccessible DefaultAccessible { get; set; } =
			SecAccessible.AfterFirstUnlock;

		public Task SetAsync(string key, string value, SecAccessible accessible)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			var kc = new KeyChain(accessible);
			kc.SetValueForKey(value, key, Alias);

			return Task.CompletedTask;
		}

		Task<string> PlatformGetAsync(string key)
		{
			var kc = new KeyChain(DefaultAccessible);
			var value = kc.ValueForKey(key, Alias);

			return Task.FromResult(value);
		}

		Task PlatformSetAsync(string key, string data) =>
			SetAsync(key, data, DefaultAccessible);

		bool PlatformRemove(string key)
		{
			var kc = new KeyChain(DefaultAccessible);
			return kc.Remove(key, Alias);
		}

		void PlatformRemoveAll()
		{
			var kc = new KeyChain(DefaultAccessible);
			kc.RemoveAll(Alias);
		}
	}

	class KeyChain
	{
		SecAccessible accessible;

		internal KeyChain(SecAccessible accessible) =>
			this.accessible = accessible;

		SecRecord ExistingRecordForKey(string key, string service)
		{
			return new SecRecord(SecKind.GenericPassword)
			{
				Account = key,
				Service = service
			};
		}

		internal string ValueForKey(string key, string service)
		{
			using (var record = ExistingRecordForKey(key, service))
			using (var match = SecKeyChain.QueryAsRecord(record, out var resultCode))
			{
				if (resultCode == SecStatusCode.Success)
					return NSString.FromData(match.ValueData, NSStringEncoding.UTF8);
				else
					return null;
			}
		}

		internal void SetValueForKey(string value, string key, string service)
		{
			using (var record = ExistingRecordForKey(key, service))
			{
				if (string.IsNullOrEmpty(value))
				{
					if (!string.IsNullOrEmpty(ValueForKey(key, service)))
						RemoveRecord(record);

					return;
				}

				// if the key already exists, remove it
				if (!string.IsNullOrEmpty(ValueForKey(key, service)))
					RemoveRecord(record);
			}

			using (var newRecord = CreateRecordForNewKeyValue(key, value, service))
			{
				var result = SecKeyChain.Add(newRecord);

				switch (result)
				{
					case SecStatusCode.DuplicateItem:
						{
							// TODO: Use Logger here?
							Debug.WriteLine("Duplicate item found. Attempting to remove and add again.");

							// try to remove and add again
							if (Remove(key, service))
							{
								result = SecKeyChain.Add(newRecord);
								if (result != SecStatusCode.Success)
									throw new Exception($"Error adding record: {result}");
							}
							else
							{
								// TODO: Use Logger here?
								Debug.WriteLine("Unable to remove key.");
							}
						}
						break;
					case SecStatusCode.Success:
						return;
					default:
						throw new Exception($"Error adding record: {result}");
				}
			}
		}

		internal bool Remove(string key, string service)
		{
			using (var record = ExistingRecordForKey(key, service))
			using (var match = SecKeyChain.QueryAsRecord(record, out var resultCode))
			{
				if (resultCode == SecStatusCode.Success)
				{
					RemoveRecord(record);
					return true;
				}
			}
			return false;
		}

		internal void RemoveAll(string service)
		{
			using (var query = new SecRecord(SecKind.GenericPassword) { Service = service })
			{
				SecKeyChain.Remove(query);
			}
		}

		SecRecord CreateRecordForNewKeyValue(string key, string value, string service)
		{
			return new SecRecord(SecKind.GenericPassword)
			{
				Account = key,
				Service = service,
				Label = key,
				Accessible = accessible,
				ValueData = NSData.FromString(value, NSStringEncoding.UTF8),
			};
		}

		bool RemoveRecord(SecRecord record)
		{
			var result = SecKeyChain.Remove(record);
			if (result != SecStatusCode.Success && result != SecStatusCode.ItemNotFound)
				throw new Exception($"Error removing record: {result}");

			return true;
		}
	}
}
