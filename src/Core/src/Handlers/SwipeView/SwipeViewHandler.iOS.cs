using System;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, UIView>
	{
		protected override UIView CreateNativeView()
		{
			throw new NotImplementedException();
		}
	}
}