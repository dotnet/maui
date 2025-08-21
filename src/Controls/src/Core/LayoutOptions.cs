#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>A struct whose static members define various alignment and expansion options.</summary>
	[System.ComponentModel.TypeConverter(typeof(LayoutOptionsConverter))]
	public struct LayoutOptions : IEquatable<LayoutOptions>
	{
		int _flags;

		/// <summary>A <see cref="Microsoft.Maui.Controls.LayoutOptions"/> structure that describes an element that appears at the start of its parent and does not expand.</summary>
		public static readonly LayoutOptions Start = new LayoutOptions(LayoutAlignment.Start, false);
		/// <summary>A <see cref="Microsoft.Maui.Controls.LayoutOptions"/> structure that describes an element that is centered and does not expand.</summary>
		public static readonly LayoutOptions Center = new LayoutOptions(LayoutAlignment.Center, false);
		/// <summary>A <see cref="Microsoft.Maui.Controls.LayoutOptions"/> structure that describes an element that appears at the end of its parent and does not expand.</summary>
		public static readonly LayoutOptions End = new LayoutOptions(LayoutAlignment.End, false);
		/// <summary>A <see cref="Microsoft.Maui.Controls.LayoutOptions"/> stucture that describes an element that has no padding around itself and does not expand.</summary>
		public static readonly LayoutOptions Fill = new LayoutOptions(LayoutAlignment.Fill, false);

		/// <summary>A <see cref="Microsoft.Maui.Controls.LayoutOptions"/> structure that describes an element that appears at the start of its parent and expands.</summary>
		[Obsolete("The StackLayout expansion options are deprecated; please use a Grid instead.")]
		public static readonly LayoutOptions StartAndExpand = new LayoutOptions(LayoutAlignment.Start, true);

		/// <summary>A <see cref="Microsoft.Maui.Controls.LayoutOptions"/> structure that describes an element that is centered and expands.</summary>
		[Obsolete("The StackLayout expansion options are deprecated; please use a Grid instead.")]
		public static readonly LayoutOptions CenterAndExpand = new LayoutOptions(LayoutAlignment.Center, true);

		/// <summary>A <see cref="Microsoft.Maui.Controls.LayoutOptions"/> object that describes an element that appears at the end of its parent and expands.</summary>
		[Obsolete("The StackLayout expansion options are deprecated; please use a Grid instead.")]
		public static readonly LayoutOptions EndAndExpand = new LayoutOptions(LayoutAlignment.End, true);

		/// <summary>A <see cref="Microsoft.Maui.Controls.LayoutOptions"/> structure that describes an element that has no padding around itself and expands.</summary>
		[Obsolete("The StackLayout expansion options are deprecated; please use a Grid instead.")]
		public static readonly LayoutOptions FillAndExpand = new LayoutOptions(LayoutAlignment.Fill, true);

		/// <summary>Creates a new <see cref="Microsoft.Maui.Controls.LayoutOptions"/> object with <paramref name="alignment"/> and <paramref name="expands"/>.</summary>
		/// <param name="alignment">An alignment value.</param>
		/// <param name="expands">Whether or not an element will expand to fill available space in its parent.</param>
		public LayoutOptions(LayoutAlignment alignment, bool expands)
		{
			var a = (int)alignment;
			if (a < 0 || a > 3)
				throw new ArgumentOutOfRangeException();
			_flags = (int)alignment | (expands ? (int)LayoutExpandFlag.Expand : 0);
		}

		/// <summary>Gets or sets a value that indicates how an element will be aligned.</summary>
		public LayoutAlignment Alignment
		{
			get { return (LayoutAlignment)(_flags & 3); }
			set { _flags = (_flags & ~3) | (int)value; }
		}

		/// <summary>Gets or sets a value that indicates whether or not the element that is described by this <see cref="Microsoft.Maui.Controls.LayoutOptions"/> structure will occupy the largest space that its parent will give to it.</summary>
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

		public bool Equals(LayoutOptions other) => _flags == other._flags;

		public override bool Equals(object obj) => obj is LayoutOptions other && Equals(other);

		public override int GetHashCode() => _flags.GetHashCode();

		public static bool operator ==(LayoutOptions left, LayoutOptions right) => left.Equals(right);

		public static bool operator !=(LayoutOptions left, LayoutOptions right) => !(left == right);
	}
}