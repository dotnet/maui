namespace Xamarin.Forms
{
	public class ShellGroupItem : BaseShellItem
	{
		public static readonly BindableProperty FlyoutDisplayOptionsProperty =
			BindableProperty.Create(nameof(FlyoutDisplayOptions), typeof(FlyoutDisplayOptions), typeof(ShellItem), FlyoutDisplayOptions.AsSingleItem, BindingMode.OneTime);

		public FlyoutDisplayOptions FlyoutDisplayOptions
		{
			get { return (FlyoutDisplayOptions)GetValue(FlyoutDisplayOptionsProperty); }
			set { SetValue(FlyoutDisplayOptionsProperty, value); }
		}

		internal virtual ShellElementCollection ShellElementCollection { get; }
	}
}