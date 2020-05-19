namespace System.Maui.Platform.GTK
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
