using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDynamicResourceHandler
	{
		void SetDynamicResource(BindableProperty property, string key);
	}
}