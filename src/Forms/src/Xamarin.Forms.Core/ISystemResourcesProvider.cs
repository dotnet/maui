using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ISystemResourcesProvider
	{
		IResourceDictionary GetSystemResources();
	}
}