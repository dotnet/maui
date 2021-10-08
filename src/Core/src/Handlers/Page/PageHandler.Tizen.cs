using Tizen.UIExtensions.Common;
using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler
	{
		public static void MapTitle(PageHandler handler, IContentView page)
		{
		}

		protected override ContentCanvas CreateNativeView()
		{
			var view = base.CreateNativeView();
			view.BackgroundColor = (DeviceInfo.GetDeviceType() == DeviceType.TV) ? EColor.Transparent : EColor.White;

			return view;
		}
	}
}