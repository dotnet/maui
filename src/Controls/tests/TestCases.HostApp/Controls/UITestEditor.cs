namespace Maui.Controls.Sample
{
	public interface IUITestEditor : IEditor
	{
		bool IsCursorVisible { get; }
	}

	public class UITestEditor : Editor, IUITestEditor
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