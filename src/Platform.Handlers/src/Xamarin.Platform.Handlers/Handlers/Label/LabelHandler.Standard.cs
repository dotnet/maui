using System;

namespace Xamarin.Platform.Handlers
{
	public partial class LabelHandler : AbstractViewHandler<ILabel, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(IViewHandler handler, ILabel label) { }
		public static void MapTextColor(IViewHandler handler, ILabel label) { }
	}
}