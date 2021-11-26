using Android.Content;
using AndroidX.Core.Widget;
using Google.Android.Material.Button;

namespace Microsoft.Maui.Platform
{
	public class MauiMaterialButton : MaterialButton
	{
		// Currently Material doesn't have any bottom gravity options
		// so we just move the layout to the bottom using
		// SetCompoundDrawablesRelative during Layout
		internal const int IconGravityBottom = 9999;
		public MauiMaterialButton(Context context) : base(context)
		{
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			// These are hacks that seem to force the button to measure correctly
			// when using top or bottom positioning. 
			if (IconGravity == IconGravityBottom)
			{
				var drawable = TextViewCompat.GetCompoundDrawablesRelative(this)[3];
				drawable?.SetBounds(0, 0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
			}
			else if (IconGravity == MaterialButton.IconGravityTop)
			{
				var drawable = TextViewCompat.GetCompoundDrawablesRelative(this)[1];
				drawable?.SetBounds(0, 0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
			}

			base.OnLayout(changed, left, top, right, bottom);
		}
	}
}