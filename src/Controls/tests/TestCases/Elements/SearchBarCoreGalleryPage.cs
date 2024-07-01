using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class SearchBarCoreGalleryPage : CoreGalleryPage<SearchBar>
{
	protected override bool SupportsTapGestureRecognizer => true;

	protected override void InitializeElement(SearchBar element)
	{
	}

	protected override void Build()
	{
		base.Build();
	}
}
