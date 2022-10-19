using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	internal sealed class ShellContentCollection : ShellElementCollection<ShellContent>
	{
		public ShellContentCollection() : base()
		{
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
			base.OnElementControllerInserting(element);
			if (element is IShellContentController controller)
				controller.IsPageVisibleChanged += OnIsPageVisibleChanged;
		}

		protected override void OnElementControllerRemoving(IElementController element)
		{
			base.OnElementControllerRemoving(element);
			if (element is IShellContentController controller)
				controller.IsPageVisibleChanged -= OnIsPageVisibleChanged;
		}
	}
}