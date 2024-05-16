using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class ProgressBarCoreGalleryPage : CoreGalleryPage<ProgressBar>
{
	protected override bool SupportsFocus => false;

	protected override void InitializeElement(ProgressBar element)
	{
		element.Progress = 1;
	}

	protected override void Build()
	{
		base.Build();
	}
}
