using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Views;
using ADragFlags = Android.Views.DragFlags;
using AUri = Android.Net.Uri;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class DragAndDropGestureHandler : Java.Lang.Object, AView.IOnDragListener
	{
		bool _isDisposed;
		CustomLocalStateData _currentCustomLocalStateData;

		public DragAndDropGestureHandler(Func<View> getView, Func<AView> getControl)
		{
			_ = getView ?? throw new ArgumentNullException(nameof(getView));
			_ = getControl ?? throw new ArgumentNullException(nameof(getControl));

			GetView = getView;
			GetControl = getControl;
			SetupHandlerForDrop();
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
				_currentCustomLocalStateData = null;
				var control = GetControl();
				if (control.IsAlive())
				{
					control.SetOnDragListener(null);
				}
			}

			base.Dispose(disposing);

		}

		void SendEventArgs<TRecognizer>(Action<TRecognizer> func, View view = null)
		{
			view = view ?? GetView();

			if (view == null)
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
			CustomLocalStateData localStateData = (e.LocalState as CustomLocalStateData) ?? _currentCustomLocalStateData ?? new CustomLocalStateData();
			_currentCustomLocalStateData = localStateData;
			IVisualElementRenderer dragSourceRenderer = localStateData?.SourceNativeView as IVisualElementRenderer;
			package = localStateData?.DataPackage;
			var dragSourceElement = _currentCustomLocalStateData?.SourceElement ?? dragSourceRenderer?.Element;

			if (package == null)
			{
				package = new DataPackage();
				_currentCustomLocalStateData.DataPackage = package;
			}

			switch (e.Action)
			{
				case DragAction.Ended:
					{
						_currentCustomLocalStateData = null;
						if (dragSourceElement is View vSource)
						{
							HandleDropCompleted(vSource);
						}
					}
					break;
				case DragAction.Started:
					break;
				case DragAction.Location:
					HandleDragOver(package);
					break;
				case DragAction.Drop:
					{
						HandleDrop(e, _currentCustomLocalStateData);
						break;
					}
				case DragAction.Entered:
					HandleDragOver(package);
					break;
				case DragAction.Exited:
					HandleDragLeave(package);
					break;
			}

			return true;
		}

		void HandleDropCompleted(View element)
		{
			var args = new DropCompletedEventArgs();
			SendEventArgs<DragGestureRecognizer>(rec => rec.SendDropCompleted(args), element);
		}

		bool HandleDragLeave(DataPackage package)
		{
			var dragEventArgs = new DragEventArgs(package);
			bool validTarget = false;
			SendEventArgs<DropGestureRecognizer>(rec =>
			{
				if (!rec.AllowDrop)
					return;

				rec.SendDragLeave(dragEventArgs);
				validTarget = validTarget || dragEventArgs.AcceptedOperation != DataPackageOperation.None;
				_currentCustomLocalStateData.AcceptedOperation = dragEventArgs.AcceptedOperation;
			});

			return validTarget;
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
				_currentCustomLocalStateData.AcceptedOperation = dragEventArgs.AcceptedOperation;
			});

			return validTarget;
		}

		void HandleDrop(DragEvent e, CustomLocalStateData customLocalStateData)
		{
			if (customLocalStateData.AcceptedOperation == DataPackageOperation.None)
				return;

			var datapackage = customLocalStateData.DataPackage;
			if (e.LocalState == null)
			{
				string text = String.Empty;
				if (e.ClipData?.ItemCount > 0)
				{
					var clipData = e.ClipData.GetItemAt(0);
					var control = GetControl();

					if (control?.Context != null)
						text = clipData.CoerceToText(control?.Context);
					else
						text = clipData.Text;
				}
				else
				{
					text = e.ClipDescription?.Label;
				}

				if (String.IsNullOrWhiteSpace(datapackage.Text))
					datapackage.Text = text;

				if (datapackage.Image == null)
					datapackage.Image = text;
			}

			var args = new DropEventArgs(datapackage?.View);
			SendEventArgs<DropGestureRecognizer>(async rec =>
			{
				if (!rec.AllowDrop)
					return;

				try
				{
					await rec.SendDrop(args);
				}
				catch (Exception exc)
				{
					Internals.Log.Warning(nameof(DropGestureRecognizer), $"{exc}");
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

				CustomLocalStateData customLocalStateData = new CustomLocalStateData();
				customLocalStateData.DataPackage = args.Data;

				//_dragSource[element] = args.Data;
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

				customLocalStateData.SourceNativeView = v;
				customLocalStateData.SourceElement = renderer?.Element;

				if (Forms.IsNougatOrNewer)
					v.StartDragAndDrop(data, dragShadowBuilder, customLocalStateData, (int)ADragFlags.Global | (int)ADragFlags.GlobalUriRead);
				else
#pragma warning disable CS0618 // Type or member is obsolete
					v.StartDrag(data, dragShadowBuilder, customLocalStateData, (int)ADragFlags.Global | (int)ADragFlags.GlobalUriRead);
#pragma warning restore CS0618 // Type or member is obsolete
			});
		}

		ClipData.Item ConvertToClipDataItem(ImageSource source, List<string> mimeTypes)
		{
			if (source is UriImageSource uriImageSource)
			{
				if (!mimeTypes.Contains(ClipDescription.MimetypeTextUrilist))
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

		class CustomLocalStateData : Java.Lang.Object
		{
			public AView SourceNativeView { get; set; }
			public DataPackage DataPackage { get; set; }
			public DataPackageOperation AcceptedOperation { get; set; } = DataPackageOperation.Copy;
			public VisualElement SourceElement { get; set; }
		}
	}
}