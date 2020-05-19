using System.ComponentModel;

namespace System.Maui.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDynamicResourceHandler
	{
		void SetDynamicResource(BindableProperty property, string key);
	}
}