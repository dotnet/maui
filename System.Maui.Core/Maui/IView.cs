namespace System.Maui
{
	public interface IView : IFrameworkElement
	{

		//Alignment GetVerticalAlignment(ILayout layout) => Alignment.Fill;
		//Alignment GetHorizontalAlignment(ILayout layout) => Alignment.Fill;
		bool IsEnabled { get; }
		Color BackgroundColor { get; }
	}
}
