#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides helpers for source-generated <see cref="IQueryAttributable"/> implementations.
	/// </summary>
	/// <remarks>
	/// The conversion depends on where a parameter came from (navigation, <see cref="ShellContent.QueryString"/> or
	/// <see cref="ShellContent.QueryParameters"/>), which is not visible through <see cref="IDictionary{TKey, TValue}"/>.
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class QueryPropertyConversion
	{
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "This reflection fallback preserves QueryProperty behavior for already-compiled IQueryAttributable implementations; source-generated implementations avoid it.")]
		internal static void ApplyLegacyQueryProperties(object target, IDictionary<string, object> query, IDictionary<string, object> previousQuery)
		{
			var type = target.GetType();
			foreach (QueryPropertyAttribute attribute in type.GetCustomAttributes(typeof(QueryPropertyAttribute), inherit: true))
			{
				var property = type.GetRuntimeProperty(attribute.Name);
				if (property is null || !property.CanWrite || property.SetMethod?.IsPublic != true)
					continue;

				if (query.TryGetValue(attribute.QueryId, out var value))
				{
					if (property.PropertyType == typeof(string))
					{
						property.SetValue(target, ConvertToString(query, attribute.QueryId, value));
					}
					else if (value is null)
					{
						property.SetValue(target, null);
					}
					else
					{
						var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
						property.SetValue(target, Convert.ChangeType(value, targetType, GetFormatProvider(query, attribute.QueryId)));
					}
				}
				else if (previousQuery.TryGetValue(attribute.QueryId, out _))
				{
					var defaultValue = property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null
						? Activator.CreateInstance(property.PropertyType)
						: null;
					property.SetValue(target, defaultValue);
				}
			}
		}

		/// <summary>Converts a query parameter value for a <see cref="string"/> property.</summary>
		/// <param name="query">The query passed to the query-property application method.</param>
		/// <param name="queryId">The query parameter name.</param>
		/// <param name="value">The query parameter value.</param>
		/// <returns>The value to assign to the property.</returns>
		public static string? ConvertToString(IDictionary<string, object> query, string queryId, object? value)
		{
			if (value is null)
				return null;

			var parameters = query as ShellRouteParameters;
			if (parameters?.IsShellContentQueryParameter(queryId) == true)
				return Convert.ToString(value, CultureInfo.InvariantCulture);

			if (parameters?.IsShellContentQueryStringParameter(queryId) == true)
				return (string)value;

			return WebUtility.UrlDecode((string)value);
		}

		/// <summary>Gets the culture used to convert a query parameter value for a non-<see cref="string"/> property.</summary>
		/// <param name="query">The query passed to the query-property application method.</param>
		/// <param name="queryId">The query parameter name.</param>
		/// <returns>The format provider to pass to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>.</returns>
		public static IFormatProvider GetFormatProvider(IDictionary<string, object> query, string queryId) =>
			query is ShellRouteParameters parameters && parameters.IsShellContentParameter(queryId)
				? CultureInfo.InvariantCulture
				: CultureInfo.CurrentCulture;

		/// <summary>Validates that the target's runtime type has a generated query-property implementation.</summary>
		/// <param name="target">The instance receiving query properties.</param>
		/// <param name="implementationType">The type declaring the generated implementation.</param>
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "This compatibility guard reflects over derived runtime types to preserve runtime QueryProperty lookup; generated implementations avoid reflection.")]
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Only derived runtime types without their own generated handler reach this compatibility guard.")]
		public static void ValidateGeneratedQueryProperties(object target, Type implementationType)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));
			if (implementationType is null)
				throw new ArgumentNullException(nameof(implementationType));

			var type = target.GetType();
			if (type != implementationType)
			{
				if (type.GetCustomAttributes(typeof(QueryPropertyAttribute), inherit: false).Length > 0)
					ThrowQueryPropertySourceGenerationFailure(type);

				foreach (QueryPropertyAttribute attribute in implementationType.GetCustomAttributes(typeof(QueryPropertyAttribute), inherit: true))
				{
					var generatedProperty = implementationType.GetRuntimeProperty(attribute.Name);
					var runtimeProperty = type.GetRuntimeProperty(attribute.Name);
					if (generatedProperty is null
						|| runtimeProperty is null
						|| generatedProperty.DeclaringType != runtimeProperty.DeclaringType
						|| generatedProperty.PropertyType != runtimeProperty.PropertyType)
						ThrowQueryPropertySourceGenerationFailure(type);
				}
			}
		}

		internal static void ThrowQueryPropertySourceGenerationFailure(Type type)
		{
			var message = $"The type '{type.FullName}' uses [QueryProperty], but the MAUI source generator did not generate an {nameof(IQueryAttributable)} implementation. Make the type and all containing types partial and ensure the MAUI source generator is enabled.";
			MauiLogger<ShellContent>.Log(LogLevel.Error, message);
			throw new InvalidOperationException(message);
		}
	}
}
