using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

internal class ActivityIndicatorCoreGalleryPage : CoreGalleryPage<ActivityIndicator>
{
	protected override bool SupportsFocus => false;

	protected override void InitializeElement(ActivityIndicator element)
	{
		element.IsRunning = true;
	}

	protected override void Build()
	{
		base.Build();
	}
}
