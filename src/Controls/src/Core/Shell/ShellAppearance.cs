#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Stores the appearance values for a Shell, including colors for background, foreground, tab bar, and title.</summary>
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

		static readonly BindableProperty[] s_ingestDoubleArray = new[]
		{
			Shell.FlyoutWidthProperty,
			Shell.FlyoutHeightProperty
		};

		Color[] _colorArray = new Color[s_ingestArray.Length];
		Brush[] _brushArray = new Brush[s_ingestBrushArray.Length];
		double[] _doubleArray = new double[s_ingestDoubleArray.Length];

		/// <summary>Gets the background color of the Shell.</summary>
		public Color BackgroundColor => _colorArray[0];

		/// <summary>Gets the color for disabled items in the Shell.</summary>
		public Color DisabledColor => _colorArray[1];

		/// <summary>Gets the foreground color of the Shell.</summary>
		public Color ForegroundColor => _colorArray[2];

		/// <summary>Gets the background color of the tab bar.</summary>
		public Color TabBarBackgroundColor => _colorArray[3];

		/// <summary>Gets the color for disabled items in the tab bar.</summary>
		public Color TabBarDisabledColor => _colorArray[4];

		/// <summary>Gets the foreground color of the tab bar.</summary>
		public Color TabBarForegroundColor => _colorArray[5];

		/// <summary>Gets the title color of the tab bar.</summary>
		public Color TabBarTitleColor => _colorArray[6];

		/// <summary>Gets the color for unselected items in the tab bar.</summary>
		public Color TabBarUnselectedColor => _colorArray[7];

		/// <summary>Gets the title color of the Shell.</summary>
		public Color TitleColor => _colorArray[8];

		/// <summary>Gets the color for unselected items in the Shell.</summary>
		public Color UnselectedColor => _colorArray[9];

		/// <summary>Gets the backdrop brush for the Shell flyout.</summary>
		public Brush FlyoutBackdrop => _brushArray[0];
		public double FlyoutWidth => _doubleArray[0];
		public double FlyoutHeight => _doubleArray[1];

		Color IShellAppearanceElement.EffectiveTabBarBackgroundColor =>
			TabBarBackgroundColor ?? BackgroundColor;

		Color IShellAppearanceElement.EffectiveTabBarDisabledColor =>
			TabBarDisabledColor ?? DisabledColor;

		Color IShellAppearanceElement.EffectiveTabBarForegroundColor =>
			TabBarForegroundColor ?? ForegroundColor;

		Color IShellAppearanceElement.EffectiveTabBarTitleColor =>
			TabBarTitleColor ?? TitleColor;

		Color IShellAppearanceElement.EffectiveTabBarUnselectedColor =>
			TabBarUnselectedColor ?? UnselectedColor;

		internal ShellAppearance()
		{
			for (int i = 0; i < _brushArray.Length; i++)
				_brushArray[i] = Brush.Default;

			for (int i = 0; i < _doubleArray.Length; i++)
				_doubleArray[i] = -1;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is ShellAppearance appearance))
				return false;

			for (int i = 0; i < _colorArray.Length; i++)
			{
				if (!EqualityComparer<Color>.Default.Equals(_colorArray[i], appearance._colorArray[i]))
					return false;
			}

			for (int i = 0; i < _brushArray.Length; i++)
			{
				if (!EqualityComparer<Brush>.Default.Equals(_brushArray[i], appearance._brushArray[i]))
					return false;
			}

			for (int i = 0; i < _doubleArray.Length; i++)
			{
				if (!EqualityComparer<double>.Default.Equals(_doubleArray[i], appearance._doubleArray[i]))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			var hashCode = -1988429770;
			for (int i = 0; i < _colorArray.Length; i++)
				hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(_colorArray[i]);

			for (int i = 0; i < _brushArray.Length; i++)
				hashCode = hashCode * -1521134295 + EqualityComparer<Brush>.Default.GetHashCode(_brushArray[i]);

			for (int i = 0; i < _doubleArray.Length; i++)
				hashCode = hashCode * -1521134295 + EqualityComparer<double>.Default.GetHashCode(_doubleArray[i]);

			return hashCode;
		}

		/// <summary>Ingests appearance values from the specified element into this instance.</summary>
		/// <param name="pivot">The element to read appearance values from.</param>
		/// <returns><see langword="true"/> if any values were ingested; otherwise, <see langword="false"/>.</returns>
		public bool Ingest(Element pivot)
		{
			bool anySet = false;

			var dataSet = pivot.GetValues<Color>(s_ingestArray);
			for (int i = 0; i < s_ingestArray.Length; i++)
			{
				if (_colorArray[i] == null && dataSet[i].IsSet)
				{
					anySet = true;
					_colorArray[i] = dataSet[i].Value;
				}
			}

			var brushDataSet = pivot.GetValues<Brush>(s_ingestBrushArray);
			for (int i = 0; i < s_ingestBrushArray.Length; i++)
			{
				if (Brush.IsNullOrEmpty(_brushArray[i]) && brushDataSet[i].IsSet)
				{
					anySet = true;
					_brushArray[i] = brushDataSet[i].Value;
				}
			}

			var doubleDataSet = pivot.GetValues<double>(s_ingestDoubleArray);
			for (int i = 0; i < s_ingestDoubleArray.Length; i++)
			{
				if (_doubleArray[i] == -1 && doubleDataSet[i].IsSet)
				{
					anySet = true;
					_doubleArray[i] = doubleDataSet[i].Value;
				}
			}

			return anySet;
		}

		/// <summary>Marks the appearance as complete by filling any remaining null values.</summary>
		public void MakeComplete()
		{
			for (int i = 0; i < s_ingestArray.Length; i++)
			{
				if (_colorArray[i] == null)
					_colorArray[i] = null;
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
