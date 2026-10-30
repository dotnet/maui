using System;
using System.Runtime.InteropServices;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>A password store keyed by (service, key). Implemented over libsecret; abstracted so it can be faked.</summary>
interface ISecretBackend
{
	/// <summary>Returns the stored secret, or null when there is none.</summary>
	string? Lookup(string service, string key);

	void Store(string service, string key, string label, string secret);

	/// <summary>Removes the secret for <paramref name="key"/>; returns whether something was removed.</summary>
	bool Clear(string service, string key);

	/// <summary>Removes every secret of the service.</summary>
	void ClearAll(string service);
}

/// <summary>
/// libsecret (<c>libsecret-1.so.0</c>) bound at run time by symbol lookup. Only the non-variadic <c>*v_sync</c> entry
/// points are used (attributes travel in a <c>GHashTable</c>), so no variadic P/Invoke is needed and a missing library
/// only fails when secure storage is used.
/// </summary>
/// <remarks>
/// The <c>SecretSchema</c> is built by hand in unmanaged memory: <c>{ char* name; int flags; { char* name; int type; } attributes[32]; ... }</c>
/// with the reserved tail zeroed. Both attributes (<c>service</c>, <c>key</c>) are strings.
/// </remarks>
sealed unsafe class LibSecret : ISecretBackend
{
	static readonly string[] LibSecretNames = { "libsecret-1.so.0", "libsecret-1.so" };
	static readonly string[] GLibNames = { "libglib-2.0.so.0", "libglib-2.0.so" };

	const int SchemaAttributeCount = 32;
	const int SchemaTailBytes = 8 * 8 + 4 * 8;
	const string Collection = "default";

	readonly nint secretLib;
	readonly nint schema;
	readonly nint nameService;
	readonly nint nameKey;

	readonly delegate* unmanaged[Cdecl]<nint, nint, nint, nint, nint, nint, nint*, int> storev;
	readonly delegate* unmanaged[Cdecl]<nint, nint, nint, nint*, nint> lookupv;
	readonly delegate* unmanaged[Cdecl]<nint, nint, nint, nint*, int> clearv;
	readonly delegate* unmanaged[Cdecl]<nint, void> passwordFree;

	readonly delegate* unmanaged[Cdecl]<nint, nint, nint, nint, nint> hashTableNewFull;
	readonly delegate* unmanaged[Cdecl]<nint, nint, nint, int> hashTableInsert;
	readonly delegate* unmanaged[Cdecl]<nint, void> hashTableUnref;
	readonly delegate* unmanaged[Cdecl]<nint, nint> strHash;
	readonly delegate* unmanaged[Cdecl]<nint, nint, int> strEqual;
	readonly delegate* unmanaged[Cdecl]<nint, void> free;
	readonly delegate* unmanaged[Cdecl]<nint, void> errorFree;

	LibSecret(string schemaName)
	{
		secretLib = Open(LibSecretNames);
		var glib = Open(GLibNames);

		storev = (delegate* unmanaged[Cdecl]<nint, nint, nint, nint, nint, nint, nint*, int>)Symbol(secretLib, "secret_password_storev_sync");
		lookupv = (delegate* unmanaged[Cdecl]<nint, nint, nint, nint*, nint>)Symbol(secretLib, "secret_password_lookupv_sync");
		clearv = (delegate* unmanaged[Cdecl]<nint, nint, nint, nint*, int>)Symbol(secretLib, "secret_password_clearv_sync");
		passwordFree = (delegate* unmanaged[Cdecl]<nint, void>)Symbol(secretLib, "secret_password_free");

		hashTableNewFull = (delegate* unmanaged[Cdecl]<nint, nint, nint, nint, nint>)Symbol(glib, "g_hash_table_new_full");
		hashTableInsert = (delegate* unmanaged[Cdecl]<nint, nint, nint, int>)Symbol(glib, "g_hash_table_insert");
		hashTableUnref = (delegate* unmanaged[Cdecl]<nint, void>)Symbol(glib, "g_hash_table_unref");
		strHash = (delegate* unmanaged[Cdecl]<nint, nint>)Symbol(glib, "g_str_hash");
		strEqual = (delegate* unmanaged[Cdecl]<nint, nint, int>)Symbol(glib, "g_str_equal");
		free = (delegate* unmanaged[Cdecl]<nint, void>)Symbol(glib, "g_free");
		errorFree = (delegate* unmanaged[Cdecl]<nint, void>)Symbol(glib, "g_error_free");

		nameService = Marshal.StringToCoTaskMemUTF8("service");
		nameKey = Marshal.StringToCoTaskMemUTF8("key");

		// struct SecretSchema { gchar *name; SecretSchemaFlags flags; SecretSchemaAttribute attributes[32]; reserved... }
		var ptr = sizeof(nint);
		var attributeOffset = (ptr + sizeof(int) + ptr - 1) / ptr * ptr; // flags padded to pointer alignment
		var attributeSize = ptr + ptr;                                  // { char* name; int type; } padded
		var total = attributeOffset + SchemaAttributeCount * attributeSize + SchemaTailBytes;

		schema = Marshal.AllocHGlobal(total);
		new Span<byte>((void*)schema, total).Clear();
		Marshal.WriteIntPtr(schema, 0, Marshal.StringToCoTaskMemUTF8(schemaName));
		Marshal.WriteInt32(schema, ptr, 0); // SECRET_SCHEMA_NONE
		Marshal.WriteIntPtr(schema, attributeOffset, nameService);
		Marshal.WriteInt32(schema, attributeOffset + ptr, 0); // SECRET_SCHEMA_ATTRIBUTE_STRING
		Marshal.WriteIntPtr(schema, attributeOffset + attributeSize, nameKey);
		Marshal.WriteInt32(schema, attributeOffset + attributeSize + ptr, 0);
	}

	/// <summary>Binds libsecret, or returns null when it (or GLib) is not installed.</summary>
	internal static LibSecret? TryCreate(string schemaName)
	{
		if (!OperatingSystem.IsLinux())
			return null;

		try
		{
			return new LibSecret(schemaName);
		}
		catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
		{
			return null;
		}
	}

	public string? Lookup(string service, string key)
	{
		var attributes = NewAttributes(service, key);
		nint error = 0;
		try
		{
			var secret = lookupv(schema, attributes, 0, &error);
			ThrowIfError(error, "read");
			if (secret == 0)
				return null;

			try
			{
				return Marshal.PtrToStringUTF8(secret);
			}
			finally
			{
				passwordFree(secret);
			}
		}
		finally
		{
			hashTableUnref(attributes);
		}
	}

	public void Store(string service, string key, string label, string secret)
	{
		var attributes = NewAttributes(service, key);
		var collection = Marshal.StringToCoTaskMemUTF8(Collection);
		var labelPtr = Marshal.StringToCoTaskMemUTF8(label);
		var secretPtr = Marshal.StringToCoTaskMemUTF8(secret);
		nint error = 0;
		try
		{
			storev(schema, attributes, collection, labelPtr, secretPtr, 0, &error);
			ThrowIfError(error, "write");
		}
		finally
		{
			hashTableUnref(attributes);
			Marshal.FreeCoTaskMem(collection);
			Marshal.FreeCoTaskMem(labelPtr);
			Marshal.FreeCoTaskMem(secretPtr);
		}
	}

	public bool Clear(string service, string key)
	{
		var attributes = NewAttributes(service, key);
		nint error = 0;
		try
		{
			var removed = clearv(schema, attributes, 0, &error);
			ThrowIfError(error, "remove");
			return removed != 0;
		}
		finally
		{
			hashTableUnref(attributes);
		}
	}

	public void ClearAll(string service)
	{
		var attributes = NewAttributes(service, null);
		nint error = 0;
		try
		{
			clearv(schema, attributes, 0, &error);
			ThrowIfError(error, "remove");
		}
		finally
		{
			hashTableUnref(attributes);
		}
	}

	/// <summary>A <c>GHashTable&lt;char*, char*&gt;</c> owning (<c>g_free</c>) its keys and values.</summary>
	nint NewAttributes(string service, string? key)
	{
		var table = hashTableNewFull((nint)strHash, (nint)strEqual, (nint)free, (nint)free);
		Insert(table, "service", service);
		if (key is not null)
			Insert(table, "key", key);
		return table;
	}

	void Insert(nint table, string name, string value)
	{
		// g_free-able copies: allocated with the C allocator through GLib's own g_strdup is avoided (non-variadic, but
		// would need another symbol); Marshal.StringToCoTaskMemUTF8 uses malloc on Linux, which g_free releases.
		hashTableInsert(table, Marshal.StringToCoTaskMemUTF8(name), Marshal.StringToCoTaskMemUTF8(value));
	}

	void ThrowIfError(nint error, string action)
	{
		if (error == 0)
			return;

		// struct GError { GQuark domain (uint); gint code; gchar *message; }
		var message = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(error, 8));
		errorFree(error);
		throw new InvalidOperationException($"libsecret failed to {action} the secret: {message}");
	}

	static nint Open(string[] names)
	{
		foreach (var name in names)
		{
			if (NativeLibrary.TryLoad(name, out var handle))
				return handle;
		}

		throw new DllNotFoundException(names[0]);
	}

	static nint Symbol(nint library, string name) =>
		NativeLibrary.GetExport(library, name);
}
