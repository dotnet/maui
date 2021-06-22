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
				Calculate = (s, e, progress) => {
					var start = (int)s;
					var end = (int)e;
					return (int)((end - start) * progress) + start;
				}
			},
			[typeof(short)] = new Lerp
			{
				Calculate = (s, e, progress) => {
					var start = (short)s;
					var end = (short)e;
					return (short)((end - start) * progress) + start;
				}
			},
			[typeof(byte)] = new Lerp
			{
				Calculate = (s, e, progress) => {
					var start = (byte)s;
					var end = (byte)e;
					return (byte)((end - start) * progress) + start;
				}
			},
			[typeof(float)] = new Lerp
			{
				Calculate = (s, e, progress) => {
					var start = (float)(s ?? 0f);
					var end = (float)(e ?? 0f);
					return (float)((end - start) * progress) + start;
				}
			},
			[typeof(double)] = new Lerp
			{
				Calculate = (s, e, progress) => {
					var start = (double)s;
					var end = (double)e;
					return ((end - start) * progress) + start;
				}
			},
			[typeof(long)] = new Lerp
			{
				Calculate = (s, e, progress) => {
					var start = (int)s;
					var end = (int)e;
					return (long)((end - start) * progress) + start;
				}
			},
			[typeof(bool)] = new Lerp
			{
				Calculate = (s, e, progress) => ((bool)s).GenericLerp((bool)e, progress)
			},

			[typeof(uint)] = new Lerp
			{
				Calculate = (s, e, progress) => {
					var start = (uint)s;
					var end = (uint)e;
					return (uint)((end - start) * progress) + start;
				}
			},

			[typeof(Color)] = new Lerp
			{
				Calculate = (s, e, progress) => {
					var start = (Color)s;
					var end = (Color)e;
					return start.Lerp(end, progress);
				}
			},
			[typeof(RectangleF)] = new Lerp
			{
				Calculate = (s, e, progress) => {

					var start = (RectangleF)s;
					var end = (RectangleF)e;
					return start.Lerp(end, progress);
				}
			},
			
			[typeof(Thickness)] = new Lerp
			{
				Calculate = (s, e, progress) => {

					var start = (Thickness)(s ?? Thickness.Zero);
					var end = (Thickness)(e ?? Thickness.Zero);
					return start.Lerp(end, progress);
				}
			},
			[typeof(object)] = new Lerp
			{
				Calculate = (s, e, progress) => (s).GenericLerp(e, progress)
			},
		};

		public static Lerp GetLerp(Type type)
		{
			Lerp lerp;
			if (Lerps.TryGetValue(type, out lerp))
				return lerp;

			var types = new List<Type> { type };
			Type baseType = type.BaseType;
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
			return lerp;
		}

		public LerpDelegate? Calculate { get; set; }
	}

}
