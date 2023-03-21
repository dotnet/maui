#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="Type[@FullName='Microsoft.Maui.Controls.LayoutOptions']/Docs/*" />
	[System.ComponentModel.TypeConverter(typeof(LayoutOptionsConverter))]
	public struct LayoutOptions : IEquatable<LayoutOptions>
	{
		int _flags;

		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='Start']/Docs/*" />
		public static readonly LayoutOptions Start = new LayoutOptions(LayoutAlignment.Start, false);
		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='Center']/Docs/*" />
		public static readonly LayoutOptions Center = new LayoutOptions(LayoutAlignment.Center, false);
		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='End']/Docs/*" />
		public static readonly LayoutOptions End = new LayoutOptions(LayoutAlignment.End, false);
		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='Fill']/Docs/*" />
		public static readonly LayoutOptions Fill = new LayoutOptions(LayoutAlignment.Fill, false);

		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='StartAndExpand']/Docs/*" />
		[Obsolete("The StackLayout expansion options are deprecated; please use a Grid instead.")]
		public static readonly LayoutOptions StartAndExpand = new LayoutOptions(LayoutAlignment.Start, true);

		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='CenterAndExpand']/Docs/*" />
		[Obsolete("The StackLayout expansion options are deprecated; please use a Grid instead.")]
		public static readonly LayoutOptions CenterAndExpand = new LayoutOptions(LayoutAlignment.Center, true);

		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='EndAndExpand']/Docs/*" />
		[Obsolete("The StackLayout expansion options are deprecated; please use a Grid instead.")]
		public static readonly LayoutOptions EndAndExpand = new LayoutOptions(LayoutAlignment.End, true);

		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='FillAndExpand']/Docs/*" />
		[Obsolete("The StackLayout expansion options are deprecated; please use a Grid instead.")]
		public static readonly LayoutOptions FillAndExpand = new LayoutOptions(LayoutAlignment.Fill, true);

		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public LayoutOptions(LayoutAlignment alignment, bool expands)
		{
			var a = (int)alignment;
			if (a < 0 || a > 3)
				throw new ArgumentOutOfRangeException();
			_flags = (int)alignment | (expands ? (int)LayoutExpandFlag.Expand : 0);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='Alignment']/Docs/*" />
		public LayoutAlignment Alignment
		{
			get { return (LayoutAlignment)(_flags & 3); }
			set { _flags = (_flags & ~3) | (int)value; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/LayoutOptions.xml" path="//Member[@MemberName='Expands']/Docs/*" />
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