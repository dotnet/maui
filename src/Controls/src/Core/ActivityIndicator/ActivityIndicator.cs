#nullable disable
using System;
using System.Diagnostics;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="Type[@FullName='Microsoft.Maui.Controls.ActivityIndicator']/Docs/*" />
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler(typeof(ActivityIndicatorHandler))]
	public partial class ActivityIndicator : View, IColorElement, IElementConfiguration<ActivityIndicator>, IActivityIndicator
	{
		/// <summary>Bindable property for <see cref="IsRunning"/>.</summary>
		public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(nameof(IsRunning), typeof(bool), typeof(ActivityIndicator), default(bool));

		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		readonly Lazy<PlatformConfigurationRegistry<ActivityIndicator>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ActivityIndicator()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ActivityIndicator>>(() => new PlatformConfigurationRegistry<ActivityIndicator>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="//Member[@MemberName='Color']/Docs/*" />
		public Color Color
		{
			get { return (Color)GetValue(ColorElement.ColorProperty); }
			set { SetValue(ColorElement.ColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ActivityIndicator.xml" path="//Member[@MemberName='IsRunning']/Docs/*" />
		public bool IsRunning
		{
			get { return (bool)GetValue(IsRunningProperty); }
			set { SetValue(IsRunningProperty, value); }
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, ActivityIndicator> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(IsRunning), IsRunning);
			return $"{base.GetDebuggerDisplay()}, {debugText}";
		}
	}
}