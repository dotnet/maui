using ElmSharp;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.Tizen
{
	public abstract class CellRenderer : IRegisterable
	{
		const string HeightProperty = "Height";
		readonly Dictionary<Cell, Dictionary<string, EvasObject>> _realizedNativeViews = new Dictionary<Cell, Dictionary<string, EvasObject>>();

		Native.ListView.ItemContext _currentItem;
		GenItemClass _itemClass;

		protected CellRenderer(string style)
		{
			Style = style;
		}

		public GenItemClass Class
		{
			get
			{
				if (_itemClass == null)
					_itemClass = CreateItemClass(Style);
				return _itemClass;
			}
			protected set
			{
				_itemClass?.Dispose();
				_itemClass = value;
			}
		}

		public virtual void SetGroupMode(bool enable)
		{
		}

		public string Style { get; protected set; }

		protected GenItemClass CreateItemClass(string style)
		{
			return new GenItemClass(style)
			{
				GetTextHandler = GetText,
				GetContentHandler = GetContent,
				DeleteHandler = ItemDeleted,
				ReusableContentHandler = ReusableContent,
			};
		}

		protected virtual bool OnCellPropertyChanged(Cell cell, string property, Dictionary<string, EvasObject> realizedView)
		{
			if (property == HeightProperty)
			{
				return true;
			}
			return false;
		}

		protected virtual Span OnGetText(Cell cell, string part)
		{
			return null;
		}
		protected virtual EvasObject OnGetContent(Cell cell, string part)
		{
			return null;
		}
		protected virtual void OnDeleted(Cell cell)
		{
		}

		protected virtual void OnUnrealizedCell(Cell cell)
		{
		}

		protected virtual EvasObject OnReusableContent(Cell cell, string part, EvasObject old)
		{
			return null;
		}

		protected double FindCellContentHeight(Cell cell)
		{
			ViewCell viewCell = cell as ViewCell;
			if (viewCell != null)
			{
				var parentWidth = (cell.Parent as VisualElement).Width;
				var view = viewCell.View;
				return view.Measure(parentWidth, double.PositiveInfinity).Request.Height;
			}
			else
				return -1;
		}

		static Native.Span ToNative(Span span)
		{
			var nativeSpan = new Native.Span();
			nativeSpan.Text = span.Text;
			nativeSpan.ForegroundColor = span.ForegroundColor.ToNative();
			nativeSpan.FontAttributes = span.FontAttributes;
			nativeSpan.BackgroundColor = span.BackgroundColor.ToNative();
			nativeSpan.FontSize = span.FontSize;
			nativeSpan.FontFamily = span.FontFamily;
			return nativeSpan;
		}

		public void SendCellPropertyChanged(Cell cell, GenItem item, string property)
		{
			Dictionary<string, EvasObject> realizedView = null;
			_realizedNativeViews.TryGetValue(cell, out realizedView);

			// just to prevent null reference exception in OnCellPropertyChanged
			realizedView = realizedView ?? new Dictionary<string, EvasObject>();

			if (property == Cell.IsEnabledProperty.PropertyName)
			{
				item.IsEnabled = cell.IsEnabled;
			}
			// if true was returned, item was updated
			// if it's possible to update the cell property without Update(), return false
			else if (OnCellPropertyChanged(cell, property, realizedView))
			{
				item.Update();
			}
		}

		public void SendUnrealizedCell(Cell cell)
		{
			Dictionary<string, EvasObject> realizedView = null;
			_realizedNativeViews.TryGetValue(cell, out realizedView);
			realizedView?.Clear();
			OnUnrealizedCell(cell);
		}

		internal Native.ListView.ItemContext GetCurrentItem()
		{
			return _currentItem;
		}

		string GetText(object data, string part)
		{
			_currentItem = data as Native.ListView.ItemContext;
			var span = OnGetText(_currentItem.Cell, part);
			return span != null ? ToNative(span).GetMarkupText() : null;
		}

		EvasObject GetContent(object data, string part)
		{
			_currentItem = data as Native.ListView.ItemContext;
			var cell = _currentItem.Cell;
			EvasObject nativeView = OnGetContent(cell, part);
			UpdateRealizedView(cell, part, nativeView);
			return nativeView;
		}

		EvasObject ReusableContent(object data, string part, EvasObject old)
		{
			_currentItem = data as Native.ListView.ItemContext;
			var cell = _currentItem.Cell;
			EvasObject nativeView = OnReusableContent(cell, part, old);
			UpdateRealizedView(cell, part, nativeView);
			return nativeView;
		}

		void UpdateRealizedView(Cell cell, string part, EvasObject nativeView)
		{
			if (part != null && nativeView != null)
			{
				Dictionary<string, EvasObject> realizedView = null;
				_realizedNativeViews.TryGetValue(cell, out realizedView);
				if (realizedView == null)
				{
					realizedView = new Dictionary<string, EvasObject>();
					_realizedNativeViews[cell] = realizedView;
				}
				realizedView[part] = nativeView;
			}
		}

		void ItemDeleted(object data)
		{
			_currentItem = data as Native.ListView.ItemContext;
			var cell = _currentItem.Cell;
			_realizedNativeViews.Remove(cell);
			OnDeleted(cell);
		}
	}
}
