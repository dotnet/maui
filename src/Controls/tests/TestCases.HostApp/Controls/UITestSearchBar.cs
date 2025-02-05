namespace Maui.Controls.Sample
{
	public interface IUITestSearchBar : ISearchBar
	{
		bool IsCursorVisible { get; }
	}

	public class UITestSearchBar : SearchBar, IUITestSearchBar
	{
		public static readonly BindableProperty IsCursorVisibleProperty =
		BindableProperty.Create(nameof(IsCursorVisible), typeof(bool), typeof(UITestSearchBar), true);

		public bool IsCursorVisible
		{
			get => (bool)GetValue(IsCursorVisibleProperty);
			set => SetValue(IsCursorVisibleProperty, value);
		}
	}
}