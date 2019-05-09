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

		Color?[] _colorArray = new Color?[s_ingestArray.Length];

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

		}

		public override bool Equals(object obj)
		{
			var appearance = obj as ShellAppearance;
			return appearance != null &&
				   EqualityComparer<Color>.Default.Equals(BackgroundColor, appearance.BackgroundColor) &&
				   EqualityComparer<Color>.Default.Equals(DisabledColor, appearance.DisabledColor) &&
				   EqualityComparer<Color>.Default.Equals(ForegroundColor, appearance.ForegroundColor) &&
				   EqualityComparer<Color>.Default.Equals(TabBarBackgroundColor, appearance.TabBarBackgroundColor) &&
				   EqualityComparer<Color>.Default.Equals(TabBarDisabledColor, appearance.TabBarDisabledColor) &&
				   EqualityComparer<Color>.Default.Equals(TabBarForegroundColor, appearance.TabBarForegroundColor) &&
				   EqualityComparer<Color>.Default.Equals(TabBarTitleColor, appearance.TabBarTitleColor) &&
				   EqualityComparer<Color>.Default.Equals(TabBarUnselectedColor, appearance.TabBarUnselectedColor) &&
				   EqualityComparer<Color>.Default.Equals(TitleColor, appearance.TitleColor) &&
				   EqualityComparer<Color>.Default.Equals(UnselectedColor, appearance.UnselectedColor);
		}

		public override int GetHashCode()
		{
			var hashCode = -1988429770;
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(BackgroundColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(DisabledColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(ForegroundColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(TabBarBackgroundColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(TabBarDisabledColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(TabBarForegroundColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(TabBarTitleColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(TabBarUnselectedColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(TitleColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(UnselectedColor);
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