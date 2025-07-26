namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3821View : ContentView
{
	public Gh3821View() => InitializeComponent();

	public static readonly BindableProperty TextProperty =
		BindableProperty.Create(nameof(Text), typeof(string), typeof(Gh3821View), default(string));

	public string Text
	{
		get { return (string)GetValue(TextProperty); }
		set { SetValue(TextProperty, value); }
	}
}