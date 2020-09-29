namespace Xamarin.Forms
{
	public sealed class StateTrigger : StateTriggerBase
	{
		public new bool IsActive
		{
			get => (bool)GetValue(IsActiveProperty);
			set => SetValue(IsActiveProperty, value);
		}

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