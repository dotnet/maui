using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui
{
	public partial class VirtualListViewHandler : ViewHandler<IVirtualListView, ItemsRepeaterScrollHost>
	{
		ItemsRepeaterScrollHost itemsRepeaterScrollHost;
		ScrollViewer scrollViewer;
		ItemsRepeater itemsRepeater;
		IrDataTemplateSelector dataTemplateSelector;
		IrSource irSource;
		VirtualListViewDataTemplateSelector templateSelector;

		internal PositionalViewSelector PositionalViewSelector { get; private set; }

		protected override ItemsRepeaterScrollHost CreateNativeView()
		{
			itemsRepeaterScrollHost = new ItemsRepeaterScrollHost();
			scrollViewer = new ScrollViewer();
			itemsRepeater = new ItemsRepeater();

			scrollViewer.Content = itemsRepeater;
			itemsRepeaterScrollHost.ScrollViewer = scrollViewer;

			return itemsRepeaterScrollHost;
		}

		protected override void ConnectHandler(ItemsRepeaterScrollHost nativeView)
		{
			base.ConnectHandler(nativeView);

			templateSelector = new VirtualListViewDataTemplateSelector(VirtualView, NativeView.Resources["ContainerTemplate"] as VirtualListViewDataTemplate);

			PositionalViewSelector = new PositionalViewSelector(VirtualView);
			irSource = new IrSource(PositionalViewSelector);

			itemsRepeater.ItemsSource = irSource;

			dataTemplateSelector = new IrDataTemplateSelector(MauiContext, PositionalViewSelector);
			itemsRepeater.ItemTemplate = templateSelector;

			//itemsRepeater.ItemTemplate = dataTemplateSelector;
			
		}



		protected override void DisconnectHandler(ItemsRepeaterScrollHost nativeView)
		{
			itemsRepeater.ItemTemplate = null;
			dataTemplateSelector.Dispose();
			dataTemplateSelector = null;

			itemsRepeater.ItemsSource = null;
			irSource = null;

			base.DisconnectHandler(nativeView);
		}

		public void InvalidateData()
		{
			dataTemplateSelector?.Reset();
			irSource?.Reset();
		}

		public static void MapAdapter(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler?.InvalidateData();

		public static void MapHeader(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler?.InvalidateData();

		public static void MapFooter(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler?.InvalidateData();

		public static void MapViewSelector(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler?.InvalidateData();

		public static void MapSelectionMode(VirtualListViewHandler handler, IVirtualListView virtualListView)
		{ }

		public static void MapInvalidateData(VirtualListViewHandler handler, IVirtualListView virtualListView)
			=> handler?.InvalidateData();

		public static void MapSetSelected(VirtualListViewHandler handler, IVirtualListView virtualListView, object? parameter)
		{
			if (parameter is ItemPosition[] items)
			{
			}
		}

		public static void MapSetDeselected(VirtualListViewHandler handler, IVirtualListView virtualListView, object? parameter)
		{
			if (parameter is ItemPosition[] items)
			{
				//
			}
		}

		public static void MapOrientation(VirtualListViewHandler handler, IVirtualListView virtualListView, object? parameter)
		{
		}
	}
}
