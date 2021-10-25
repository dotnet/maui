//using Microsoft.UI.Xaml;
//using Microsoft.UI.Xaml.Controls;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Microsoft.Maui
//{
//	internal class IrDataTemplateSelector : IElementFactory, IDisposable
//	{
//		public IrDataTemplateSelector(IMauiContext context, PositionalViewSelector positionalViewSelector)
//		{
//			MauiContext = context;
//			PositionalViewSelector = positionalViewSelector;
//		}

//		readonly object lockObj = new object();
//		protected readonly IMauiContext MauiContext;

//		protected readonly PositionalViewSelector PositionalViewSelector;

//		internal record IrRecycledElement(string reuseId, UIElement element);

//		List<IrElementContainer> recycledElements = new ();

//		public UIElement GetElement(UI.Xaml.ElementFactoryGetArgs args)
//		{
//			if (args.Data is IrDataWrapper dataWrapper)
//			{
//				var info = dataWrapper.position;
//				if (info == null)
//					return null;

//				var data = PositionalViewSelector.Adapter.DataFor(info.Kind, info.SectionIndex, info.ItemIndex);

//				var reuseId = PositionalViewSelector?.ViewSelector?.GetReuseId(info, data);

//				IrElementContainer container;

//				lock (lockObj)
//				{
//					container = recycledElements?.FirstOrDefault(re => re.ReuseId == reuseId)
//						?? new IrElementContainer(MauiContext, reuseId);
//				}

//				var view = PositionalViewSelector?.ViewSelector?.CreateView(info, data);

//				if (view is IPositionInfo viewWithPositionInfo)
//					viewWithPositionInfo.SetPositionInfo(info);

//				container.Update(info, view);

//				return container;
//			}

//			return null;
//		}

//		public void RecycleElement(UI.Xaml.ElementFactoryRecycleArgs args)
//		{
//			if (args.Element is IrElementContainer container && container != null)
//			{
//				lock(lockObj)
//					recycledElements.Add(container);
//			}
//		}

//		public void Reset()
//		{
//			lock (lockObj)
//				recycledElements.Clear();
//		}

//		public void Dispose()
//		{
//			lock (lockObj)
//				recycledElements.Clear();
//		}
//	}

//	internal class IrElementContainer : ContentControl
//	{
//		public IrElementContainer(IMauiContext context, string reuseId)
//			: base()
//		{
//			MauiContext = context;
//			ReuseId = reuseId;
//		}

//		public readonly string ReuseId;
//		public readonly IMauiContext MauiContext;

//		public PositionInfo PositionInfo { get; private set; }

//		public IView VirtualView { get; private set; }

//		public UIElement NativeView { get; private set; }

//		public void Update(PositionInfo positionInfo, IView newView)
//		{
//			PositionInfo = positionInfo;

//			SwapView(newView);
//		}

//		void SwapView(IView newView)
//		{
//			if (VirtualView == null || VirtualView.Handler == null || NativeView == null)
//			{
//				NativeView = newView.ToNative(MauiContext);
//				VirtualView = newView;

//				Content = NativeView;
//			}
//			else
//			{
//				var handler = VirtualView.Handler;
//				newView.Handler = handler;
//				handler.SetVirtualView(newView);
//				VirtualView = newView;
//			}

//			VirtualView.InvalidateMeasure();
//			VirtualView.InvalidateArrange();
//		}
//	}
//}
