namespace Microsoft.Maui.Controls
{
	static class ToolbarElement
	{
		internal static void SetValue(ref Toolbar? toolbar, Toolbar? value, IElementHandler handler)
		{
			if (toolbar == value)
				return;

			toolbar?.Handler?.DisconnectHandler();
			toolbar = value;
			handler?.UpdateValue(nameof(IToolbarElement.Toolbar));
		}
	}
}
