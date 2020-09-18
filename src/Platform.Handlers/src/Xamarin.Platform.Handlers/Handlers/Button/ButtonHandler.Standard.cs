using System;

namespace Xamarin.Platform.Handlers
{
	public partial class ButtonHandler : AbstractViewHandler<IButton, object>
	{
		public static void MapText(IViewHandler handler, IButton view) { }

		protected override object CreateView() => throw new NotImplementedException();
	}
}