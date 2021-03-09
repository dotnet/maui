using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DynamicResource
	{
		public string Key { get; private set; }
		public DynamicResource(string key) => Key = key;
	}
}