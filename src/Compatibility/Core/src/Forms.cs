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

#if !(__ANDROID__ || __IOS__ || WINDOWS || TIZEN)
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class Forms
	{
		[Obsolete]
		public static void Init(IActivationState activationState)
		{
			throw new NotImplementedException();
		}

		internal static IMauiContext MauiContext => throw new NotImplementedException();
	}
}
#endif
