#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, FrameworkElement>
	{
		protected override FrameworkElement CreateNativeView()
		{
			throw new System.NotImplementedException();
		}
	}
}