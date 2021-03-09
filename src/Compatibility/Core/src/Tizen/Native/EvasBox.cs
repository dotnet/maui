using System;
using System.Runtime.InteropServices;
using ElmSharp;
using EColor = ElmSharp.Color;
using ERectangle = ElmSharp.Rectangle;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	public class EvasBox : Container
	{
		Interop.CanvasBoxLayoutCallback _layoutCallback;
		Lazy<ERectangle> _rectangle;

		public event EventHandler<LayoutEventArgs> LayoutUpdated;

		public EvasBox(EvasObject parent) : base(parent)
		{
			_rectangle = new Lazy<ERectangle>(() =>
			{
				var rectangle = new ERectangle(this) { AlignmentX = -1, AlignmentY = -1, WeightX = 1, WeightY = 1 };
				Interop.evas_object_box_insert_at(Handle, rectangle, 0);
				rectangle.Lower();
				rectangle.Show();
				return rectangle;
			});

			SetLayoutCallback(() => { NotifyOnLayout(); });
		}

		public override EColor BackgroundColor
		{
			get
			{
				return _rectangle.Value.Color;
			}
			set
			{
				_rectangle.Value.Color = value.IsDefault ? EColor.Transparent : value;
			}
		}

		public void PackEnd(EvasObject content)
		{
			Interop.evas_object_box_append(Handle, content);
			AddChild(content);
		}

		public bool UnPack(EvasObject content)
		{
			var ret = Interop.evas_object_box_remove(Handle, content);
			if (ret)
				RemoveChild(content);
			return ret;
		}

		public bool UnPackAll()
		{
			var ret = Interop.evas_object_box_remove_all(Handle, true);
			if (ret)
			{
				ClearChildren();
				if (_rectangle.IsValueCreated)
				{
					Interop.evas_object_box_append(Handle, _rectangle.Value);
				}
			}
			return ret;
		}

		public void SetLayoutCallback(Action action)
		{
			_layoutCallback = (obj, priv, data) =>
			{
				if (_rectangle.IsValueCreated)
				{
					_rectangle.Value.Geometry = Geometry;
				}
				action();
			};
			Interop.evas_object_box_layout_set(Handle, _layoutCallback, IntPtr.Zero, null);
		}

		protected override IntPtr CreateHandle(EvasObject parent)
		{
			return Interop.evas_object_box_add(Interop.evas_object_evas_get(parent.Handle));
		}

		void NotifyOnLayout()
		{
			if (null != LayoutUpdated)
			{
				LayoutUpdated(this, new LayoutEventArgs() { Geometry = Geometry });
			}
		}

		class Interop
		{
			public const string LibEvas = "libevas.so.1";

			public delegate void CanvasBoxLayoutCallback(IntPtr obj, IntPtr priv, IntPtr userData);

			public delegate void CanvasBoxDataFreeCallback(IntPtr data);

			[DllImport(LibEvas)]
			internal static extern IntPtr evas_object_box_add(IntPtr evas);

			[DllImport(LibEvas)]
			internal static extern IntPtr evas_object_evas_get(IntPtr obj);

			[DllImport(LibEvas)]
			internal static extern void evas_object_box_append(IntPtr obj, IntPtr child);

			[DllImport(LibEvas)]
			internal static extern void evas_object_box_insert_at(IntPtr obj, IntPtr child, int pos);

			[DllImport(LibEvas)]
			[return: MarshalAs(UnmanagedType.U1)]
			internal static extern bool evas_object_box_remove(IntPtr obj, IntPtr child);

			[DllImport(LibEvas)]
			[return: MarshalAs(UnmanagedType.U1)]
			internal static extern bool evas_object_box_remove_all(IntPtr obj, bool clear);

			[DllImport(LibEvas)]
			internal static extern void evas_object_box_layout_set(IntPtr obj, CanvasBoxLayoutCallback cb, IntPtr data, CanvasBoxDataFreeCallback dataFreeCb);

			[DllImport(LibEvas)]
			internal static extern void evas_object_box_layout_set(IntPtr obj, CanvasBoxLayoutCallback cb, IntPtr data, IntPtr dataFreeCb);
		}
	}
}
