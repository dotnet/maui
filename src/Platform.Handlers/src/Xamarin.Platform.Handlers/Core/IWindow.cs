namespace Xamarin.Platform
{
	public interface IWindow
	{
		public IMauiContext MauiContext { get; set; }
		public IPage Page { get; set; }
	}
}
