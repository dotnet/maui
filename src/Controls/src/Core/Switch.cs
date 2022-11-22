using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="Type[@FullName='Microsoft.Maui.Controls.Switch']/Docs/*" />
	public partial class Switch : View, IElementConfiguration<Switch>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='SwitchOnVisualState']/Docs/*" />
		public const string SwitchOnVisualState = "On";
		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='SwitchOffVisualState']/Docs/*" />
		public const string SwitchOffVisualState = "Off";

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='IsToggledProperty']/Docs/*" />
		public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(Switch), false, propertyChanged: (bindable, oldValue, newValue) =>
		{
			((Switch)bindable).Toggled?.Invoke(bindable, new ToggledEventArgs((bool)newValue));
			((Switch)bindable).ChangeVisualState();
			((IView)bindable)?.Handler?.UpdateValue(nameof(ISwitch.TrackColor));

		}, defaultBindingMode: BindingMode.TwoWay);

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='OnColorProperty']/Docs/*" />
		public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(Switch), null,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				((IView)bindable)?.Handler?.UpdateValue(nameof(ISwitch.TrackColor));
			});

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='ThumbColorProperty']/Docs/*" />
		public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(Switch), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='OnColor']/Docs/*" />
		public Color OnColor
		{
			get { return (Color)GetValue(OnColorProperty); }
			set { SetValue(OnColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='ThumbColor']/Docs/*" />
		public Color ThumbColor
		{
			get { return (Color)GetValue(ThumbColorProperty); }
			set { SetValue(ThumbColorProperty, value); }
		}

		readonly Lazy<PlatformConfigurationRegistry<Switch>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Switch()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Switch>>(() => new PlatformConfigurationRegistry<Switch>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='IsToggled']/Docs/*" />
		public bool IsToggled
		{
			get { return (bool)GetValue(IsToggledProperty); }
			set { SetValue(IsToggledProperty, value); }
		}

		protected internal override void ChangeVisualState()
		{
			base.ChangeVisualState();
			if (IsEnabled && IsToggled)
				VisualStateManager.GoToState(this, SwitchOnVisualState);
			else if (IsEnabled && !IsToggled)
				VisualStateManager.GoToState(this, SwitchOffVisualState);
		}

		public event EventHandler<ToggledEventArgs> Toggled;

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Switch> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}