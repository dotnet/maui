using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Trigger that activates when the device display rotation matches the specified <see cref="Rotation"/>.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="DisplayRotationStateTrigger"/> enables developers to create visual states that are triggered based on the device's display rotation.
/// Unlike <see cref="OrientationStateTrigger"/> which only differentiates between portrait and landscape orientations,
/// this trigger provides granular control over specific rotation angles (0°, 90°, 180°, 270°).
/// </para>
/// <para>
/// This trigger is particularly useful for applications that need to respond to specific device orientations,
/// such as games that have different layouts for each rotation state or apps that need to handle upside-down orientations differently.
/// </para>
/// </remarks>
/// <example>
/// <para>
/// The following example shows how to use <see cref="DisplayRotationStateTrigger"/> to change the background color based on device rotation:
/// </para>
/// <code lang="XAML"><![CDATA[
/// <ContentPage.Resources>
///     <Style TargetType="ContentPage">
///         <Setter Property="VisualStateManager.VisualStateGroups">
///             <VisualStateGroupList>
///                 <VisualStateGroup>
///                     <VisualState Name="Rotation0">
///                         <VisualState.StateTriggers>
///                             <controls:DisplayRotationStateTrigger Rotation="Rotation0" />
///                         </VisualState.StateTriggers>
///                         <VisualState.Setters>
///                             <Setter Property="BackgroundColor" Value="Red" />
///                         </VisualState.Setters>
///                     </VisualState>
///                     <VisualState Name="Rotation90">
///                         <VisualState.StateTriggers>
///                             <controls:DisplayRotationStateTrigger Rotation="Rotation90" />
///                         </VisualState.StateTriggers>
///                         <VisualState.Setters>
///                             <Setter Property="BackgroundColor" Value="Green" />
///                         </VisualState.Setters>
///                     </VisualState>
///                     <VisualState Name="Rotation180">
///                         <VisualState.StateTriggers>
///                             <controls:DisplayRotationStateTrigger Rotation="Rotation180" />
///                         </VisualState.StateTriggers>
///                         <VisualState.Setters>
///                             <Setter Property="BackgroundColor" Value="Blue" />
///                         </VisualState.Setters>
///                     </VisualState>
///                     <VisualState Name="Rotation270">
///                         <VisualState.StateTriggers>
///                             <controls:DisplayRotationStateTrigger Rotation="Rotation270" />
///                         </VisualState.StateTriggers>
///                         <VisualState.Setters>
///                             <Setter Property="BackgroundColor" Value="Yellow" />
///                         </VisualState.Setters>
///                     </VisualState>
///                 </VisualStateGroup>
///             </VisualStateGroupList>
///         </Setter>
///     </Style>
/// </ContentPage.Resources>
/// ]]></code>
/// </example>
public sealed class DisplayRotationStateTrigger : StateTriggerBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DisplayRotationStateTrigger"/> class.
	/// </summary>
	/// <remarks>
	/// The trigger will immediately evaluate its state based on the current device display rotation.
	/// </remarks>
	public DisplayRotationStateTrigger()
	{
		UpdateState();
	}

	/// <summary>
	/// Gets or sets the display rotation that will activate this trigger.
	/// </summary>
	/// <value>
	/// The <see cref="DisplayRotation"/> value that will activate this trigger when the device display matches it.
	/// </value>
	/// <remarks>
	/// <para>
	/// The trigger will be active when the device's current display rotation matches this property value.
	/// Possible values are:
	/// </para>
	/// <list type="bullet">
	/// <item><description><see cref="DisplayRotation.Rotation0"/> - Device is in its natural position (0 degrees)</description></item>
	/// <item><description><see cref="DisplayRotation.Rotation90"/> - Device is rotated 90 degrees</description></item>
	/// <item><description><see cref="DisplayRotation.Rotation180"/> - Device is rotated 180 degrees (upside down)</description></item>
	/// <item><description><see cref="DisplayRotation.Rotation270"/> - Device is rotated 270 degrees</description></item>
	/// <item><description><see cref="DisplayRotation.Unknown"/> - Device rotation is unknown</description></item>
	/// </list>
	/// </remarks>
	public DisplayRotation Rotation
	{
		get => (DisplayRotation)GetValue(RotationProperty);
		set => SetValue(RotationProperty, value);
	}

	/// <summary>
	/// Identifies the <see cref="Rotation"/> bindable property.
	/// </summary>
	public static readonly BindableProperty RotationProperty =
		BindableProperty.Create(nameof(Rotation), typeof(DisplayRotation), typeof(DisplayRotationStateTrigger), DisplayRotation.Unknown,
			propertyChanged: OnRotationChanged);

	static void OnRotationChanged(BindableObject bindable, object oldvalue, object newvalue)
	{
		((DisplayRotationStateTrigger)bindable).UpdateState();
	}

	protected override void OnAttached()
	{
		base.OnAttached();

		if (!DesignMode.IsDesignModeEnabled)
		{
			UpdateState();
			DeviceDisplay.MainDisplayInfoChanged += OnInfoPropertyChanged;
		}
	}

	protected override void OnDetached()
	{
		base.OnDetached();

		DeviceDisplay.MainDisplayInfoChanged -= OnInfoPropertyChanged;
	}

	void OnInfoPropertyChanged(object? sender, DisplayInfoChangedEventArgs e) =>
		UpdateState();

	void UpdateState()
	{
		var currentRotation = DeviceDisplay.MainDisplayInfo.Rotation;
		SetActive(currentRotation == Rotation);
	}
}
