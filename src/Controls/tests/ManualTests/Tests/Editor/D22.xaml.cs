using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "D22",
	title: "Toggling IsSpellCheckEnabled should refresh red squiggly lines.",
	category: Category.Editor)]
public partial class D22 : ContentPage
{
	public D22()
	{
		InitializeComponent();
	}
}
