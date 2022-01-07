using System;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeHandler : ViewHandler<ISwipeView, UIView>
	{
		protected override UIView CreateNativeView()
		{
			throw new NotImplementedException();
		}
	}
}