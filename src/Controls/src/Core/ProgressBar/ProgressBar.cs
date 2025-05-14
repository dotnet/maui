#nullable disable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A view control that displays progress as a partially filled bar.
	/// </summary>
	/// <remarks>
	/// The ProgressBar displays progress as a horizontal bar that is filled to a percentage represented by the <see cref="Progress"/> property.
	/// Use the <see cref="ProgressTo"/> method to animate the progress bar.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler<ProgressBarHandler>]
	public partial class ProgressBar : View, IElementConfiguration<ProgressBar>, IProgress
	{
		/// <summary>Bindable property for <see cref="ProgressColor"/>.</summary>
		public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(ProgressBar), null);

		/// <summary>Bindable property for <see cref="Progress"/>.</summary>
		public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(double), typeof(ProgressBar), 0d, coerceValue: (bo, v) => ((double)v).Clamp(0, 1));

		readonly Lazy<PlatformConfigurationRegistry<ProgressBar>> _platformConfigurationRegistry;

		/// <summary>
		/// Initializes a new instance of the ProgressBar class.
		/// </summary>
		public ProgressBar()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ProgressBar>>(() => new PlatformConfigurationRegistry<ProgressBar>(this));
		}

		/// <summary>
		/// Gets or sets the color of the progress bar. This is a bindable property.
		/// </summary>
		/// <value>The color of the progress bar.</value>
		public Color ProgressColor
		{
			get { return (Color)GetValue(ProgressColorProperty); }
			set { SetValue(ProgressColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the progress value. This is a bindable property.
		/// </summary>
		/// <value>A value between 0.0 and 1.0 that specifies the fraction of the bar that is filled. Values outside this range will be clamped.</value>
		public double Progress
		{
			get { return (double)GetValue(ProgressProperty); }
			set { SetValue(ProgressProperty, value); }
		}

		/// <summary>
		/// Animates the <see cref="Progress"/> property from its current value to the specified value.
		/// </summary>
		/// <param name="value">The target progress value (0.0 to 1.0).</param>
		/// <param name="length">The length of the animation in milliseconds.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A task that completes when the animation finishes, with a result indicating whether the animation completed successfully.</returns>
		public Task<bool> ProgressTo(double value, uint length, Easing easing)
		{
			var tcs = new TaskCompletionSource<bool>();

			this.Animate("Progress", d => Progress = d, Progress, value, length: length, easing: easing, finished: (d, finished) => tcs.SetResult(finished));

			return tcs.Task;
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, ProgressBar> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, Progress = {Progress}";
		}
	}
}