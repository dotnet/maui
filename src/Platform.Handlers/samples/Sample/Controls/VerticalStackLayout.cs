using Xamarin.Platform.Layouts;

namespace Sample
{
	public class VerticalStackLayout : StackLayout
	{
		protected override ILayoutManager CreateLayoutManager() => new VerticalStackLayoutManager(this);
	}
}
