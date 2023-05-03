#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/StateTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.StateTrigger']/Docs/*" />
	public sealed class StateTrigger : StateTriggerBase
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/StateTrigger.xml" path="//Member[@MemberName='IsActive']/Docs/*" />
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
