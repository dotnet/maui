namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, MauiToolbar>
	{
		protected override MauiToolbar CreatePlatformElement() => new();

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
			arg1.PlatformView.UpdateTitle(arg2);
		}
	}
}
