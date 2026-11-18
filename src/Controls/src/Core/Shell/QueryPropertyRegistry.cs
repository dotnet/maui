#nullable enable
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Holds the <see cref="QueryPropertyAttribute"/> setters generated at build time by the QueryProperty source generator.
	/// </summary>
	/// <remarks>
	/// Generated module initializers call <see cref="Register"/> through <c>UnsafeAccessor</c>, so this type is not part of the public API.
	/// When a type is registered, <see cref="ShellContent"/> applies its query properties without reflection.
	/// Types that are not registered keep using the reflection-based implementation.
	/// </remarks>
	static class QueryPropertyRegistry
	{
		static readonly ConcurrentDictionary<Type, QueryProperty[]> s_queryProperties = new();

		/// <summary>
		/// Registers the query properties of <paramref name="type"/>, in the order in which
		/// <c>type.GetCustomAttributes(typeof(QueryPropertyAttribute), true)</c> returns their attributes.
		/// Attributes whose property can't be set through reflection are omitted.
		/// </summary>
		/// <remarks>The signature is a contract with the code emitted by the source generator. Do not change it.</remarks>
		internal static void Register(Type type, string[] queryIds, Type[] propertyTypes, Action<object, object?>[] setters, Action<object>[] resetters)
		{
			var queryProperties = new QueryProperty[queryIds.Length];
			for (var i = 0; i < queryProperties.Length; i++)
				queryProperties[i] = new QueryProperty(queryIds[i], propertyTypes[i], setters[i], resetters[i]);

			s_queryProperties[type] = queryProperties;
		}

		// The generated code calls Register through UnsafeAccessor; make sure the trimmer keeps it.
		[DynamicDependency(nameof(Register), typeof(QueryPropertyRegistry))]
		internal static bool TryGetQueryProperties(Type type, [NotNullWhen(true)] out QueryProperty[]? queryProperties) =>
			s_queryProperties.TryGetValue(type, out queryProperties);

		internal sealed class QueryProperty
		{
			public QueryProperty(string queryId, Type propertyType, Action<object, object?> setValue, Action<object> resetValue)
			{
				QueryId = queryId;
				PropertyType = propertyType;
				SetValue = setValue;
				ResetValue = resetValue;
			}

			public string QueryId { get; }
			public Type PropertyType { get; }
			public Action<object, object?> SetValue { get; }
			public Action<object> ResetValue { get; }
		}
	}
}
