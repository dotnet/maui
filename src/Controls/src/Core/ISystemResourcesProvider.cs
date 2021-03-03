using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ISystemResourcesProvider
	{
		IResourceDictionary GetSystemResources();
	}
}