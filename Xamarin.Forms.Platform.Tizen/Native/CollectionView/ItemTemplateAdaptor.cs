using System.Collections;
using System.Collections.Generic;
using ElmSharp;
using ESize = ElmSharp.Size;
using XLabel = Xamarin.Forms.Label;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class ItemDefaultTemplateAdaptor : ItemTemplateAdaptor
	{
		public ItemDefaultTemplateAdaptor(ItemsView itemsView) : base(itemsView)
		{
			ItemTemplate = new DataTemplate(() =>
			{
				return new StackLayout
				{
					BackgroundColor = Color.White,
					Padding = 30,
					Children =
					{
						new XLabel()
					}
				};
			});
		}
		public override void SetBinding(EvasObject native, int index)
		{
			((GetTemplatedView(native) as StackLayout).Children[0] as XLabel).Text = this[index].ToString();
		}

		public override ESize MeasureItem(int widthConstraint, int heightConstraint)
		{
			var view = (View)ItemTemplate.CreateContent();
			if (Count > 0)
			{
				((view as StackLayout).Children[0] as XLabel).Text = this[0].ToString();
			}
			var renderer = Platform.GetOrCreateRenderer(view);
			var request = view.Measure(Forms.ConvertToScaledDP(widthConstraint), Forms.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request;
			renderer.Dispose();
			return request.ToPixel();
		}
	}

	public class ItemTemplateAdaptor : ItemAdaptor
	{
		Dictionary<EvasObject, View> _nativeFormsTable = new Dictionary<EvasObject, View>();
		ItemsView _itemsView;

		public ItemTemplateAdaptor(ItemsView itemsView) : base(itemsView.ItemsSource)
		{
			ItemTemplate = itemsView.ItemTemplate;
			_itemsView = itemsView;
		}

		protected ItemTemplateAdaptor(ItemsView itemsView, IEnumerable items, DataTemplate template) : base(items)
		{
			ItemTemplate = template;
			_itemsView = itemsView;
		}

		protected DataTemplate ItemTemplate { get; set; }

		protected View GetTemplatedView(EvasObject evasObject)
		{
			return _nativeFormsTable[evasObject];
		}

		public override EvasObject CreateNativeView(EvasObject parent)
		{
			var view = ItemTemplate.CreateContent() as View;
			var renderer = Platform.GetOrCreateRenderer(view);
			var native = Platform.GetOrCreateRenderer(view).NativeView;
			view.Parent = _itemsView;
			(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();

			_nativeFormsTable[native] = view;
			return native;
		}

		public override void RemoveNativeView(EvasObject native)
		{
			if (_nativeFormsTable.TryGetValue(native, out View view))
			{
				Platform.GetRenderer(view)?.Dispose();
				_nativeFormsTable.Remove(native);
			}
		}

		public override void SetBinding(EvasObject native, int index)
		{
			if (_nativeFormsTable.TryGetValue(native, out View view))
			{
				view.BindingContext = this[index];
			}
		}

		public override ESize MeasureItem(int widthConstraint, int heightConstraint)
		{
			var view = ItemTemplate.CreateContent() as View;
			var renderer = Platform.GetOrCreateRenderer(view);
			view.Parent = _itemsView;
			if (Count > 0)
				view.BindingContext = this[0];
			var request = view.Measure(Forms.ConvertToScaledDP(widthConstraint), Forms.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request;
			renderer.Dispose();

			return request.ToPixel();
		}

	}
}
