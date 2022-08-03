using System;
using System.Linq;
using ElmSharp;
using Tizen.Common;
using Tizen.UIExtensions.ElmSharp;
using EGestureType = ElmSharp.GestureLayer.GestureType;

namespace Microsoft.Maui.Controls.Platform
{
	public class DropGestureHandler : GestureHandler
	{
		DragDropExtensions.Interop.DragStateCallback _dragEnterCallback;
		DragDropExtensions.Interop.DragStateCallback _dragLeaveCallback;
		DragDropExtensions.Interop.DropCallback _dropCallback;

		public override EGestureType Type => default(EGestureType);

		public DropGestureHandler(IGestureRecognizer recognizer, IViewHandler handler) : base(recognizer)
		{
			_dragEnterCallback = OnEnterCallback;
			_dragLeaveCallback = OnLeaveCallback;
			_dropCallback = OnDropCallback;
			Handler = handler;
		}

		public IViewHandler Handler { get; }

		EvasObject PlatformView
		{
			get
			{
				var native = Handler.PlatformView as EvasObject;
				if (native is Canvas canvas)
				{
					var child = canvas.Children.LastOrDefault<EvasObject>();

					if (child != null)
					{
						if (child.PassEvents)
							child.PassEvents = false;

						return child;
					}
				}
				return native;
			}
		}


		public void AddDropGesture()
		{
			if (Handler == null)
				return;

			var target = DragDropExtensions.DragDropContentType.Targets;

			DragDropExtensions.AddDropTarget(PlatformView,
				target,
				_dragEnterCallback,
				_dragLeaveCallback, null,
				_dropCallback);
		}

		void OnEnterCallback(IntPtr data, IntPtr obj)
		{
			var currentStateData = DragGestureHandler.CurrentStateData;
			if (currentStateData == null)
				return;

			var arg = new DragEventArgs(currentStateData.DataPackage);

			if (Recognizer is DropGestureRecognizer dropRecognizer && dropRecognizer.AllowDrop)
				dropRecognizer.SendDragOver(arg);

			DragGestureHandler.CurrentStateData.AcceptedOperation = arg.AcceptedOperation;
		}

		void OnLeaveCallback(IntPtr data, IntPtr obj)
		{
			var currentStateData = DragGestureHandler.CurrentStateData;
			if (currentStateData == null)
				return;

			var arg = new DragEventArgs(currentStateData.DataPackage);

			if (Recognizer is DropGestureRecognizer dropRecognizer && dropRecognizer.AllowDrop)
				dropRecognizer.SendDragLeave(arg);

			DragGestureHandler.CurrentStateData.AcceptedOperation = arg.AcceptedOperation;
		}

		bool OnDropCallback(IntPtr data, IntPtr obj, IntPtr selectionData)
		{
			var currentStateData = DragGestureHandler.CurrentStateData;

			if (currentStateData.DataPackage == null || currentStateData.AcceptedOperation == DataPackageOperation.None)
				return false;

			Application.Current?.Dispatcher.Dispatch(async () =>
			{
				if (Recognizer is DropGestureRecognizer dropRecognizer && dropRecognizer.AllowDrop)
					await dropRecognizer.SendDrop(new DropEventArgs(currentStateData.DataPackage.View));
			});

			return true;
		}

		#region GestureHandler
		protected override void OnStarted(View sender, object data)
		{
		}

		protected override void OnMoved(View sender, object data)
		{
		}

		protected override void OnCompleted(View sender, object data)
		{
		}

		protected override void OnCanceled(View sender, object data)
		{
		}
		#endregion
	}
}
