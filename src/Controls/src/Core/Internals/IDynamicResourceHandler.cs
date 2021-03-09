using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDynamicResourceHandler
	{
		void SetDynamicResource(BindableProperty property, string key);
	}
}