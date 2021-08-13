using Android.Content;
using AndroidX.Core.Widget;
using Google.Android.Material.Button;

namespace Microsoft.Maui.Handlers
{
	public class MauiMaterialButton : MaterialButton
	{
		// Currently Material doesn't have any bottom gravity options
		// so we just move the layout to the bottom using
		// SetCompoundDrawablesRelative during Layout
		internal const int IconGravityBottom = 9999;
		bool bottomGravity = false;
		public MauiMaterialButton(Context context) : base(context)
		{
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (bottomGravity)
			{
				var drawable = TextViewCompat.GetCompoundDrawablesRelative(this)[3];
				drawable?.SetBounds(0, 0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
			}

			base.OnLayout(changed, left, top, right, bottom);
		}

		public override int IconGravity
		{
			get
			{
				return base.IconGravity;
			}

			set
			{
				if (value == IconGravityBottom)
				{
					bottomGravity = true;
					base.IconGravity = MaterialButton.IconGravityTop;
				}
				else
				{
					bottomGravity = false;
					base.IconGravity = value;
				}
			}
		}
	}
}