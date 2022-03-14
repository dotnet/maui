using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DisplayInfo']/Docs" />
	[Preserve(AllMembers = true)]
	public readonly struct DisplayInfo : IEquatable<DisplayInfo>
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public DisplayInfo(double width, double height, double density, DisplayOrientation orientation, DisplayRotation rotation)
		{
			Width = width;
			Height = height;
			Density = density;
			Orientation = orientation;
			Rotation = rotation;
			RefreshRate = 0;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public DisplayInfo(double width, double height, double density, DisplayOrientation orientation, DisplayRotation rotation, float rate)
		{
			Width = width;
			Height = height;
			Density = density;
			Orientation = orientation;
			Rotation = rotation;
			RefreshRate = rate;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='Width']/Docs" />
		public double Width { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='Height']/Docs" />
		public double Height { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='Density']/Docs" />
		public double Density { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='Orientation']/Docs" />
		public DisplayOrientation Orientation { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='Rotation']/Docs" />
		public DisplayRotation Rotation { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='RefreshRate']/Docs" />
		public float RefreshRate { get; }

		public static bool operator ==(DisplayInfo left, DisplayInfo right) =>
			left.Equals(right);

		public static bool operator !=(DisplayInfo left, DisplayInfo right) =>
			!left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public override bool Equals(object obj) =>
			(obj is DisplayInfo metrics) && Equals(metrics);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='Equals'][2]/Docs" />
		public bool Equals(DisplayInfo other) =>
			Width.Equals(other.Width) &&
			Height.Equals(other.Height) &&
			Density.Equals(other.Density) &&
			Orientation.Equals(other.Orientation) &&
			Rotation.Equals(other.Rotation);

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			(Height, Width, Density, Orientation, Rotation).GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/DisplayInfo.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(Height)}: {Height}, {nameof(Width)}: {Width}, " +
			$"{nameof(Density)}: {Density}, {nameof(Orientation)}: {Orientation}, " +
			$"{nameof(Rotation)}: {Rotation}";
	}
}
