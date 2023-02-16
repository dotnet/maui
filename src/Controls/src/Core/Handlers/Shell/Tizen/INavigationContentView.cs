using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public interface INavigationContentView
	{
		NView? TargetView { get; }

		NView? TitleView { get; set; }

		NView? Content { get; set; }
	}
}