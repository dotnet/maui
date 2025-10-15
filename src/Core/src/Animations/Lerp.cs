using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
namespace Microsoft.Maui.Animations
{
	public class Lerp
	{
		public delegate object LerpDelegate(object start, object end, double progress);
		public static readonly Dictionary<Type, Lerp> Lerps = new Dictionary<Type, Lerp>()
		{
			[typeof(int)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{
					var start = Convert.ToInt32(s);
					var end = Convert.ToInt32(e);
					return (int)((end - start) * progress) + start;
				}
			},
			[typeof(short)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{
					var start = Convert.ToInt16(s);
					var end = Convert.ToInt16(e);
					return (short)((end - start) * progress) + start;
				}
			},
			[typeof(byte)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{
					var start = Convert.ToByte(s);
					var end = Convert.ToByte(e);
					return (byte)((end - start) * progress) + start;
				}
			},
			[typeof(float)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{
					var start = (float)(s ?? 0f);
					var end = (float)(e ?? 0f);
					return (float)((end - start) * progress) + start;
				}
			},
			[typeof(double)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{
					var start = Convert.ToDouble(s);
					var end = Convert.ToDouble(e);
					return ((end - start) * progress) + start;
				}
			},
			[typeof(long)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{
					var start = Convert.ToInt64(s);
					var end = Convert.ToInt64(e);
					return (long)((end - start) * progress) + start;
				}
			},
			[typeof(bool)] = new Lerp
			{
				Calculate = (s, e, progress) => ((bool)s).GenericLerp((bool)e, progress)
			},

			[typeof(uint)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{
					var start = Convert.ToUInt32(s);
					var end = Convert.ToUInt32(e);
					return (uint)((end - start) * progress) + start;
				}
			},

			[typeof(Color)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{
					var start = (Color)s;
					var end = (Color)e;
					return start.Lerp(end, progress);
				}
			},
			[typeof(RectF)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{

					var start = (RectF)s;
					var end = (RectF)e;
					return start.Lerp(end, progress);
				}
			},
			[typeof(Rect)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{

					var start = (Rect)s;
					var end = (Rect)e;
					return start.Lerp(end, progress);
				}
			},
			[typeof(PointF)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{

					var start = (PointF)s;
					var end = (PointF)e;
					return start.Lerp(end, progress);
				}
			},
			[typeof(Point)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{

					var start = (Point)s;
					var end = (Point)e;
					return start.Lerp(end, progress);
				}
			},
			[typeof(SizeF)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{

					var start = (SizeF)s;
					var end = (SizeF)e;
					return start.Lerp(end, progress);
				}
			},
			[typeof(Size)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{

					var start = (Size)s;
					var end = (Size)e;
					return start.Lerp(end, progress);
				}
			},
			[typeof(Thickness)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{

					var start = (Thickness)(s ?? Thickness.Zero);
					var end = (Thickness)(e ?? Thickness.Zero);
					return start.Lerp(end, progress);
				}
			},
			[typeof(SolidPaint)] = new Lerp
			{
				Calculate = (s, e, progress) =>
				{
					var start = (SolidPaint)s;
					var end = (SolidPaint)e;
					return start.Lerp(end, progress);
				}
			},
			[typeof(object)] = new Lerp
			{
				Calculate = (s, e, progress) => (s).GenericLerp(e, progress)
			},
		};

		public static Lerp? GetLerp(Type type)
		{
			if (type == null)
				return null;

			Lerp? lerp;
			if (Lerps.TryGetValue(type, out lerp))
				return lerp;

			var types = new List<Type> { type };
			Type? baseType = type?.BaseType;
			while (baseType != null)
			{
				types.Add(baseType);
				baseType = baseType.BaseType;
			}

			foreach (var t in types)
			{
				if (Lerps.TryGetValue(t, out lerp))
					return lerp;
			}

			// For interface types or when no specific lerp is found, fall back to object lerp
			if (lerp == null)
			{
				Lerps.TryGetValue(typeof(object), out lerp);
			}

			return lerp;
		}

		public LerpDelegate? Calculate { get; set; }
	}

}
