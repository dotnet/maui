using System;
using System.Threading.Tasks;
using ElmSharp;
using Microsoft.Maui.Controls.Internals;
using Tizen.UIExtensions.ElmSharp;
using EGestureType = ElmSharp.GestureLayer.GestureType;
using TImage = Tizen.UIExtensions.ElmSharp.Image;
using TLabel = Tizen.UIExtensions.ElmSharp.Label;

namespace Microsoft.Maui.Controls.Platform
{
	public class DragGestureHandler : GestureHandler
	{
		DragDropExtensions.Interop.DragIconCreateCallback _iconCallback;
		DragDropExtensions.Interop.DragStateCallback _dragDoneCallback;

		static bool s_isDragging;
		static CustomDragStateData s_currentDragStateData;

		protected virtual IView Element => Handler?.VirtualView as IView;

		public DragGestureHandler(IGestureRecognizer recognizer, IViewHandler handler) : base(recognizer)
		{
			_iconCallback = OnIconCallback;
			_dragDoneCallback = OnDragDoneCallback;
			Handler = handler;
		}

		public override EGestureType Type => EGestureType.LongTap;

		public IViewHandler Handler { get; }

		public static CustomDragStateData CurrentStateData
		{
			get
			{
				return s_currentDragStateData;
			}
		}

		EvasObject PlatformView
		{
			get
			{
				return Handler.PlatformView as EvasObject;
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
				if (Handler == null)
					return;

				var arg = dragGestureRecognizer.SendDragStarting(Element);

				if (arg.Cancel)
					return;

				s_currentDragStateData = new CustomDragStateData();
				s_currentDragStateData.DataPackage = arg.Data;

				var target = DragDropExtensions.DragDropContentType.Text;
				var strData = string.IsNullOrEmpty(arg.Data.Text) ? " " : arg.Data.Text;

				s_isDragging = true;

				DragDropExtensions.StartDrag(PlatformView,
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
			EvasObject parent = new CustomWindow(PlatformView, window);

			if (s_currentDragStateData.DataPackage.Image != null)
			{
				icon = GetImageIconAsync(parent).Result;
			}
			else if (PlatformView is ShapeView)
			{
				icon = GetShapeView(parent);
			}
			else
			{
				icon = GetDefaultIcon(parent);
			}
			var bound = PlatformView.Geometry;
			bound.X = 0;
			bound.Y = 0;
			icon.Geometry = bound;

			if (icon is TLabel)
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
				var label = new TLabel(parent);
				label.Text = s_currentDragStateData.DataPackage.Text;

				if (Element is IFontElement fe)
					label.FontSize = fe.FontSize;

				return label;
			}
			else
			{
				var box = new ElmSharp.Rectangle(parent);
				box.Color = new ElmSharp.Color(128, 128, 128, 128);
				return box;
			}
		}

		async Task<EvasObject> GetImageIconAsync(EvasObject parent)
		{
			var image = new TImage(parent);
			var mImage = s_currentDragStateData.DataPackage.Image;
			var services = Handler.MauiContext?.Services;
			var provider = services.GetService(typeof(IImageSourceServiceProvider)) as IImageSourceServiceProvider;
			var service = provider?.GetImageSourceService(mImage);
			var result = await service.GetImageAsync(mImage, image);
			if (result == null)
				return null;
			return image;
		}

		EvasObject GetShapeView(EvasObject parent)
		{
			var copiedImg = new EvasImage(parent)
			{
				IsFilled = true
			};

			if (PlatformView is ShapeView shapeView)
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
