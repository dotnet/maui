using Android.Content;
using Android.Content.Res;
using Microsoft.Maui.Controls.Platform.Android;
using AAttribute = Android.Resource.Attribute;

namespace Microsoft.Maui.Controls.Compatibility.Material.Android
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