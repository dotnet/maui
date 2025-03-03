#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[DebuggerTypeProxy(typeof(ShellContentCollectionDebugView))]
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

		string GetDebuggerDisplay() => $"Count = {Count}";

		/// <summary>
		/// Provides a debug view for the <see cref="ShellContentCollection"/> class.
		/// </summary>
		/// <param name="collection">The <see cref="ShellSectionCollection"/> instance to debug.</param>
		private sealed class ShellContentCollectionDebugView(ShellContentCollection collection)
		{
			public IReadOnlyCollection<ShellContent> VisibleItemsReadOnly => collection.VisibleItemsReadOnly;
		}
	}
}