#nullable enable

using System.Collections;
using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellFlyoutItemTemplateAdaptor : ItemTemplateAdaptor
	{
		Shell _shell;
		bool _hasHeader;

		public ShellFlyoutItemTemplateAdaptor(Shell shell, IEnumerable items, bool hasHeader) : base(shell, items, GetTemplate())
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
			return new DataTemplate(() =>
			{
				return new ShellItemTemplatedView();
			});
		}
	}
}
