using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class ProgressBar : View, IElementConfiguration<ProgressBar>
	{
		public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(ProgressBar), null);

		public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(double), typeof(ProgressBar), 0d, coerceValue: (bo, v) => ((double)v).Clamp(0, 1));

		protected override bool TabStopDefaultValueCreator() => false;

		readonly Lazy<PlatformConfigurationRegistry<ProgressBar>> _platformConfigurationRegistry;

		public ProgressBar()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ProgressBar>>(() => new PlatformConfigurationRegistry<ProgressBar>(this));
		}

		public Color ProgressColor
		{
			get { return (Color)GetValue(ProgressColorProperty); }
			set { SetValue(ProgressColorProperty, value); }
		}

		public double Progress
		{
			get { return (double)GetValue(ProgressProperty); }
			set { SetValue(ProgressProperty, value); }
		}

		public Task<bool> ProgressTo(double value, uint length, Easing easing)
		{
			var tcs = new TaskCompletionSource<bool>();

			this.Animate("Progress", d => Progress = d, Progress, value, length: length, easing: easing, finished: (d, finished) => tcs.SetResult(finished));

			return tcs.Task;
		}

		public IPlatformElementConfiguration<T, ProgressBar> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}