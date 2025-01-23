namespace Maui.Controls.Sample
{
    public partial class IsCursorVisibleBehavior
	{
		public static readonly BindableProperty IsCursorVisibleProperty =
			BindableProperty.Create(nameof(IsCursorVisible), typeof(bool), typeof(IsCursorVisibleBehavior));

		public bool IsCursorVisible
		{
			get => (bool)GetValue(IsCursorVisibleProperty);
			set => SetValue(IsCursorVisibleProperty, value);
		}
	}
}
