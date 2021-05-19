using System;
using Pango;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	public class NativeFontStyle : IFontStyle {

		private NativeFontFamily? _family;

		public NativeFontStyle(string family, double size, int weight, Pango.Stretch stretch, FontStyleType styleType) {
			FamilyName = family;
			Size = size;
			Weight = weight;
			StyleType = styleType;
			Stretch = stretch;
		}

		protected Stretch Stretch { get; }

		protected string FamilyName { get; }

		public IFontFamily FontFamily => _family ??= new NativeFontFamily(FamilyName);

		public string Id => $"{FamilyName} {Size:N0} {StyleType.ToPangoStyle()}";

		public double Size { get; }

		public string Name => Id;

		public string FullName => Id;

		public int Weight { get; }

		public FontStyleType StyleType { get; }

		public override bool Equals(object obj) {
			if (obj == null)
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			if (obj.GetType() != typeof(NativeFontStyle))
				return false;

			var other = (NativeFontStyle) obj;

			return Id == other.Id;
		}

		public override int GetHashCode() {
			return Id != null ? Id.GetHashCode() : 0;
		}

		public override string ToString() {
			return Name;
		}

		public int CompareTo(IFontStyle other) {
			if (Name.Equals("Regular") || Name.Equals("Plain") || Name.Equals("Normal")) {
				return -1;
			} else if (other.Name.Equals("Regular") || other.Name.Equals("Plain") || other.Name.Equals("Normal")) {
				return 1;
			}

			return string.Compare(Name, other.Name, StringComparison.Ordinal);
		}

		[GtkMissingImplementation]
		public System.IO.Stream OpenStream() {
			throw new NotImplementedException();
		}

	}

}
