namespace Microsoft.Maui
{
	public interface ICheck : IView
	{
		bool IsChecked { get; set; }
		Color Color { get; }
	}
}