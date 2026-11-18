#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
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
		/// <summary>Converts a query parameter value for a <see cref="string"/> property.</summary>
		/// <param name="query">The query passed to <see cref="IQueryAttributable.ApplyQueryProperties"/>.</param>
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
		/// <param name="query">The query passed to <see cref="IQueryAttributable.ApplyQueryProperties"/>.</param>
		/// <param name="queryId">The query parameter name.</param>
		/// <returns>The format provider to pass to <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>.</returns>
		public static IFormatProvider GetFormatProvider(IDictionary<string, object> query, string queryId) =>
			query is ShellRouteParameters parameters && parameters.IsShellContentParameter(queryId)
				? CultureInfo.InvariantCulture
				: CultureInfo.CurrentCulture;

		/// <summary>Validates that the target's runtime type has a generated query-property implementation.</summary>
		/// <param name="target">The instance receiving query properties.</param>
		/// <param name="implementationType">The type declaring the generated implementation.</param>
		public static void ValidateGeneratedQueryProperties(object target, Type implementationType)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));
			if (implementationType is null)
				throw new ArgumentNullException(nameof(implementationType));

			var type = target.GetType();
			if (type != implementationType)
				ThrowQueryPropertySourceGenerationFailure(type);
		}

		internal static void ThrowQueryPropertySourceGenerationFailure(Type type)
		{
			var message = $"The type '{type.FullName}' uses [QueryProperty], but the MAUI source generator did not generate an {nameof(IQueryAttributable)} implementation. Make the type and all containing types partial and ensure the MAUI source generator is enabled.";
			MauiLogger<ShellContent>.Log(LogLevel.Error, message);
			throw new InvalidOperationException(message);
		}
	}
}
