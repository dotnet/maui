using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.Forms
{
	internal sealed class ShellContentCollection : ShellElementCollection<ShellContent>
	{
		public ShellContentCollection() : base()
		{
			Inner = new ObservableCollection<ShellContent>();
		}

		protected override bool IsShellElementVisible(BaseShellItem item)
		{
			IShellContentController controller = (IShellContentController)item;
			return controller.Page == null || controller.Page.IsVisible;
		}

		void OnIsPageVisibleChanged(object sender, EventArgs e)
		{
			CheckVisibility((ShellContent)sender);
		}

		protected override void OnElementControllerInserting(IElementController element)
		{
			if (element is IShellContentController controller)
				controller.IsPageVisibleChanged += OnIsPageVisibleChanged;
		}

		protected override void OnElementControllerRemoving(IElementController element)
		{
			if (element is IShellContentController controller)
				controller.IsPageVisibleChanged -= OnIsPageVisibleChanged;
		}
	}
}