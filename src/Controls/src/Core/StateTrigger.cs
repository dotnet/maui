#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A state trigger that activates a visual state when <see cref="IsActive"/> is set to <see langword="true"/>.
	/// </summary>
	public sealed class StateTrigger : StateTriggerBase
	{
		/// <summary>
		/// Gets or sets a value indicating whether this trigger is active. This is a bindable property.
		/// </summary>
		public new bool IsActive
		{
			get => (bool)GetValue(IsActiveProperty);
			set => SetValue(IsActiveProperty, value);
		}

		/// <summary>Bindable property for <see cref="IsActive"/>.</summary>
		public static readonly BindableProperty IsActiveProperty =
			BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(StateTrigger), default(bool),
				propertyChanged: OnIsActiveChanged);

		static void OnIsActiveChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			if (newvalue is bool b)
			{
				((StateTrigger)bindable).UpdateState();
			}
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			UpdateState();
		}

		void UpdateState()
		{
			SetActive(IsActive);
		}
	}
}
