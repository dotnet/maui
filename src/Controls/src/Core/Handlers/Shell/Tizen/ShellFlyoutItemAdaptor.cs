using System.Collections;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellFlyoutItemAdaptor : ItemTemplateAdaptor
	{
		Shell _shell;
		bool _hasHeader;

		public ShellFlyoutItemAdaptor(Shell shell, IEnumerable items, bool hasHeader) : base(shell, items, GetTemplate())
		{
			_shell = shell;
			_hasHeader = hasHeader;
		}

		protected override bool IsSelectable => true;

		protected override View? CreateHeaderView()
		{
			if (!_hasHeader)
			{
				_headerCache = null;
				return null;
			}

			_headerCache = ((IShellController)_shell).FlyoutHeader;
			return _headerCache;
		}

		static DataTemplate GetTemplate()
		{
			return new FlyoutItemDataTemplateSelector();
		}
	}

	class FlyoutItemDataTemplateSelector : DataTemplateSelector
	{
		DataTemplate DefaultItemTemplate { get; }

		public FlyoutItemDataTemplateSelector()
		{
			DefaultItemTemplate = new DataTemplate(() =>
			{
				return new ShellFlyoutItemView();
			});
		}

		protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
		{
			DataTemplate template = DefaultItemTemplate;

			if (item != null && item is BindableObject bo)
			{
				BindableProperty? bp = null;
				var bindableObjectWithTemplate = Shell.GetBindableObjectWithFlyoutItemTemplate(bo);

				if (bo is IMenuItemController)
					bp = Shell.MenuItemTemplateProperty;
				else
					bp = Shell.ItemTemplateProperty;

				if (bindableObjectWithTemplate.IsSet(bp) || container.IsSet(bp))
				{
					DataTemplate? dataTemplate = (container as IShellController)?.GetFlyoutItemDataTemplate(bo);
					template = dataTemplate.SelectDataTemplate(item, container);
				}
			}
			return template;
		}
	}
}
