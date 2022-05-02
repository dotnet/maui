using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Views;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase
	{
		protected bool GetIsAccessibilityElement(IViewHandler viewHandler) =>
			GetSemanticPlatformElement(viewHandler).ImportantForAccessibility == ImportantForAccessibility.Yes;


		protected bool GetExcludedWithChildren(IViewHandler viewHandler) =>
			GetSemanticPlatformElement(viewHandler).ImportantForAccessibility == ImportantForAccessibility.NoHideDescendants;

		public View GetSemanticPlatformElement(IViewHandler viewHandler)
		{
			if (viewHandler.PlatformView is AndroidX.AppCompat.Widget.SearchView sv)
				return sv.FindViewById(Resource.Id.search_button)!;

			return (View)viewHandler.PlatformView;
		}
	}
}
