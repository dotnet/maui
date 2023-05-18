using NColor = Tizen.NUI.Color;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public interface INavigationView
	{
		NView? TargetView { get; }

		NView? Header { get; set; }

		NView? Content { get; set; }

		NView? Footer { get; set; }

		NColor? BackgroundColor { get; set; }
	}
}
