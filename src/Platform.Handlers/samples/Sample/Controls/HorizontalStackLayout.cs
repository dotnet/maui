using Xamarin.Platform.Layouts;

namespace Xamarin.Platform
{
	public class HorizontalStackLayout : StackLayout
	{
		protected override ILayoutManager CreateLayoutManager() => new HorizontalStackLayoutManager(this);
	}
}
