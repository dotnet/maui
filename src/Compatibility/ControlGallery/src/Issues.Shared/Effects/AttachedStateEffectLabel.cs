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
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Effects
{
	[Preserve(AllMembers = true)]
	public class AttachedStateEffectLabel : Label
	{
		// Android renderers don't detach effects when the renderers get disposed
		// so this is a hack setup to detach those effects when testing if dispose is called from a renderer
		// https://github.com/xamarin/Xamarin.Forms/issues/2520
	}
}
