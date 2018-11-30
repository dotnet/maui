namespace Xamarin.Forms
{
	public class MenuShellItem : ShellItem
	{
		internal MenuShellItem(MenuItem menuItem)
		{
			MenuItem = menuItem;

			SetBinding(TitleProperty, new Binding("Text", BindingMode.OneWay, source: menuItem));
			SetBinding(IconProperty, new Binding("Icon", BindingMode.OneWay, source: menuItem));
		}

		public MenuItem MenuItem { get; }
	}
}
