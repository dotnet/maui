namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a shortcut key for a <see cref="MenuItem"/>.
	/// </summary>
	public class KeyboardAccelerator : BindableObject, IKeyboardAccelerator
	{
		/// <summary>Bindable property for <see cref="Modifiers"/>.</summary>
		public static readonly BindableProperty ModifiersProperty = BindableProperty.Create(nameof(Modifiers), typeof(KeyboardAcceleratorModifiers), typeof(KeyboardAccelerator), KeyboardAcceleratorModifiers.None);

		/// <summary>Bindable property for <see cref="Key"/>.</summary>
		public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(KeyboardAccelerator), null);

		/// <summary>
		/// Gets the modifiers for the keyboard accelerator.
		/// </summary>
		public KeyboardAcceleratorModifiers Modifiers 
		{
			get => (KeyboardAcceleratorModifiers)GetValue(ModifiersProperty);
			set => SetValue(ModifiersProperty, value);
		}

		/// <summary>
		/// Gets the key for the keyboard accelerator.
		/// </summary>
		public string? Key
		{
			get => (string?)GetValue(KeyProperty);
			set => SetValue(KeyProperty, value);
		}
	}
}
