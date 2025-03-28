namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26990, "Accessibility only gets set on WrapperView", PlatformAffected.iOS & PlatformAffected.macOS & PlatformAffected.Android)]
public partial class Issue26990 : ContentPage
{
	public Issue26990()
	{
		InitializeComponent();
		SETGR.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
		METGR.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
		SEBTGR.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
		MEBTGR.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		SPT_label.Text = FindAccessibilityLabel(SPT);
		MPT_label.Text = FindAccessibilityLabel(MPT);
		SST_label.Text = FindAccessibilityLabel(SST);
		MST_label.Text = FindAccessibilityLabel(MST);
		SET_label.Text = FindAccessibilityLabel(SET);
		MET_label.Text = FindAccessibilityLabel(MET);

		SPTB_label.Text = FindAccessibilityLabel(SPTB);
		MPTB_label.Text = FindAccessibilityLabel(MPTB);
		SSTB_label.Text = FindAccessibilityLabel(SSTB);
		MSTB_label.Text = FindAccessibilityLabel(MSTB);
		SETB_label.Text = FindAccessibilityLabel(SETB);
		METB_label.Text = FindAccessibilityLabel(METB);

		BNS_label.Text = FindAccessibilityLabel(BNS);
		BWS_label.Text = FindAccessibilityLabel(BWS);
	}

	string FindAccessibilityLabel(View v)
	{
		var plat = v.Handler?.PlatformView;

#if IOS || MACCATALYST
		if (plat is UIKit.UIView view)
		{
			bool isButton = (view.AccessibilityTraits & UIKit.UIAccessibilityTrait.Button) == UIKit.UIAccessibilityTrait.Button;
			return $"Type: {(isButton ? "Button" : "Not Button")}";
		}
#elif ANDROID
		if (plat is Android.Views.View view && OperatingSystem.IsAndroidVersionAtLeast(29))
		{
			var nodeInfo = AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat.Obtain(view);
			var del = AndroidX.Core.View.ViewCompat.GetAccessibilityDelegate(view);

			// Make sure the delegate has a chance to modify the info
			if (del != null && nodeInfo is not null)
			{
				del.OnInitializeAccessibilityNodeInfo(view, nodeInfo);
			}
			else
			{
				return "Type: Could not get node info";
			}

			string className = nodeInfo?.ClassName?.ToString() ?? "null";
			bool isButton = className == "android.widget.Button";

			return $"Type: {(isButton ? "Button" : "Not Button")}";
		}
#endif
		return "Type:";
	}
}