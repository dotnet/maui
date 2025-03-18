namespace Maui.Controls.Sample
{
	public interface IUITestEntry : IEntry
	{
		bool IsCursorVisible { get; }
	}

	public class UITestEntry : Entry, IUITestEntry
	{
		public static readonly BindableProperty IsCursorVisibleProperty =
		BindableProperty.Create(nameof(IsCursorVisible), typeof(bool), typeof(UITestEntry), true);

		public bool IsCursorVisible
		{
			get => (bool)GetValue(IsCursorVisibleProperty);
			set => SetValue(IsCursorVisibleProperty, value);
		}
	}
}