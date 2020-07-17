namespace Xamarin.Forms.Material.iOS
{
	public interface IMaterialEntryRenderer
	{
		Color TextColor { get; }
		Color PlaceholderColor { get; }
		Color BackgroundColor { get; }
		Brush Background { get; }
		string Placeholder { get; }
	}
}