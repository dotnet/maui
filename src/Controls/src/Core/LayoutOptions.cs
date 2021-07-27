using System;

namespace Microsoft.Maui.Controls
{
	[System.ComponentModel.TypeConverter(typeof(LayoutOptionsConverter))]
	public struct LayoutOptions
	{
		int _flags;

		public static readonly LayoutOptions Start = new LayoutOptions(LayoutAlignment.Start, false);
		public static readonly LayoutOptions Center = new LayoutOptions(LayoutAlignment.Center, false);
		public static readonly LayoutOptions End = new LayoutOptions(LayoutAlignment.End, false);
		public static readonly LayoutOptions Fill = new LayoutOptions(LayoutAlignment.Fill, false);
		public static readonly LayoutOptions StartAndExpand = new LayoutOptions(LayoutAlignment.Start, true);
		public static readonly LayoutOptions CenterAndExpand = new LayoutOptions(LayoutAlignment.Center, true);
		public static readonly LayoutOptions EndAndExpand = new LayoutOptions(LayoutAlignment.End, true);
		public static readonly LayoutOptions FillAndExpand = new LayoutOptions(LayoutAlignment.Fill, true);

		public LayoutOptions(LayoutAlignment alignment, bool expands)
		{
			var a = (int)alignment;
			if (a < 0 || a > 3)
				throw new ArgumentOutOfRangeException();
			_flags = (int)alignment | (expands ? (int)LayoutExpandFlag.Expand : 0);
		}

		public LayoutAlignment Alignment
		{
			get { return (LayoutAlignment)(_flags & 3); }
			set { _flags = (_flags & ~3) | (int)value; }
		}

		public bool Expands
		{
			get { return (_flags & (int)LayoutExpandFlag.Expand) != 0; }
			set { _flags = (_flags & 3) | (value ? (int)LayoutExpandFlag.Expand : 0); }
		}

		internal Primitives.LayoutAlignment ToCore()
		{
			switch (Alignment)
			{
				case LayoutAlignment.Start:
					return Primitives.LayoutAlignment.Start;
				case LayoutAlignment.Center:
					return Primitives.LayoutAlignment.Center;
				case LayoutAlignment.End:
					return Primitives.LayoutAlignment.End;
				case LayoutAlignment.Fill:
					return Primitives.LayoutAlignment.Fill;
			}

			return Primitives.LayoutAlignment.Start;
		}
	}
}