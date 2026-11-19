using Android.Content;
using Google.Android.Material.CheckBox;

namespace Microsoft.Maui.Platform;

public class MauiMaterialCheckBox : MaterialCheckBox
{
	public MauiMaterialCheckBox(Context context) : base(context)
	{

	}

	internal bool IsThreeState { get; set; }

	/// <summary>Cycles through checked, indeterminate, and unchecked when three-state support is enabled; otherwise toggles between checked and unchecked.</summary>
	public override void Toggle()
	{
		CheckedState = CheckedState switch
		{
			MaterialCheckBox.StateUnchecked => MaterialCheckBox.StateChecked,
			MaterialCheckBox.StateChecked when IsThreeState => MaterialCheckBox.StateIndeterminate,
			_ => MaterialCheckBox.StateUnchecked
		};
	}
}