
#if __MOBILE__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class DragAndDropDelegate : NSObject, IUIDragInteractionDelegate, IUIDropInteractionDelegate 
	{
		public class CustomDragItem : UIDragItem
		{
			public CustomDragItem(NSItemProvider itemProvider, DataPackage dataPackage) : base(itemProvider)
			{
				DataPackage = dataPackage;
			}

			public DataPackage DataPackage { get; }
		}

		#region UIDragInteractionDelegate


		[Export("dragInteraction:session:willEndWithOperation:")]
		[Preserve(Conditional = true)]
		public void SessionWillEnd(UIDragInteraction interaction, IUIDragSession session, UIDropOperation operation)
		{
			if ((operation == UIDropOperation.Cancel || operation == UIDropOperation.Forbidden) &&
				session.Items.Length > 0 &&
				session.Items[0].LocalObject is IVisualElementRenderer ver)
			{
				this.HandleDropCompleted(ver.Element as View);
			}
		}

		public UIDragItem[] GetItemsForBeginningSession(UIDragInteraction interaction, IUIDragSession session)
		{
			if (interaction.View is IVisualElementRenderer renderer && renderer.Element is View view)
				return HandleDragStarting(view, renderer);

			return new UIDragItem[0];
		}
		#endregion

		[Export("dropInteraction:canHandleSession:")]
		[Preserve(Conditional = true)]
		public bool CanHandleSession(UIDropInteraction interaction, IUIDropSession session)
		{
			if (session.LocalDragSession == null)
				return false;

			if (session.LocalDragSession.Items.Length > 0 &&
				session.LocalDragSession.Items[0].LocalObject is IVisualElementRenderer)
			{
				return true;
			}
			
			return false;
		}

		[Export("dropInteraction:sessionDidUpdate:")]
		[Preserve(Conditional = true)]
		public UIDropProposal SessionDidUpdate(UIDropInteraction interaction, IUIDropSession session)
		{
			UIDropOperation operation = UIDropOperation.Cancel;

			if (session.LocalDragSession == null)
				return new UIDropProposal(operation);

			if (interaction.View is IVisualElementRenderer renderer)
			{
				DataPackage package = null;
					
				if(session.LocalDragSession.Items.Length > 0 &&
					session.LocalDragSession.Items[0] is CustomDragItem cdi)
				{
					package = cdi.DataPackage;
				}

				if (HandleDragOver((View)renderer.Element, package))
				{
					operation = UIDropOperation.Copy;
				}
			}

			return new UIDropProposal(operation);
		}

		[Export("dropInteraction:performDrop:")]
		[Preserve(Conditional = true)]
		public void PerformDrop(UIDropInteraction interaction, IUIDropSession session)
		{
			if (session.LocalDragSession == null)
				return;

			if(session.LocalDragSession.Items.Length > 0 && 
				session.LocalDragSession.Items[0] is CustomDragItem cdi)
			{
				HandleDrop(interaction.View, cdi.DataPackage);
				if (cdi.LocalObject is IVisualElementRenderer renderer)
					HandleDropCompleted(renderer.Element as View);
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
				if(gesture is TRecognizer recognizer)
					func(recognizer);
			}
		}

		public UIDragItem[] HandleDragStarting(View element, IVisualElementRenderer renderer)
		{
			UIDragItem[] returnValue = null;
			SendEventArgs<DragGestureRecognizer>(rec =>
			{
				if (!rec.CanDrag)
					return;

				var args = rec.SendDragStarting(element);

				if (args.Cancel)
					return;

				if (!args.Handled)
				{
					UIImage uIImage = null;
					string clipDescription = String.Empty;
					NSItemProvider itemProvider = null;

					if (renderer is IImageVisualElementRenderer iver)
					{
						uIImage = iver.GetImage()?.Image;
						if (uIImage != null)
							itemProvider = new NSItemProvider(uIImage);
						else
							itemProvider = new NSItemProvider(new NSString(""));

						if (args.Data.Image == null && renderer.Element is IImageElement imageElement)
							args.Data.Image = imageElement.Source;
					}
					else
					{
						string text = args.Data.Text ?? clipDescription;

						if (String.IsNullOrWhiteSpace(text))
						{
							itemProvider = new NSItemProvider(renderer.NativeView.ConvertToImage());
						}
						else
						{
							itemProvider = new NSItemProvider(new NSString(text));
						}
					}

					var dragItem = new CustomDragItem(itemProvider, args.Data);
					dragItem.LocalObject = renderer.NativeView;
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

		bool HandleDragOver(View element, DataPackage dataPackage)
		{
			var dragEventArgs = new DragEventArgs(dataPackage);

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

		void HandleDrop(object sender, DataPackage datapackage)
		{
			VisualElement element = null;

			if (sender is IVisualElementRenderer renderer)
			{
				element = renderer.Element;
			}

			var args = new DropEventArgs(datapackage?.View);
			SendEventArgs<DropGestureRecognizer>(async rec =>
			{
				if (!rec.AllowDrop)
					return;

				try
				{
					await rec.SendDrop(args, element);
				}
				catch (Exception e)
				{
					Internals.Log.Warning(nameof(DropGestureRecognizer), $"{e}");
				}
			}, (View)element);
		}
	}

}
#endif
	  