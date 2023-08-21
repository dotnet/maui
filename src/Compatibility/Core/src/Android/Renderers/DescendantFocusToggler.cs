//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Android.Views;
using Microsoft.Maui.Controls.Handlers.Compatibility;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal class DescendantFocusToggler : IDescendantFocusToggler
	{
		public bool RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus)
		{
			IViewParent ancestor = control.Parent;
			var previousFocusability = DescendantFocusability.BlockDescendants;
			ConditionalFocusLayout cfl = null;

			// Work our way up through the tree until we find a ConditionalFocusLayout
			while (ancestor is ViewGroup)
			{
				cfl = ancestor as ConditionalFocusLayout;

				if (cfl != null)
				{
					previousFocusability = cfl.DescendantFocusability;
					// Toggle DescendantFocusability to allow this control to get focus
					cfl.DescendantFocusability = DescendantFocusability.AfterDescendants;
					break;
				}

				ancestor = ancestor.Parent;
			}

			// Call the original RequestFocus implementation for the View
			bool result = baseRequestFocus();

			if (cfl != null)
			{
				// Toggle descendantfocusability back to whatever it was
				cfl.DescendantFocusability = previousFocusability;
			}

			return result;
		}
	}
}