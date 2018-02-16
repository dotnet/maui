namespace Xamarin.Forms
{
	public class InputView : View
	{
		public static readonly BindableProperty KeyboardProperty = BindableProperty.Create("Keyboard", typeof(Keyboard), typeof(InputView), Keyboard.Default,
			coerceValue: (o, v) => (Keyboard)v ?? Keyboard.Default);
		public static readonly BindableProperty IsSpellCheckEnabledProperty = BindableProperty.Create("IsSpellCheckEnabled", typeof(bool), typeof(InputView), true);

		internal InputView()
		{
		}

		public Keyboard Keyboard
		{
			get { return (Keyboard)GetValue(KeyboardProperty); }
			set { SetValue(KeyboardProperty, value); }
		}

		public bool IsSpellCheckEnabled
		{
			get { return (bool)GetValue(IsSpellCheckEnabledProperty); }
			set { SetValue(IsSpellCheckEnabledProperty, value); }
		}
	}
}