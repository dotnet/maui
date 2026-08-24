#nullable enable

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Defines a named value that a <see cref="ShellContent"/> supplies to its page through Shell query parameter handling.
	/// </summary>
	/// <remarks>
	/// Parameter names are case-sensitive. When a <see cref="ShellContent"/> contains duplicate names, the last parameter
	/// in the collection supplies the value. A navigation parameter supplied to <c>GoToAsync</c> takes precedence for
	/// that navigation; selecting the content again reapplies its declarative parameters.
	/// </remarks>
	[ContentProperty(nameof(Value))]
	public sealed class ShellContentQueryParameter : BindableObject
	{
		/// <summary>Bindable property for <see cref="Name"/>.</summary>
		public static readonly BindableProperty NameProperty =
			BindableProperty.Create(nameof(Name), typeof(string), typeof(ShellContentQueryParameter), default(string));

		/// <summary>Bindable property for <see cref="Value"/>.</summary>
		public static readonly BindableProperty ValueProperty =
			BindableProperty.Create(nameof(Value), typeof(object), typeof(ShellContentQueryParameter));

		/// <summary>
		/// Gets or sets the query parameter name.
		/// </summary>
		/// <remarks>A parameter with a <see langword="null"/> or empty name is ignored.</remarks>
		public string? Name
		{
			get => (string?)GetValue(NameProperty);
			set => SetValue(NameProperty, value);
		}

		/// <summary>
		/// Gets or sets the query parameter value.
		/// </summary>
		/// <remarks>A <see langword="null"/> value is delivered to the target.</remarks>
		public object? Value
		{
			get => GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}
	}
}
