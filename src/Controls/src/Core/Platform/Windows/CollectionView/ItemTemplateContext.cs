#nullable disable
using System;

namespace Microsoft.Maui.Controls.Platform
{
	public class ItemTemplateContext
	{
		readonly WeakReference<BindableObject> _container;

		public DataTemplate FormsDataTemplate { get; }
		public IMauiContext MauiContext { get; }
		public object Item { get; }
		public BindableObject Container => _container.TryGetTarget(out var c) ? c : null;
		public double ItemHeight { get; }
		public double ItemWidth { get; }
		public Thickness ItemSpacing { get; }

		internal ItemTemplateContext(DataTemplate formsDataTemplate, object item, BindableObject container,
			double? height = null, double? width = null, Thickness? itemSpacing = null, IMauiContext mauiContext = null)
		{
			FormsDataTemplate = formsDataTemplate;
			Item = item;
			_container = new(container);
			MauiContext = mauiContext;

			if (height.HasValue)
				ItemHeight = height.Value;

			if (width.HasValue)
				ItemWidth = width.Value;

			if (itemSpacing.HasValue)
				ItemSpacing = itemSpacing.Value;
		}
	}
}