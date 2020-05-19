namespace System.Maui.Platform.Tizen.Watch
{
	public class ShellRendererFactory
	{
		static ShellRendererFactory _instance;
		public static ShellRendererFactory Default
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ShellRendererFactory();
				}
				return _instance;
			}
			set
			{
				_instance = value;
			}

		}

		public virtual IShellItemRenderer CreateItemRenderer(ShellItem item)
		{
			if (item.Items.Count == 1)
			{
				return CreateShellNavigationRenderer(item.CurrentItem);
			}
			return new ShellItemRenderer(item);
		}

		public virtual IShellItemRenderer CreateShellNavigationRenderer(ShellSection item)
		{
			return new ShellSectionNavigationRenderer(item);
		}

		public virtual IShellItemRenderer CreateItemRenderer(ShellSection item)
		{
			if (item.Items.Count == 1)
			{
				return CreateItemRenderer(item.CurrentItem);
			}
			return new ShellSectionItemsRenderer(item);
		}

		public virtual IShellItemRenderer CreateItemRenderer(ShellContent item)
		{
			return new ShellContentRenderer(item);
		}
	}
}
