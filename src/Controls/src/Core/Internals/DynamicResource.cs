#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Represents a reference to a dynamic resource by key.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DynamicResource
	{
		/// <summary>Gets the resource dictionary key.</summary>
		public string Key { get; private set; }
		/// <summary>Creates a new DynamicResource with the specified key.</summary>
		public DynamicResource(string key) => Key = key;
	}
}