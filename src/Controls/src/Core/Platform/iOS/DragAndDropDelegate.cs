#nullable disable
#if __MOBILE__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;

using Foundation;
using Microsoft.Extensions.Logging;
using ObjCRuntime;
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
				this.HandleDropCompleted(cdi.View, new PlatformDropCompletedEventArgs(cdi.View.Handler.PlatformView as UIView, interaction, session, operation));
			}
		}

		[Preserve(Conditional = true)]
		public UIDragItem[] GetItemsForBeginningSession(UIDragInteraction interaction, IUIDragSession session)
		{
			return HandleDragStarting((View)_viewHandler.VirtualView, _viewHandler, new PlatformDragStartingEventArgs(_viewHandler.PlatformView, interaction, session));
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

			if (HandleDragLeave((View)_viewHandler.VirtualView, package, new PlatformDragEventArgs(_viewHandler.PlatformView, interaction, session)))
			{
			}
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

			var platformArgs = new PlatformDragEventArgs(_viewHandler.PlatformView, interaction, session);
			if (HandleDragOver((View)_viewHandler.VirtualView, package, platformArgs))
			{
				if (platformArgs._dropProposal is not null)
					return platformArgs._dropProposal;

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
				HandleDrop(view, cdi.DataPackage, new PlatformDropEventArgs(cdi.View.Handler.PlatformView as UIView, interaction, session));
				HandleDropCompleted(cdi.View, new PlatformDropCompletedEventArgs(cdi.View.Handler.PlatformView as UIView, interaction, session));
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

		public UIDragItem[] HandleDragStarting(View element, IPlatformViewHandler handler, PlatformDragStartingEventArgs platformArgs)
		{
			UIDragItem[] returnValue = null;
			SendEventArgs<DragGestureRecognizer>(rec =>
			{
				if (!rec.CanDrag)
					return;

				var args = rec.SendDragStarting(element, platformArgs);

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

					var dragItem = new UIDragItem(args.PlatformArgs._itemProvider ?? itemProvider);
					dragItem.LocalObject = new CustomLocalStateData()
					{
						Handler = handler,
						View = handler.VirtualView as View,
						DataPackage = args.PlatformArgs._dataPackage ?? args.Data
					};
					if (args.PlatformArgs._previewProvider is not null)
						dragItem.PreviewProvider = args.PlatformArgs._previewProvider;

					returnValue = new UIDragItem[] { dragItem };
				}
			},
			element);

			return returnValue ?? new UIDragItem[0];
		}

		void HandleDropCompleted(View element, PlatformDropCompletedEventArgs platformArgs)
		{
			var args = new DropCompletedEventArgs();
			args.PlatformArgs = platformArgs;
			SendEventArgs<DragGestureRecognizer>(rec => rec.SendDropCompleted(args), element);
		}

		bool HandleDragLeave(View element, DataPackage dataPackage, PlatformDragEventArgs platformArgs)
		{
			var dragEventArgs = new DragEventArgs(dataPackage);
			dragEventArgs.PlatformArgs = platformArgs;

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

		bool HandleDragOver(View element, DataPackage dataPackage, PlatformDragEventArgs platformArgs)
		{
			var dragEventArgs = new DragEventArgs(dataPackage);
			dragEventArgs.PlatformArgs = platformArgs;

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

		void HandleDrop(View element, DataPackage datapackage, PlatformDropEventArgs platformArgs)
		{
			var args = new DropEventArgs(datapackage?.View);
			args.PlatformArgs = platformArgs;
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

		class CustomLocalStateData : NSObject
		{
			public View View { get; set; }
			public IViewHandler Handler { get; set; }
			public DataPackage DataPackage { get; set; }
		}
	}
}
#endif
