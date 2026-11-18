#nullable enable
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Applies Shell query parameters to the properties declared by <see cref="QueryPropertyAttribute"/> without reflection.
	/// </summary>
	/// <remarks>
	/// The QueryProperty source generator implements this interface explicitly on partial classes that use
	/// <see cref="QueryPropertyAttribute"/>. It isn't intended to be implemented manually.
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IQueryPropertyAttributable
	{
		/// <summary>Applies the current query parameters and resets properties whose parameters were removed.</summary>
		/// <param name="query">The current query parameters.</param>
		/// <param name="previousQuery">The previously applied query parameters.</param>
		void ApplyQueryProperties(IDictionary<string, object> query, IDictionary<string, object> previousQuery);
	}
}
