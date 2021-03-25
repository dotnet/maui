namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public abstract class WindowsPage : WindowsBasePage
	{
		protected override Platform CreatePlatform()
		{
			return new WindowsPlatform(this);
		}
	}
}