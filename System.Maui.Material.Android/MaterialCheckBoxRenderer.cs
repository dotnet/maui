using System;
using Android.Content;
using Android.Content.Res;
using Xamarin.Forms.Platform.Android;
using AColor = Android.Graphics.Color;
#if __ANDROID_29__
using AColorUtils = AndroidX.Core.Graphics.ColorUtils;
#else
using AColorUtils = Android.Support.V4.Graphics.ColorUtils;
#endif
using AAttribute = Android.Resource.Attribute;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace Xamarin.Forms.Material.Android
{
	public class MaterialCheckBoxRenderer : CheckBoxRendererBase
	{
		static int[][] _checkedStates = new int[][]
					{
						new int[] { AAttribute.StateEnabled, AAttribute.StateChecked },
						new int[] { AAttribute.StateEnabled, -AAttribute.StateChecked },
						new int[] { -AAttribute.StateEnabled, AAttribute.StateChecked },
						new int[] { -AAttribute.StateEnabled, -AAttribute.StatePressed }
					};

		public MaterialCheckBoxRenderer(Context context) : base(MaterialContextThemeWrapper.Create(context), Resource.Attribute.materialCheckBoxStyle)
		{

		}

		protected override ColorStateList GetColorStateList()
		{
			if (Element.Color != Color.Default)
				return base.GetColorStateList();

			int[] checkBoxColorsList = new int[4];
			checkBoxColorsList[0] = MaterialColors.GetCheckBoxColor(true, true);
			checkBoxColorsList[1] = MaterialColors.GetCheckBoxColor(false, true);
			checkBoxColorsList[2] = MaterialColors.GetCheckBoxColor(true, false);
			checkBoxColorsList[3] = MaterialColors.GetCheckBoxColor(false, false);

			return new ColorStateList(_checkedStates, checkBoxColorsList);
		}
	}
}