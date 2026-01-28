#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <summary>Maps a query parameter from a URI to a property on the target page or view model during Shell navigation.</summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	[RequiresUnreferencedCode(TrimmerConstants.QueryPropertyAttributeWarning, Url = TrimmerConstants.QueryPropertyDocsUrl)]
	public class QueryPropertyAttribute : Attribute
	{
		/// <summary>Gets the name of the property that will receive the query parameter value.</summary>
		public string Name { get; }
		/// <summary>Gets the query parameter name from the URI.</summary>
		public string QueryId { get; }

		/// <summary>Creates a new <see cref="QueryPropertyAttribute"/> that maps a query parameter to a property.</summary>
		/// <param name="name">The name of the property to set.</param>
		/// <param name="queryId">The query parameter name in the URI.</param>
		public QueryPropertyAttribute(string name, string queryId)
		{
			Name = name;
			QueryId = queryId;
		}
	}
}
