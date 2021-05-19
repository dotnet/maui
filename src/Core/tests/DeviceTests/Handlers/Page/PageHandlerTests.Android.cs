using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using ATextAlignment = Android.Views.TextAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PageHandlerTests
	{
		public View GetNativePageContent(IPage page)
		{
			int childCount = 0;
			if (page.Handler.NativeView is ViewGroup view)
			{
				childCount = view.ChildCount;
				if (childCount == 1)
					return view.GetChildAt(0);
			}


			Assert.Equal(1, childCount);
			return null;
		}

	}
}