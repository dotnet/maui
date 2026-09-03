namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK
{
	internal class PlatformRenderer : GtkFormsContainer
	{
		public PlatformRenderer(Platform platform)
		{
			Platform = platform;
		}

		public Platform Platform { get; set; }
	}
}
