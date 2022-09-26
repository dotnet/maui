using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="Type[@FullName='Microsoft.Maui.Controls.ProgressBar']/Docs/*" />
	public partial class ProgressBar : View, IElementConfiguration<ProgressBar>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='ProgressColorProperty']/Docs/*" />
		public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(ProgressBar), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='ProgressProperty']/Docs/*" />
		public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(double), typeof(ProgressBar), 0d, coerceValue: (bo, v) => ((double)v).Clamp(0, 1));

		readonly Lazy<PlatformConfigurationRegistry<ProgressBar>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ProgressBar()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ProgressBar>>(() => new PlatformConfigurationRegistry<ProgressBar>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='ProgressColor']/Docs/*" />
		public Color ProgressColor
		{
			get { return (Color)GetValue(ProgressColorProperty); }
			set { SetValue(ProgressColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='Progress']/Docs/*" />
		public double Progress
		{
			get { return (double)GetValue(ProgressProperty); }
			set { SetValue(ProgressProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ProgressBar.xml" path="//Member[@MemberName='ProgressTo']/Docs/*" />
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
	}
}