using System;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorderView, NotImplementedView>
	{
		[MissingMapper]
		protected override NotImplementedView CreatePlatformView() => new();

		[MissingMapper]
		public static void MapContent(IBorderHandler handler, IBorderView border)
		{
		}
	}
}
