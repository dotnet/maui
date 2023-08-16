#nullable disable
#if __MOBILE__
using System;
using System.Runtime.Versioning;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	[SupportedOSPlatform("ios11.0")]
	class DragAndDropDelegate : NSObject, IUIDragInteractionDelegate, IUIDropInteractionDelegate
	{
		IPlatformViewHandler _viewHandler;
		public DragAndDropDelegate(IPlatformViewHandler viewHandler)
		{
			_viewHandler = viewHandler;
		}

		[Export("dragInteraction:session:willEndWithOperation:")]
		[Preserve(Conditional = true)]
		public void SessionWillEnd(UIDragInteraction interaction, IUIDragSession session, UIDropOperation operation)
		{
			if ((operation == UIDropOperation.Cancel || operation == UIDropOperation.Forbidden) &&
				session.Items.Length > 0 &&
				session.Items[0].LocalObject is CustomLocalStateData cdi)
			{
				this.HandleDropCompleted(cdi.View);
			}
		}

		[Preserve(Conditional = true)]
		public UIDragItem[] GetItemsForBeginningSession(UIDragInteraction interaction, IUIDragSession session)
		{
			var originalPoint = session.LocationInView(_viewHandler.PlatformView);

			return HandleDragStarting((View)_viewHandler.VirtualView, _viewHandler, session);
		}

		[Export("dropInteraction:canHandleSession:")]
		[Preserve(Conditional = true)]
		public bool CanHandleSession(UIDropInteraction interaction, IUIDropSession session)
		{
			if (session.LocalDragSession == null)
				return false;

			if (session.LocalDragSession.Items.Length > 0 &&
				session.LocalDragSession.Items[0].LocalObject is CustomLocalStateData)
			{
				return true;
			}

			return false;
		}

		[Export("dropInteraction:sessionDidExit:")]
		[Preserve(Conditional = true)]
		public void SessionDidExit(UIDropInteraction interaction, IUIDropSession session)
		{
			DataPackage package = null;

			if (session.LocalDragSession.Items.Length > 0 &&
				session.LocalDragSession.Items[0].LocalObject is CustomLocalStateData cdi)
			{
				package = cdi.DataPackage;
			}

			HandleDragLeave((View)_viewHandler.VirtualView, package, session.LocalDragSession);
		}

		[Export("dropInteraction:sessionDidUpdate:")]
		[Preserve(Conditional = true)]
		public UIDropProposal SessionDidUpdate(UIDropInteraction interaction, IUIDropSession session)
		{
			UIDropOperation operation = UIDropOperation.Cancel;

			if (session.LocalDragSession == null)
				return new UIDropProposal(operation);

			DataPackage package = null;

			if (session.LocalDragSession.Items.Length > 0 &&
				session.LocalDragSession.Items[0].LocalObject is CustomLocalStateData cdi)
			{
				package = cdi.DataPackage;
			}

			var dragLocation = session.LocalDragSession.LocationInView(_viewHandler.PlatformView);

			if (HandleDragOver((View)_viewHandler.VirtualView, package, session.LocalDragSession))
			{
				operation = UIDropOperation.Copy;
			}

			return new UIDropProposal(operation);
		}

		[Export("dropInteraction:performDrop:")]
		[Preserve(Conditional = true)]
		public void PerformDrop(UIDropInteraction interaction, IUIDropSession session)
		{
			if (session.LocalDragSession == null)
				return;

			if (session.LocalDragSession.Items.Length > 0 &&
				session.LocalDragSession.Items[0].LocalObject is CustomLocalStateData cdi &&
				_viewHandler.VirtualView is View view)
			{
				HandleDrop(view, cdi.DataPackage);
				HandleDropCompleted(cdi.View);
			}
		}


		void SendEventArgs<TRecognizer>(Action<TRecognizer> func, View view)
			where TRecognizer : class
		{
			var gestures =
				view.GestureRecognizers;

			if (gestures == null)
				return;

			foreach (var gesture in gestures)
			{
				if (gesture is TRecognizer recognizer)
					func(recognizer);
			}
		}

		public UIDragItem[] HandleDragStarting(View element, IPlatformViewHandler handler, IUIDragSession session)
		{
			UIDragItem[] returnValue = null;
			SendEventArgs<DragGestureRecognizer>(rec =>
			{
				if (!rec.CanDrag)
					return;

				var viewHandlerRef = new WeakReference(handler);
				var sessionRef = new WeakReference(session);

				var args = rec.SendDragStarting(element, (relativeTo) => CalculatePosition(relativeTo, viewHandlerRef, sessionRef));

				if (args.Cancel)
					return;

				if (!args.Handled)
				{
					UIImage uIImage = null;
					string clipDescription = String.Empty;
					NSItemProvider itemProvider = null;

					if (handler.PlatformView is UIImageView iv)
						uIImage = iv.Image;

					if (handler.PlatformView is UIButton b && b.ImageView != null)
						uIImage = b.ImageView.Image;

					if (uIImage != null)
					{
						if (uIImage != null)
							itemProvider = new NSItemProvider(uIImage);
						else
							itemProvider = new NSItemProvider(new NSString(""));

						if (args.Data.Image == null && handler.VirtualView is IImageElement imageElement)
							args.Data.Image = imageElement.Source;
					}
					else
					{
						string text = args.Data.Text ?? clipDescription;

						if (String.IsNullOrWhiteSpace(text))
						{
							itemProvider = new NSItemProvider(handler.PlatformView.ConvertToImage());
						}
						else
						{
							itemProvider = new NSItemProvider(new NSString(text));
						}
					}

					var dragItem = new UIDragItem(itemProvider);
					dragItem.LocalObject = new CustomLocalStateData()
					{
						Handler = handler,
						View = handler.VirtualView as View,
						DataPackage = args.Data
					};

					returnValue = new UIDragItem[] { dragItem };
				}
			},
			element);

			return returnValue ?? new UIDragItem[0];
		}

		void HandleDropCompleted(View element)
		{
			var args = new DropCompletedEventArgs();
			SendEventArgs<DragGestureRecognizer>(rec => rec.SendDropCompleted(args), element);
		}

		bool HandleDragLeave(View element, DataPackage dataPackage, IUIDragSession session)
		{
			var viewHandlerRef = new WeakReference(_viewHandler);
			var sessionRef = new WeakReference(session);

			var dragEventArgs = new DragEventArgs(dataPackage, (relativeTo) => CalculatePosition(relativeTo, viewHandlerRef, sessionRef));

			bool validTarget = false;
			SendEventArgs<DropGestureRecognizer>(rec =>
			{
				if (!rec.AllowDrop)
					return;

				rec.SendDragLeave(dragEventArgs);
				validTarget = validTarget || dragEventArgs.AcceptedOperation != DataPackageOperation.None;
			}, element);

			return validTarget;
		}

		bool HandleDragOver(View element, DataPackage dataPackage, IUIDragSession session)
		{
			var viewHandlerRef = new WeakReference(_viewHandler);
			var sessionRef = new WeakReference(session);

			var dragEventArgs = new DragEventArgs(dataPackage, (relativeTo) => CalculatePosition(relativeTo, viewHandlerRef, sessionRef));

			bool validTarget = false;
			SendEventArgs<DropGestureRecognizer>(rec =>
			{
				if (!rec.AllowDrop)
					return;

				rec.SendDragOver(dragEventArgs);
				validTarget = validTarget || dragEventArgs.AcceptedOperation != DataPackageOperation.None;
			}, element);

			return validTarget;
		}

		void HandleDrop(View element, DataPackage datapackage)
		{
			var args = new DropEventArgs(datapackage?.View);
			SendEventArgs<DropGestureRecognizer>(async rec =>
			{
				if (!rec.AllowDrop)
					return;

				try
				{
					await rec.SendDrop(args);
				}
				catch (Exception dropExc)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<DropGestureRecognizer>()?.LogWarning(dropExc, "Error sending drop event");
				}
			}, (View)element);
		}

		static internal Point? CalculatePosition(IElement relativeTo, WeakReference viewHandlerRef, WeakReference sessionRef)
		{

			var viewHandler = viewHandlerRef.Target as IPlatformViewHandler;
			var session = sessionRef.Target as IUIDragSession;

			var virtualView = viewHandler?.VirtualView;
			var platformView = viewHandler?.PlatformView;
			var relativeView = relativeTo?.Handler?.PlatformView as UIView;

			CGPoint dragLocation;

			if (virtualView is null)
				return null;

			// If relativeTo is null we get the location on the screen
			if (relativeTo is null)
			{
				var screenLocation = virtualView.GetLocationOnScreen();
				dragLocation = session.LocationInView(platformView);

				if (!screenLocation.HasValue)
					return null;

				double x = dragLocation.X + screenLocation.Value.X;
				double y = dragLocation.Y + screenLocation.Value.Y;

				return new Point(x, y);
			}

			// If relativeTo is the same as the view sending the event, we get the position relative to itself
			if (relativeTo == virtualView)
			{
				dragLocation = session.LocationInView(platformView);
				return new Point(dragLocation.X, dragLocation.Y);
			}
			else if (relativeView is not null)
			{
				dragLocation = session.LocationInView(relativeView);
				return new Point(dragLocation.X, dragLocation.Y);
			}

			return null;
		}

		class CustomLocalStateData : NSObject
		{
			public View View { get; set; }
			public IViewHandler Handler { get; set; }
			public DataPackage DataPackage { get; set; }
		}
	}
}
#endif
