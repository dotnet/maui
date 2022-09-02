using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelTests
	{
		TextBlock GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		// Yes, this looks wrong (because ultimately, it is)
		// We're returning TextTrimming instead of the obviously more correct TextWrapping because
		// LineBreakMode is a fundamentally incorrect conflation of wrapping and trimming. 
		// But for now we have to preserve the old Forms behavior and make the tests pass, so
		// these tests will consider Windows's "LineBreakMode" to be it's text trimming mode
		TextTrimming GetPlatformLineBreakMode(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).TextTrimming;

		int GetPlatformMaxLines(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).MaxLines;
	}
}
