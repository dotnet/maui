namespace Xamarin.Forms.Platform.Android
{
	internal interface IPlatformLayout
	{
		void OnLayout(bool changed, int l, int t, int r, int b);
	}
}