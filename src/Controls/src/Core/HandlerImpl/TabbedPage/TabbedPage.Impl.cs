#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/TabbedPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.TabbedPage']/Docs/*" />
	public partial class TabbedPage : ITabbedView
	{
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			// We don't want forcelayout to call the legacy
			// Page.LayoutChildren code
		}

		partial void OnHandlerChangingPartial(HandlerChangingEventArgs args);
		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (args.NewHandler == null)
			{
				PagesChanged -= OnPagesChanged;
				WireUnwireChanges(false);
			}
			else if (args.OldHandler == null)
			{
				PagesChanged += OnPagesChanged;
				WireUnwireChanges(true);
			}

			OnHandlerChangingPartial(args);
			void OnPagesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
			{
				WireUnwireChanges(false);
				Handler?.UpdateValue(TabbedPage.ItemsSourceProperty.PropertyName);
				WireUnwireChanges(true);
			}

			void WireUnwireChanges(bool wire)
			{
				foreach (var page in Children)
				{
					if (wire)
						page.PropertyChanged += OnPagePropertyChanged;
					else
						page.PropertyChanged -= OnPagePropertyChanged;
				}
			}

			void OnPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				if (e.PropertyName == Page.TitleProperty.PropertyName)
					Handler?.UpdateValue(TabbedPage.ItemsSourceProperty.PropertyName);
			}
		}
	}
}
