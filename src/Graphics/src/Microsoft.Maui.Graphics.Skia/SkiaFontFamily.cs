using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Skia
{
	public class SkiaFontFamily : IFontFamily, IComparable<IFontFamily>, IComparable
	{
		private readonly string _name;
		private IFontStyle[] _fontStyles;

		public SkiaFontFamily(string name)
		{
			_name = name;
		}

		public string Name => _name;

		public IFontStyle[] GetFontStyles()
		{
			return _fontStyles ?? (_fontStyles = InitializeFontStyles());
		}

		private IFontStyle[] InitializeFontStyles()
		{
			var styles = new List<IFontStyle>();

			styles.Add(
				new SkiaFontStyle(
					this,
					_name,
					_name,
					_name,
					FontUtils.Normal,
					FontStyleType.Normal));

			styles.Sort();
			return styles.ToArray();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != typeof(SkiaFontFamily))
				return false;
			SkiaFontFamily other = (SkiaFontFamily) obj;
			return _name == other._name;
		}

		public override int GetHashCode()
		{
			return (_name != null ? _name.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return Name;
		}

		public int CompareTo(IFontFamily other)
		{
			return string.Compare(_name, other.Name, StringComparison.Ordinal);
		}

		public int CompareTo(object obj)
		{
			if (obj is IFontFamily other)
				return CompareTo(other);

			return -1;
		}
	}
}
