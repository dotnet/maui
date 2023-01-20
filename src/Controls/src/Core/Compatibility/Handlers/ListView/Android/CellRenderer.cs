#nullable disable
using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using AView = Android.Views.View;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class CellRenderer : ElementHandler<Cell, AView>, IRegisterable
	{
		static readonly PropertyChangedEventHandler PropertyChangedHandler = OnGlobalCellPropertyChanged;

		EventHandler _onForceUpdateSizeRequested;

		public static PropertyMapper<Cell, CellRenderer> Mapper =
				new PropertyMapper<Cell, CellRenderer>(ElementHandler.ElementMapper);

		public static CommandMapper<Cell, CellRenderer> CommandMapper =
			new CommandMapper<Cell, CellRenderer>(ElementHandler.ElementCommandMapper);

		public CellRenderer() : base(Mapper, CommandMapper)
		{
		}

		public View ParentView { get; set; }

		protected Cell Cell { get; set; }

		protected override AView CreatePlatformElement()
		{
			var creationArgs = VirtualView.ConvertView;
			VirtualView.ConvertView = null;
			return GetCell(VirtualView, creationArgs, null, (MauiContext.Context));
		}

		public AView GetCell(Cell item, AView convertView, ViewGroup parent, Context context)
		{
			if (item.Parent is View parentView)
				ParentView = parentView;

			if (parent == null && ParentView?.Handler?.PlatformView is ViewGroup platformParent)
				parent = platformParent;

			Performance.Start(out string reference);

			Cell = item;
			Cell.PropertyChanged -= PropertyChangedHandler;

			if (convertView != null)
			{
				Object tag = convertView.Tag;
				CellRenderer renderer = (tag as RendererHolder)?.Renderer;

				Cell oldCell = renderer?.Cell;

				if (oldCell != null)
				{
					((ICellController)oldCell).SendDisappearing();

					if (Cell != oldCell)
					{
						oldCell.Handler?.DisconnectHandler();
					}
				}
			}

			AView view = GetCellCore(item, convertView, parent, context);

			WireUpForceUpdateSizeRequested(item, view);

			var holder = view.Tag as RendererHolder;
			if (holder == null)
				view.Tag = new RendererHolder(this);
			else
				holder.Renderer = this;

			Cell.PropertyChanged += PropertyChangedHandler;
			((ICellController)Cell).SendAppearing();

			Performance.Stop(reference);

			return view;
		}

		protected virtual AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
		{
			Performance.Start(out string reference, "GetCellCore");

			LayoutInflater inflater = LayoutInflater.FromContext(context);
			const int type = global::Android.Resource.Layout.SimpleListItem1;
			AView view = inflater.Inflate(type, null);

			var textView = view.FindViewById<TextView>(global::Android.Resource.Id.Text1);
			textView.Text = item.ToString();
			textView.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
			view.SetBackgroundColor(global::Android.Graphics.Color.Black);

			Performance.Stop(reference, "GetCellCore");

			return view;
		}

		protected virtual void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}

		protected void WireUpForceUpdateSizeRequested(Cell cell, AView platformCell)
		{
			ICellController cellController = cell;
			cellController.ForceUpdateSizeRequested -= _onForceUpdateSizeRequested;

			_onForceUpdateSizeRequested = (sender, e) =>
			{
				if (platformCell.Handle == IntPtr.Zero)
					return;
				// RenderHeight may not be changed, but that's okay, since we
				// don't actually use the height argument in the OnMeasure override.
				platformCell.Measure(platformCell.Width, (int)cell.RenderHeight);
				platformCell.SetMinimumHeight(platformCell.MeasuredHeight);
				platformCell.SetMinimumWidth(platformCell.MeasuredWidth);
			};

			cellController.ForceUpdateSizeRequested += _onForceUpdateSizeRequested;
		}

		internal static CellRenderer GetRenderer(Cell cell)
		{
			return (CellRenderer)cell.Handler;
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
			readonly WeakReference<CellRenderer> _rendererRef;

			public RendererHolder(CellRenderer renderer)
			{
				_rendererRef = new WeakReference<CellRenderer>(renderer);
			}

			public CellRenderer Renderer
			{
				get
				{
					CellRenderer renderer;
					return _rendererRef.TryGetTarget(out renderer) ? renderer : null;
				}
				set { _rendererRef.SetTarget(value); }
			}
		}
	}
}