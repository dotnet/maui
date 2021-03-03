using System;
using System.Threading;
using ElmSharp;
using Tizen.Common;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen.SkiaSharp;
using EGestureType = ElmSharp.GestureLayer.GestureType;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class DragGestureHandler : GestureHandler
	{
		bool _isApi4;

		DragDropExtensions.Interop.DragIconCreateCallback _iconCallback;
		DragDropExtensions.Interop.DragStateCallback _dragDoneCallback;

		static bool s_isDragging;
		static CustomDragStateData s_currentDragStateData;

		public DragGestureHandler(IGestureRecognizer recognizer, IVisualElementRenderer renderer) : base(recognizer)
		{
			_iconCallback = OnIconCallback;
			_dragDoneCallback = OnDragDoneCallback;
			_isApi4 = DotnetUtil.TizenAPIVersion <= 4;
			Renderer = renderer;
		}

		public override EGestureType Type => EGestureType.LongTap;

		public IVisualElementRenderer Renderer { get; }

		public static CustomDragStateData CurrentStateData
		{
			get
			{
				return s_currentDragStateData;
			}
		}

		EvasObject NativeView
		{
			get
			{
				var native = Renderer.NativeView;
				if (Renderer is SkiaSharp.ICanvasRenderer canvasRenderer)
				{
					native = canvasRenderer.RealNativeView;
				}
				return native;
			}
		}

		public void ResetCurrentStateData()
		{
			s_currentDragStateData = null;
		}

		protected override void OnStarted(View sender, object data)
		{
		}

		protected override void OnMoved(View sender, object data)
		{
			//Workaround to prevent an error occuring by multiple StartDrag calling in Tizen 6.5
			if (!s_isDragging)
			{
				ResetCurrentStateData();
				StartDrag();
			}
		}

		protected override void OnCompleted(View sender, object data)
		{
		}

		protected override void OnCanceled(View sender, object data)
		{
		}

		void StartDrag()
		{
			if (Recognizer is DragGestureRecognizer dragGestureRecognizer && dragGestureRecognizer.CanDrag)
			{
				if (Renderer == null)
					return;

				var arg = dragGestureRecognizer.SendDragStarting(Renderer.Element);

				if (arg.Cancel)
					return;

				s_currentDragStateData = new CustomDragStateData();
				s_currentDragStateData.DataPackage = arg.Data;

				var target = DragDropExtensions.DragDropContentType.Text;
				var strData = string.IsNullOrEmpty(arg.Data.Text) ? " " : arg.Data.Text;

				s_isDragging = true;

				DragDropExtensions.StartDrag(NativeView,
					target,
					strData,
					DragDropExtensions.DragDropActionType.Move,
					_iconCallback,
					null,
					null,
					_dragDoneCallback);
			}
		}

		IntPtr OnIconCallback(IntPtr data, IntPtr window, ref int xoff, ref int yoff)
		{
			EvasObject icon = null;
			EvasObject parent = new CustomWindow(NativeView, window);

			if (s_currentDragStateData.DataPackage.Image != null)
			{
				icon = GetImageIcon(parent);
			}
			else if (NativeView is ShapeView)
			{
				icon = GetShapeView(parent);
			}
			else
			{
				icon = GetDefaultIcon(parent);
			}
			var bound = NativeView.Geometry;
			bound.X = 0;
			bound.Y = 0;
			icon.Geometry = bound;

			if (icon is Native.Label)
			{
				icon.Resized += (s, e) =>
				{
					var map = new EvasMap(4);
					map.PopulatePoints(icon.Geometry, 0);
					map.Zoom(0.5, 0.5, 0, 0);
					icon.IsMapEnabled = true;
					icon.EvasMap = map;
				};
			}
			else
			{
				var map = new EvasMap(4);
				map.PopulatePoints(icon.Geometry, 0);
				map.Zoom(0.5, 0.5, 0, 0);
				icon.IsMapEnabled = true;
				icon.EvasMap = map;
			}


			return icon;
		}

		EvasObject GetDefaultIcon(EvasObject parent)
		{
			if (!string.IsNullOrEmpty(s_currentDragStateData.DataPackage.Text))
			{
				var label = new Native.Label(parent);
				label.Text = s_currentDragStateData.DataPackage.Text;

				if (Renderer.Element is Label lb)
					label.FontSize = lb.FontSize;
				else if (Renderer.Element is Entry et)
					label.FontSize = et.FontSize;
				else if (Renderer.Element is Editor ed)
					label.FontSize = ed.FontSize;

				return label;
			}
			else
			{
				var box = new ElmSharp.Rectangle(parent);
				box.Color = new ElmSharp.Color(128, 128, 128, 128);
				return box;
			}
		}

		EvasObject GetImageIcon(EvasObject parent)
		{
			var image = new Native.Image(parent);
			_ = image.LoadFromImageSourceAsync(s_currentDragStateData.DataPackage.Image);
			return image;
		}

		EvasObject GetShapeView(EvasObject parent)
		{
			var copiedImg = new EvasImage(parent);
			copiedImg.IsFilled = true;

			if (NativeView is ShapeView shapeView)
			{
				var canvas = shapeView.SKCanvasView;
				var realHandle = DragDropExtensions.Interop.elm_object_part_content_get(canvas, "elm.swallow.content");

				DragDropExtensions.Interop.evas_object_image_size_get(realHandle, out int w, out int h);
				DragDropExtensions.Interop.evas_object_image_size_set(copiedImg, w, h);

				var imgData = DragDropExtensions.Interop.evas_object_image_data_get(realHandle, false);
				DragDropExtensions.Interop.evas_object_image_data_set(copiedImg, imgData);
			}

			return copiedImg;
		}

		void OnDragDoneCallback(IntPtr data, IntPtr obj)
		{
			s_isDragging = false;
			if (Recognizer is DragGestureRecognizer dragGestureRecognizer && dragGestureRecognizer.CanDrag)
			{
				dragGestureRecognizer.SendDropCompleted(new DropCompletedEventArgs());
			}
		}

		public class CustomWindow : EvasObject
		{
			IntPtr _handle;

			public CustomWindow(EvasObject parent, IntPtr handle) : base()
			{
				_handle = handle;
				Realize(parent);
			}

			public CustomWindow(EvasObject handle) : base(handle)
			{
			}

			protected override IntPtr CreateHandle(EvasObject parent)
			{
				return _handle;
			}
		}

		public class CustomDragStateData
		{
			public DataPackage DataPackage { get; set; }
			public DataPackageOperation AcceptedOperation { get; set; } = DataPackageOperation.Copy;
		}
	}
}