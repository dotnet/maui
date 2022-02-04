using System;
using System.Collections.Generic;
using ElmSharp;
using EColor = ElmSharp.Color;
using TThemeConstants = Tizen.UIExtensions.ElmSharp.ThemeConstants;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellSearchResultList : GenList
	{
		GenItemClass _defaultClass = null;
		IReadOnlyList<object> _itemsSource;

		public ShellSearchResultList(IMauiContext context) : base(context?.GetNativeParent())
		{
			MauiContext = context;

			SetAlignment(-1, -1);
			SetWeight(1, 1);
			AllowFocus(true);

			Homogeneous = true;
			SelectionMode = GenItemSelectionMode.Always;
			BackgroundColor = EColor.White;

			_defaultClass = new GenItemClass(TThemeConstants.GenItemClass.Styles.Full)
			{
				GetContentHandler = GetContent,
			};
		}

		public int Height { get; private set; }

		public IReadOnlyList<object> ItemsSource
		{
			get => _itemsSource;
			set
			{
				Clear();
				Height = 0;

				_itemsSource = value;
				foreach (var item in _itemsSource)
				{
					Append(item);
				}
			}
		}

		protected IMauiContext MauiContext { get; private set; }

		protected EvasObject NativeParent
		{
			get => MauiContext.GetNativeParent();
		}

		public void UpdateLayout()
		{
			if (FirstItem != null && Height == 0)
			{
				var view = FirstItem.Data as View;
				var native = view.ToPlatform(MauiContext);
				var measured = view.Measure(DPExtensions.ConvertToScaledDP(Geometry.Width), double.PositiveInfinity);
				Height = DPExtensions.ConvertToScaledPixel(measured.Request.Height);
			}

			var bound = Geometry;
			bound.Height = Math.Min(Height * _itemsSource.Count, bound.Width);
			Geometry = bound;

			UpdateRealizedItems();
		}

		public DataTemplate ItemTemplate { get; set; }

		EvasObject GetContent(object data, string part)
		{
			var view = data as View;
			var native = view.ToPlatform(MauiContext);

			if (Height == 0)
			{
				var measured = view.Measure(DPExtensions.ConvertToScaledDP(Geometry.Width), double.PositiveInfinity);
				Height = DPExtensions.ConvertToScaledPixel(measured.Request.Height);
			}

			native.MinimumHeight = Height;
			return native;
		}

		void Append(object data)
		{
			var view = ItemTemplate.CreateContent() as View;
			view.Parent = Shell.Current;
			view.BindingContext = data;
			Append(_defaultClass, view);
		}
	}
}
