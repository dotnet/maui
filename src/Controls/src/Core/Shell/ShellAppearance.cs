using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="Type[@FullName='Microsoft.Maui.Controls.ShellAppearance']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='BackgroundColor']/Docs/*" />
		public Color BackgroundColor => _colorArray[0];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='DisabledColor']/Docs/*" />
		public Color DisabledColor => _colorArray[1];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='ForegroundColor']/Docs/*" />
		public Color ForegroundColor => _colorArray[2];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='TabBarBackgroundColor']/Docs/*" />
		public Color TabBarBackgroundColor => _colorArray[3];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='TabBarDisabledColor']/Docs/*" />
		public Color TabBarDisabledColor => _colorArray[4];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='TabBarForegroundColor']/Docs/*" />
		public Color TabBarForegroundColor => _colorArray[5];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='TabBarTitleColor']/Docs/*" />
		public Color TabBarTitleColor => _colorArray[6];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='TabBarUnselectedColor']/Docs/*" />
		public Color TabBarUnselectedColor => _colorArray[7];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='TitleColor']/Docs/*" />
		public Color TitleColor => _colorArray[8];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='UnselectedColor']/Docs/*" />
		public Color UnselectedColor => _colorArray[9];

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='FlyoutBackdrop']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='Equals']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='Ingest']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellAppearance.xml" path="//Member[@MemberName='MakeComplete']/Docs/*" />
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
