using System.Collections.Generic;
using System.ComponentModel;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ShellAppearance : IShellAppearanceElement
	{
		static readonly BindableProperty[] s_ingestArray = new[]
		{
			Shell.BackgroundColorProperty,
			Shell.DisabledColorProperty,
			Shell.ForegroundColorProperty,
			Shell.TabBarBackgroundColorProperty,
			Shell.TabBarDisabledColorProperty,
			Shell.TabBarForegroundColorProperty,
			Shell.TabBarTitleColorProperty,
			Shell.TabBarUnselectedColorProperty,
			Shell.TitleColorProperty,
			Shell.UnselectedColorProperty
		};

		static readonly BindableProperty[] s_ingestBrushArray = new[]
		{
			Shell.FlyoutBackdropProperty
		};

		Color?[] _colorArray = new Color?[s_ingestArray.Length];
		Brush[] _brushArray = new Brush[s_ingestBrushArray.Length];

		public Color BackgroundColor => _colorArray[0].Value;

		public Color DisabledColor => _colorArray[1].Value;

		public Color ForegroundColor => _colorArray[2].Value;

		public Color TabBarBackgroundColor => _colorArray[3].Value;

		public Color TabBarDisabledColor => _colorArray[4].Value;

		public Color TabBarForegroundColor => _colorArray[5].Value;

		public Color TabBarTitleColor => _colorArray[6].Value;

		public Color TabBarUnselectedColor => _colorArray[7].Value;

		public Color TitleColor => _colorArray[8].Value;

		public Color UnselectedColor => _colorArray[9].Value;

		public Brush FlyoutBackdrop => _brushArray[0];

		Color IShellAppearanceElement.EffectiveTabBarBackgroundColor =>
			!TabBarBackgroundColor.IsDefault ? TabBarBackgroundColor : BackgroundColor;

		Color IShellAppearanceElement.EffectiveTabBarDisabledColor =>
			!TabBarDisabledColor.IsDefault ? TabBarDisabledColor : DisabledColor;

		Color IShellAppearanceElement.EffectiveTabBarForegroundColor =>
			!TabBarForegroundColor.IsDefault ? TabBarForegroundColor : ForegroundColor;

		Color IShellAppearanceElement.EffectiveTabBarTitleColor =>
			!TabBarTitleColor.IsDefault ? TabBarTitleColor : TitleColor;

		Color IShellAppearanceElement.EffectiveTabBarUnselectedColor =>
			!TabBarUnselectedColor.IsDefault ? TabBarUnselectedColor : UnselectedColor;

		internal ShellAppearance()
		{
			for (int i = 0; i < _brushArray.Length; i++)
				_brushArray[0] = Brush.Default;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ShellAppearance appearance))
				return false;

			for (int i = 0; i < _colorArray.Length; i++)
			{
				if (!EqualityComparer<Color>.Default.Equals(_colorArray[i].Value, appearance._colorArray[i].Value))
					return false;
			}

			for (int i = 0; i < _brushArray.Length; i++)
			{
				if (!EqualityComparer<Brush>.Default.Equals(_brushArray[i], appearance._brushArray[i]))
					return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			var hashCode = -1988429770;
			for (int i = 0; i < _colorArray.Length; i++)
				hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(_colorArray[i].Value);

			for (int i = 0; i < _brushArray.Length; i++)
				hashCode = hashCode * -1521134295 + EqualityComparer<Brush>.Default.GetHashCode(_brushArray[i]);

			return hashCode;
		}

		public bool Ingest(Element pivot)
		{
			bool anySet = false;

			var dataSet = pivot.GetValues<Color>(s_ingestArray);
			for (int i = 0; i < s_ingestArray.Length; i++)
			{
				if (!_colorArray[i].HasValue && dataSet[i].IsSet)
				{
					anySet = true;
					_colorArray[i] = dataSet[i].Value;
				}
			}

			var brushDataSet = pivot.GetValues<Brush>(s_ingestBrushArray);
			for (int i = 0; i < s_ingestBrushArray.Length; i++)
			{
				if (_brushArray[i] != Brush.Default && brushDataSet[i].IsSet)
				{
					anySet = true;
					_brushArray[i] = brushDataSet[i].Value;
				}
			}

			return anySet;
		}

		public void MakeComplete()
		{
			for (int i = 0; i < s_ingestArray.Length; i++)
			{
				if (_colorArray[i] == null)
					_colorArray[i] = Color.Default;
			}
		}

		public static bool operator ==(ShellAppearance appearance1, ShellAppearance appearance2)
		{
			return EqualityComparer<ShellAppearance>.Default.Equals(appearance1, appearance2);
		}

		public static bool operator !=(ShellAppearance appearance1, ShellAppearance appearance2)
		{
			return !(appearance1 == appearance2);
		}
	}
}