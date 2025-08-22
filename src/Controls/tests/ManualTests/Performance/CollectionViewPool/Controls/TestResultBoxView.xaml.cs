using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace PoolMathApp.Xaml
{
	public partial class TestResultBoxView : Border
	{
		public TestResultBoxView()
		{
			InitializeComponent();
		}

		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create(nameof(Title), typeof(string), typeof(TestResultBoxView), string.Empty);

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set
			{
				SetValue(TitleProperty, value);
			}
		}

		public static readonly BindableProperty ValueProperty =
			BindableProperty.Create(nameof(Value), typeof(double?), typeof(TestResultBoxView), null);

		public double? Value
		{
			get { return (double?)GetValue(ValueProperty); }
			set
			{
				SetValue(ValueProperty, value);
			}
		}

		public static readonly BindableProperty ChemicalTypeProperty =
			BindableProperty.Create(nameof(ChemicalType), typeof(PoolMath.ChemicalLevelType), typeof(TestResultBoxView), PoolMath.ChemicalLevelType.None);

		public PoolMath.ChemicalLevelType ChemicalType
		{
			get => (PoolMath.ChemicalLevelType)GetValue(ChemicalTypeProperty);
			set => SetValue(ChemicalTypeProperty, value);
		}

		public static readonly BindableProperty ValueFormatProperty =
			BindableProperty.Create(nameof(ValueFormat), typeof(string), typeof(TestResultBoxView), null);

		public string ValueFormat
		{
			get { return (string)GetValue(ValueFormatProperty); }
			set
			{
				SetValue(ValueFormatProperty, value);
			}
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == TitleProperty.PropertyName)
			{
				labelTitle.Text = Title;
			}
			else if (propertyName == ValueProperty.PropertyName || propertyName == ValueFormatProperty.PropertyName)
			{
				if (Title.Equals("TEMP", StringComparison.Ordinal))
					labelValue.Text = Value.HasValue ? Value.Value.ToString(ValueFormat) + "°" : "--";
				else
					labelValue.Text = Value.HasValue ? Value.Value.ToString(ValueFormat) : "--";
			}
			else if (propertyName == ChemicalTypeProperty.PropertyName)
			{
				//SetDynamicResource(Border.StrokeProperty, Host.Theme.GetColorLighterKey(ChemicalType));
				//this.SetDynamicResource(RoundTheCornersBehavior.BgColorProperty, Host.Theme.GetColorLighterKey(ChemicalType));
				// SetDynamicResource(Border.BackgroundProperty, Host.Theme.GetColorLighterKey(ChemicalType));
				//labelTitle.SetDynamicResource(Label.TextColorProperty, Host.Theme.GetColorKey(ChemicalType));
			}
		}
	}
}
