using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using AUri = Android.Net.Uri;
using AView = Android.Views.View;
using ADragFlags = Android.Views.DragFlags;
using System.IO;
using Android.Sax;
using Javax.Xml.Transform;

namespace Xamarin.Forms.Platform.Android
{
	internal class DragAndDropGestureHandler : Java.Lang.Object, AView.IOnDragListener
	{
		bool _isDisposed;
		static Dictionary<VisualElement, DataPackage> _dragSource;

		public DragAndDropGestureHandler(Func<View> getView, Func<AView> getControl)
		{
			_ = getView ?? throw new ArgumentNullException(nameof(getView));
			_ = getControl ?? throw new ArgumentNullException(nameof(getControl));

			GetView = getView;
			GetControl = getControl;
			SetupHandlerForDrop();

			if (_dragSource == null)
				_dragSource = new Dictionary<VisualElement, DataPackage>();
		}

		Func<View> GetView { get; }
		Func<AView> GetControl { get; }

		public bool HasAnyDragGestures()
		{
			var gestures = GetView()?.GestureRecognizers;
			if (gestures == null || gestures.Count == 0)
				return false;

			foreach (var gesture in gestures)
				if (gesture is DragGestureRecognizer)
					return true;

			return false;
		}

		public bool HasAnyDropGestures()
		{
			var gestures = GetView()?.GestureRecognizers;
			if (gestures == null || gestures.Count == 0)
				return false;

			foreach (var gesture in gestures)
				if (gesture is DropGestureRecognizer)
					return true;

			return false;
		}

		public void SetupHandlerForDrop()
		{
			if (HasAnyDropGestures())
				GetControl()?.SetOnDragListener(this);
			else
				GetControl()?.SetOnDragListener(null);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				var control = GetControl();
				if (control.IsAlive())
				{
					control.SetOnDragListener(null);
				}

				var view = GetView();
				if(view != null && _dragSource.ContainsKey(view))
				{
					_dragSource.Remove(view);
				}
			}

			base.Dispose(disposing);

		}

		void SendEventArgs<TRecognizer>(Action<TRecognizer> func, View view = null)
		{
			view = view ?? GetView();
			var renderer = Platform.GetRenderer(view);

			if (!renderer.View.IsAlive() && view != null)
				return;

			var gestures =
				view
					.GestureRecognizers?
					.OfType<TRecognizer>();

			if (gestures == null)
				return;

			foreach (var gesture in gestures)
			{
				func(gesture);
			}
		}

		public bool OnDrag(AView v, DragEvent e)
		{
			DataPackage package = null;
			if (e.LocalState is IVisualElementRenderer renderer &&
				_dragSource.ContainsKey(renderer.Element))
			{
				package = _dragSource[renderer.Element];
			}
			else
				renderer = null;

			switch (e.Action)
			{
				case DragAction.Started:
					return HandleDragOver(package);
				case DragAction.Drop:
					{
						HandleDrop(e.LocalState, package, e.ClipData);
						if (renderer != null && _dragSource.ContainsKey(renderer.Element))
						{
							HandleDropCompleted(renderer.Element as View);
							_dragSource.Remove(renderer.Element);
						}

						return true;
					}
				case DragAction.Ended:
					{
						if (renderer != null && _dragSource.ContainsKey(renderer.Element))
						{
							HandleDropCompleted(renderer.Element as View);
							_dragSource.Remove(renderer.Element);
						}
					}
					break;
				case DragAction.Entered:
					{
						return HandleDragOver(package);
					}
				case DragAction.Location:
				case DragAction.Exited:
					return true;
			}

			return false;
		}

		void HandleDropCompleted(View element)
		{
			var args = new DropCompletedEventArgs();
			SendEventArgs<DragGestureRecognizer>(rec => rec.SendDropCompleted(args), element);
		}

		bool HandleDragOver(DataPackage package)
		{
			var dragEventArgs = new DragEventArgs(package);
			bool validTarget = false;
			SendEventArgs<DropGestureRecognizer>(rec =>
			{
				if (!rec.AllowDrop)
					return;

				rec.SendDragOver(dragEventArgs);
				validTarget = validTarget || dragEventArgs.AcceptedOperation != DataPackageOperation.None;
			});

			return validTarget;
		}

		void HandleDrop(object sender, DataPackage datapackage, ClipData clipData)
		{
			if(datapackage == null)
			{
				var clipItem = clipData.GetItemAt(0);

				var uri = clipItem.Uri;
				var text = clipItem.Text;
				datapackage = new DataPackage();
				datapackage.Text = text ?? uri.ToString();
			}

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
				catch(Exception e)
				{
					Internals.Log.Warning(nameof(DropGestureRecognizer), $"{e}");
				}
			});
		}

		public void OnLongPress(MotionEvent e)
		{
			if (!HasAnyDragGestures())
				return;

			SendEventArgs<DragGestureRecognizer>(rec =>
			{
				if (!rec.CanDrag)
					return;

				var element = GetView();
				var renderer = Platform.GetRenderer(element);
				var v = renderer.View;

				if (v.Handle == IntPtr.Zero)
					return;

				var args = rec.SendDragStarting(element);

				if (args.Cancel)
					return;

				_dragSource[element] = args.Data;
				string clipDescription = FastRenderers.AutomationPropertiesProvider.ConcatenateNameAndHelpText(element) ?? String.Empty;
				ClipData.Item item = null;
				List<string> mimeTypes = new List<string>();

				if (!args.Handled)
				{
					if (args.Data.Image != null)
					{						
						mimeTypes.Add("image/jpeg");						
						item = ConvertToClipDataItem(args.Data.Image, mimeTypes);
					}
					else
					{
						string text = clipDescription ?? args.Data.Text;
						if (Uri.TryCreate(text, UriKind.Absolute, out _))
						{
							item = new ClipData.Item(AUri.Parse(text));
							mimeTypes.Add(ClipDescription.MimetypeTextUrilist);
						}
						else
						{
							item = new ClipData.Item(text);
							mimeTypes.Add(ClipDescription.MimetypeTextPlain);
						}
					}
				}

				var dataPackage = args.Data;
				ClipData.Item userItem = null;
				if (dataPackage.Image != null)
					userItem = ConvertToClipDataItem(dataPackage.Image, mimeTypes);

				if (dataPackage.Text != null)
					userItem = new ClipData.Item(dataPackage.Text);

				if (item == null)
				{
					item = userItem;
					userItem = null;
				}

				ClipData data = new ClipData(clipDescription, mimeTypes.ToArray(), item);

				if (userItem != null)
					data.AddItem(userItem);

				var dragShadowBuilder = new AView.DragShadowBuilder(v);

				if(Forms.IsNougatOrNewer)
					v.StartDragAndDrop(data, dragShadowBuilder, v, (int)ADragFlags.Global | (int)ADragFlags.GlobalUriRead);
				else
#pragma warning disable CS0618 // Type or member is obsolete
					v.StartDrag(data, dragShadowBuilder, v, (int)ADragFlags.Global | (int)ADragFlags.GlobalUriRead);
#pragma warning restore CS0618 // Type or member is obsolete
			});
		}

		ClipData.Item ConvertToClipDataItem(ImageSource source, List<string> mimeTypes)
		{
			if (source is UriImageSource uriImageSource)
			{
				if(!mimeTypes.Contains(ClipDescription.MimetypeTextUrilist))
					mimeTypes.Add(ClipDescription.MimetypeTextUrilist);

				var aUri = AUri.Parse(uriImageSource.Uri.ToString());
				return new ClipData.Item(aUri);
			}
			else if (source is FileImageSource fileImageSource && File.Exists(fileImageSource.File))
			{
				var aUri = AUri.FromFile(new Java.IO.File(fileImageSource.File));
				return new ClipData.Item(aUri);
			}

			return new ClipData.Item(source?.ToString() ?? String.Empty);

		}
	}
}