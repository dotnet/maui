using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, NView>
	{
		protected override NView CreatePlatformElement()
		{
			throw new System.NotImplementedException();
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
		}
	}
}
