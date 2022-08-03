using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static class TransformationExtensions
	{
		static Dictionary<EvasObject, Action> s_movedHandlers = new Dictionary<EvasObject, Action>();

		public static void UpdateTransformation(this EvasObject platformView, IView? view)
		{
			if (view == null)
				return;

			// prepare the EFL effect structure
			Rect geometry = platformView.Geometry;
			EvasMap map = new EvasMap(4);
			map.PopulatePoints(geometry, 0);

			bool changed = false;
			view.ApplyScale(map, geometry, ref changed);
			view.ApplyRotation(platformView, map, geometry, ref changed);
			view.ApplyTranslation(map, geometry, ref changed);

			platformView.IsMapEnabled = changed;

			if (changed)
			{
				platformView.EvasMap = map;
				if (!s_movedHandlers.ContainsKey(platformView))
				{
					// not registered moved handler
					s_movedHandlers[platformView] = () => platformView.UpdateTransformation(view);
					platformView.Moved += OnMoved;
				}
			}
			else
			{
				if (s_movedHandlers.ContainsKey(platformView))
				{
					// need to unregister moved handler
					platformView.Moved -= OnMoved;
					s_movedHandlers.Remove(platformView);
				}
			}
		}

		static void OnMoved(object? sender, EventArgs e)
		{
			if (sender is EvasObject platformView)
			{
				s_movedHandlers[platformView].Invoke();
			}
		}

		internal static void ApplyScale(this IView view, EvasMap map, Rect geometry, ref bool changed)
		{
			var scale = view.Scale;
			var scaleX = view.ScaleX * scale;
			var scaleY = view.ScaleY * scale;

			// apply scale factor
			if (scaleX != 1.0 || scaleY != 1.0)
			{
				map.Zoom(scaleX, scaleY,
					geometry.X + (int)(geometry.Width * view.AnchorX),
					geometry.Y + (int)(geometry.Height * view.AnchorY));
				changed = true;
			}
		}

		internal static void ApplyRotation(this IView view, EvasObject platformView, EvasMap map, Rect geometry, ref bool changed)
		{
			var rotationX = view.RotationX;
			var rotationY = view.RotationY;
			var rotationZ = view.Rotation;
			var anchorX = view.AnchorX;
			var anchorY = view.AnchorY;

			// apply rotations
			if (rotationX != 0 || rotationY != 0 || rotationZ != 0)
			{
				map.Rotate3D(rotationX, rotationY, rotationZ, (int)(geometry.X + geometry.Width * anchorX),
															  (int)(geometry.Y + geometry.Height * anchorY), 0);
				// the last argument is focal length, it determine the strength of distortion. We compared it with the Android implementation
				map.Perspective3D(geometry.X + geometry.Width / 2, geometry.Y + geometry.Height / 2, 0, (int)(1.3 * Math.Max(geometry.Height, geometry.Width)));
				// Need to unset clip because perspective 3d rotation is going beyond the container bound
				platformView.SetClip(null);
				changed = true;
			}
		}

		internal static void ApplyTranslation(this IView view, EvasMap map, Rect geometry, ref bool changed)
		{
			var shiftX = view.TranslationX.ToScaledPixel();
			var shiftY = view.TranslationY.ToScaledPixel();

			// apply translation, i.e. move/shift the object a little
			if (shiftX != 0 || shiftY != 0)
			{
				if (changed)
				{
					// special care is taken to apply the translation last
					Point3D p;
					for (int i = 0; i < 4; i++)
					{
						p = map.GetPointCoordinate(i);
						p.X += shiftX;
						p.Y += shiftY;
						map.SetPointCoordinate(i, p);
					}
				}
				else
				{
					// in case when we only need translation, then construct the map in a simpler way
					geometry.X += shiftX;
					geometry.Y += shiftY;
					map.PopulatePoints(geometry, 0);

					changed = true;
				}
			}
		}

		public static void Perspective3D(this EvasMap map, int px, int py, int z0, int foc)
		{
			var mapType = typeof(EvasMap);
			var propInfo = mapType.GetProperty("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			IntPtr? handle = (IntPtr?)propInfo?.GetValue(map);
			if (handle != null)
				evas_map_util_3d_perspective(handle.Value, px, py, z0, foc);
		}

		[DllImport("libevas.so.1")]
		static extern void evas_map_util_3d_perspective(IntPtr map, int px, int py, int z0, int foc);
	}
}
