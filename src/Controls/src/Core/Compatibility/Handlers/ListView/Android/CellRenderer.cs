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
#pragma warning disable CS0618 // Type or member is obsolete
	public class CellRenderer : ElementHandler<Cell, AView>, IRegisterable
#pragma warning restore CS0618 // Type or member is obsolete
	{
		static readonly PropertyChangedEventHandler PropertyChangedHandler = OnGlobalCellPropertyChanged;

#pragma warning disable CS0618 // Type or member is obsolete
		public static PropertyMapper<Cell, CellRenderer> Mapper =
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
				new PropertyMapper<Cell, CellRenderer>(ElementHandler.ElementMapper);
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
		public static CommandMapper<Cell, CellRenderer> CommandMapper =
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			new CommandMapper<Cell, CellRenderer>(ElementHandler.ElementCommandMapper);
#pragma warning restore CS0618 // Type or member is obsolete

		public CellRenderer() : base(Mapper, CommandMapper)
		{
		}

		public View ParentView { get; set; }

#pragma warning disable CS0618 // Type or member is obsolete
		protected Cell Cell { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete

		protected override AView CreatePlatformElement()
		{
			var creationArgs = VirtualView.ConvertView;
			VirtualView.ConvertView = null;
			return GetCell(VirtualView, creationArgs, null, (MauiContext.Context));
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public AView GetCell(Cell item, AView convertView, ViewGroup parent, Context context)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			if (item.Parent is View parentView)
				ParentView = parentView;

			if (parent == null && ParentView?.Handler?.PlatformView is ViewGroup platformParent)
				parent = platformParent;

			if (Cell is ICellController cellController)
				cellController.ForceUpdateSizeRequested -= OnForceUpdateSizeRequested;

			Cell = item;
			Cell.PropertyChanged -= PropertyChangedHandler;

			if (convertView is not null)
			{
				Object tag = convertView.Tag;
				CellRenderer renderer = (tag as RendererHolder)?.Renderer;

#pragma warning disable CS0618 // Type or member is obsolete
				Cell oldCell = renderer?.Cell;
#pragma warning restore CS0618 // Type or member is obsolete

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

			return view;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		protected virtual AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			LayoutInflater inflater = LayoutInflater.FromContext(context);
			const int type = global::Android.Resource.Layout.SimpleListItem1;
			AView view = inflater.Inflate(type, null);

			var textView = view.FindViewById<TextView>(global::Android.Resource.Id.Text1);
			textView.Text = item.ToString();
			textView.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
			view.SetBackgroundColor(global::Android.Graphics.Color.Black);

			return view;
		}

		protected virtual void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}

#pragma warning disable CS0618 // Type or member is obsolete
		protected void WireUpForceUpdateSizeRequested(Cell cell, AView platformCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			ICellController cellController = cell;
			cellController.ForceUpdateSizeRequested -= OnForceUpdateSizeRequested;
			cellController.ForceUpdateSizeRequested += OnForceUpdateSizeRequested;
		}

		protected override void DisconnectHandler(AView platformView)
		{
			if (Cell is ICellController cellController)
				cellController.ForceUpdateSizeRequested -= OnForceUpdateSizeRequested;

			base.DisconnectHandler(platformView);
		}

		static void OnForceUpdateSizeRequested(object sender, EventArgs e)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (sender is not Cell cellInner)
				return;
#pragma warning restore CS0618 // Type or member is obsolete

			if (cellInner.Handler is not IElementHandler elementHandler ||
				elementHandler.PlatformView is not AView pCell ||
				!pCell.IsAlive())
			{
				return;
			}

			// RenderHeight may not be changed, but that's okay, since we
			// don't actually use the height argument in the OnMeasure override.
			pCell.Measure(pCell.Width, (int)cellInner.RenderHeight);
			pCell.SetMinimumHeight(pCell.MeasuredHeight);
			pCell.SetMinimumWidth(pCell.MeasuredWidth);
		}

#pragma warning disable CS0618 // Type or member is obsolete
		internal static CellRenderer GetRenderer(Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return (CellRenderer)cell.Handler;
		}

		static void OnGlobalCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var cell = (Cell)sender;
#pragma warning restore CS0618 // Type or member is obsolete
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