using Xamarin.Platform.Layouts;

namespace Sample
{
	public class HorizontalStackLayout : StackLayout
	{
		protected override ILayoutManager CreateLayoutManager() => new HorizontalStackLayoutManager(this);
	}
}
