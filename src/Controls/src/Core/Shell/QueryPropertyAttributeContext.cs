#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>Provides query values to source-generated <see cref="QueryPropertyAttribute"/> setters.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class QueryPropertyAttributeContext
	{
		readonly ShellRouteParameters _query;
		readonly ShellRouteParameters _oldQuery;

		internal QueryPropertyAttributeContext(ShellRouteParameters query, ShellRouteParameters oldQuery)
		{
			_query = query;
			_oldQuery = oldQuery;
		}

		/// <summary>Gets and converts the current value for a query parameter.</summary>
		/// <param name="queryId">The query parameter name.</param>
		/// <param name="targetType">The property type, or its underlying type when nullable.</param>
		/// <param name="value">The converted value when the parameter is present.</param>
		/// <returns><see langword="true"/> when the current query contains the parameter.</returns>
		public bool TryGetValue(string queryId, Type targetType, out object value)
		{
			if (!_query.TryGetValue(queryId, out value))
				return false;

			if (targetType == typeof(string))
			{
				if (value != null)
				{
					if (_query.IsShellContentQueryParameter(queryId))
						value = Convert.ToString(value, CultureInfo.InvariantCulture);
					else if (!_query.IsShellContentQueryStringParameter(queryId))
						value = global::System.Net.WebUtility.UrlDecode((string)value);
				}
			}
			else if (value != null)
			{
				var conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;
				var culture = _query.IsShellContentParameter(queryId)
					? CultureInfo.InvariantCulture
					: CultureInfo.CurrentCulture;
				value = Convert.ChangeType(value, conversionType, culture);
			}

			return true;
		}

		/// <summary>Determines whether a parameter was present previously but is absent from the current query.</summary>
		/// <param name="queryId">The query parameter name.</param>
		/// <returns><see langword="true"/> when the property should be reset to its default value.</returns>
		public bool WasRemoved(string queryId) =>
			!_query.ContainsKey(queryId) && _oldQuery.ContainsKey(queryId);
	}
}
