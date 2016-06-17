using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using Object = Java.Lang.Object;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public class CellRenderer : IRegisterable
	{
		static readonly PropertyChangedEventHandler PropertyChangedHandler = OnGlobalCellPropertyChanged;

		static readonly BindableProperty RendererProperty = BindableProperty.CreateAttached("Renderer", typeof(CellRenderer), typeof(Cell), null);

		EventHandler _onForceUpdateSizeRequested;

		public View ParentView { get; set; }

		protected Cell Cell { get; set; }

		public AView GetCell(Cell item, AView convertView, ViewGroup parent, Context context)
		{
			Performance.Start();

			Cell = item;
			Cell.PropertyChanged -= PropertyChangedHandler;

			SetRenderer(Cell, this);

			if (convertView != null)
			{
				Object tag = convertView.Tag;
				var renderHolder = tag as RendererHolder;
				if (renderHolder != null)
				{
					Cell oldCell = renderHolder.Renderer.Cell;
					((ICellController)oldCell).SendDisappearing();

					if (Cell != oldCell)
						SetRenderer(oldCell, null);
				}
			}

			AView view = GetCellCore(item, convertView, parent, context);

			WireUpForceUpdateSizeRequested(item, view);

			var holder = view.Tag as RendererHolder;
			if (holder == null)
				view.Tag = new RendererHolder { Renderer = this };
			else
				holder.Renderer = this;

			Cell.PropertyChanged += PropertyChangedHandler;
			((ICellController)Cell).SendAppearing();

			Performance.Stop();

			return view;
		}

		protected virtual AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
		{
			Performance.Start();

			LayoutInflater inflater = LayoutInflater.FromContext(context);
			const int type = global::Android.Resource.Layout.SimpleListItem1;
			AView view = inflater.Inflate(type, null);

			var textView = view.FindViewById<TextView>(global::Android.Resource.Id.Text1);
			textView.Text = item.ToString();
			textView.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
			view.SetBackgroundColor(global::Android.Graphics.Color.Black);

			Performance.Stop();

			return view;
		}

		protected virtual void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}

		protected void WireUpForceUpdateSizeRequested(Cell cell, AView nativeCell)
		{
			ICellController cellController = cell;
			cellController.ForceUpdateSizeRequested -= _onForceUpdateSizeRequested;

			_onForceUpdateSizeRequested = (sender, e) => 
			{
				// RenderHeight may not be changed, but that's okay, since we
				// don't actually use the height argument in the OnMeasure override.
				nativeCell.Measure(nativeCell.Width, (int)cell.RenderHeight);
				nativeCell.SetMinimumHeight(nativeCell.MeasuredHeight);
				nativeCell.SetMinimumWidth(nativeCell.MeasuredWidth);
			};

			cellController.ForceUpdateSizeRequested += _onForceUpdateSizeRequested;
		}

		internal static CellRenderer GetRenderer(BindableObject cell)
		{
			return (CellRenderer)cell.GetValue(RendererProperty);
		}

		internal static void SetRenderer(BindableObject cell, CellRenderer renderer)
		{
			cell.SetValue(RendererProperty, renderer);
		}

		static void OnGlobalCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var cell = (Cell)sender;
			CellRenderer renderer = GetRenderer(cell);
			if (renderer == null)
			{
				cell.PropertyChanged -= PropertyChangedHandler;
				return;
			}

			renderer.OnCellPropertyChanged(sender, e);
		}

		class RendererHolder : Object
		{
			public CellRenderer Renderer;
		}
	}
}