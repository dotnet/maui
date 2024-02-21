using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class EntryCoreGalleryPage : CoreGalleryPage<Entry>
{
	protected override bool SupportsTapGestureRecognizer => false;

	protected override void InitializeElement(Entry element)
	{
	}

	protected override void Build()
	{
		base.Build();
	}
}
