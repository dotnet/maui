using System;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorderView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapContent(IBorderHandler handler, IBorderView border)
		{
		}
	}
}
