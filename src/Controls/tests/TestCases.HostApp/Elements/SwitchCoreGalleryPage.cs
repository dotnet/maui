using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class SwitchCoreGalleryPage : CoreGalleryPage<Switch>
{
	protected override bool SupportsFocus => false;

	protected override bool SupportsTapGestureRecognizer => false;

	protected override void InitializeElement(Switch element)
	{
	}

	protected override void Build()
	{
		base.Build();
	}
}
