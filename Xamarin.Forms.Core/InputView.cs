namespace Xamarin.Forms
{
	public class InputView : View
	{
		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create("Keyboard", typeof(Keyboard), typeof(InputView), Keyboard.Default,
			coerceValue: (o, v) => (Keyboard)v ?? Keyboard.Default);

		internal InputView()
		{
		}

		public Keyboard Keyboard
		{
			get { return (Keyboard)GetValue(KeyboardProperty); }
			set { SetValue(KeyboardProperty, value); }
		}
	}
}