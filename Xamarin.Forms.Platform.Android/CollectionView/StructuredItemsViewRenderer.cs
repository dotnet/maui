using System.ComponentModel;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	public class StructuredItemsViewRenderer<TItemsView, TAdapter, TItemsViewSource> : ItemsViewRenderer<TItemsView, TAdapter, TItemsViewSource>
		where TItemsView : StructuredItemsView
		where TAdapter : StructuredItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsViewSource : IItemsViewSource
	{
		StructuredItemsView _itemsView;

		public StructuredItemsViewRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(StructuredItemsView.ItemsLayoutProperty))
			{
				UpdateLayoutManager();
			}
			else if (changedProperty.Is(StructuredItemsView.ItemSizingStrategyProperty))
			{
				UpdateAdapter();
			}
		}

		protected override TAdapter CreateAdapter()
		{
			return (TAdapter)new StructuredItemsViewAdapter<TItemsView, TItemsViewSource>(ItemsView);
		}

		protected override void SetUpNewElement(TItemsView newElement)
		{
			_itemsView = newElement;

			base.SetUpNewElement(newElement);
		}

		protected override IItemsLayout GetItemsLayout()
		{
			return _itemsView.ItemsLayout;
		}
	}
}