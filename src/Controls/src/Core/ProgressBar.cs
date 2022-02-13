using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="Type[@FullName='Microsoft.Maui.Controls.ProgressBar']/Docs" />
	public partial class ProgressBar : View, IElementConfiguration<ProgressBar>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='ProgressColorProperty']/Docs" />
		public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(ProgressBar), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='ProgressProperty']/Docs" />
		public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(double), typeof(ProgressBar), 0d, coerceValue: (bo, v) => ((double)v).Clamp(0, 1), propertyChanged: ProgressPropChanged);

		readonly Lazy<PlatformConfigurationRegistry<ProgressBar>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ProgressBar()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ProgressBar>>(() => new PlatformConfigurationRegistry<ProgressBar>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='ProgressColor']/Docs" />
		public Color ProgressColor
		{
			get { return (Color)GetValue(ProgressColorProperty); }
			set { SetValue(ProgressColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='Progress']/Docs" />
		public double Progress
		{
			get { return (double)GetValue(ProgressProperty); }
			set { 
				SetValue(ProgressProperty, value);
				Value = Progress * (Maximum - Minimum) + Minimum; // UWP compatibity
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='ProgressTo']/Docs" />
		public Task<bool> ProgressTo(double value, uint length, Easing easing)
		{
			var tcs = new TaskCompletionSource<bool>();

			this.Animate("Progress", d => Progress = d, Progress, value, length: length, easing: easing, finished: (d, finished) => tcs.SetResult(finished));

			return tcs.Task;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='On']/Docs" />
		public IPlatformElementConfiguration<T, ProgressBar> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
		#region "UWP compatibility"

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='ValueProperty']/Docs" />
		public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(ProgressBar), 0.0, coerceValue: CoerceValueProp);

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='MinimumProperty']/Docs" />
		public static readonly BindableProperty MinimumProperty = BindableProperty.Create(nameof(Minimum), typeof(double), typeof(ProgressBar), 0.0, validateValue: IsValidMinProp, propertyChanged: MinMaxPropChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='MaximumProperty']/Docs" />
		public static readonly BindableProperty MaximumProperty = BindableProperty.Create(nameof(Maximum), typeof(double), typeof(ProgressBar), 100.0, validateValue: IsValidMaxProp, propertyChanged: MinMaxPropChanged);

		static object CoerceValueProp(BindableObject bindable, object value)
		{
			ProgressBar progBar = bindable as ProgressBar;
			double input = (double)value;

			input = Math.Max(input, progBar.Minimum);
			input = Math.Min(input, progBar.Maximum);
			return input;
		}

		static bool IsValidMinProp(BindableObject bindable, object value)
		{
			ProgressBar progBar = bindable as ProgressBar;
			double input = (double)value;

			return (input < progBar.Maximum);
		}

		static bool IsValidMaxProp(BindableObject bindable, object value)
		{
			ProgressBar progBar = bindable as ProgressBar;
			double input = (double)value;

			return (input > progBar.Minimum);
		}


		static void MinMaxPropChanged(BindableObject bindable, object oldValue, object newValue)
		{
			bindable.CoerceValue(ValueProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='Value']/Docs" />
		public double Value
		{
			get { return Progress * (Maximum - Minimum) + Minimum; }
			set { Progress = (Maximum - Minimum) / value; }
		}


		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='Minimum']/Docs" />
		public double Minimum
		{
			get { return (double)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}


		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='Maximum']/Docs" />
		public double Maximum
		{
			get { return (double)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		static void ProgressPropChanged(BindableObject bindable, object oldValue, object newValue)
		{
			ProgressBar progBar = bindable as ProgressBar;
			progBar.Value = (double)newValue * (progBar.Maximum - progBar.Minimum) + progBar.Minimum;
		}

		#endregion
	}
}
