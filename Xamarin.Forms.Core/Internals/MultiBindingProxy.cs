namespace Xamarin.Forms.Internals
{
	// This class serves as a "proxy" for the target and the sources,
	// with the MultiBinding serving as the bridge between them. 
	// All the BindingExpression's actually bind between the proxies and the
	// target/sources. This avoids the need for what would otherwise be some very 
	// ugly subclassing or modifications to BindingExpression. And it
	// makes it very easy to supported nested MultiBinding's which is 
	// not possible with WPF.
	internal class MultiBindingProxy : BindableObject
	{
		internal bool SuspendValueChangeNotification { get; private set; }

		internal bool IsTarget { get; }

		public static readonly BindableProperty ValueProperty = BindableProperty.Create(
			nameof(Value),
			typeof(object),
			typeof(MultiBindingProxy),
			null,
			propertyChanged:
				new BindableProperty.BindingPropertyChangedDelegate(
					(obj, oldVal, newVal)=>
						(obj as MultiBindingProxy).OnValueChanged(oldVal, newVal)));

		internal MultiBindingProxy(MultiBinding multiBinding, bool isTarget)
		{
			this.MultiBinding = multiBinding;
			this.IsTarget = isTarget;
		}		
		
		public object Value
		{
			get
			{
				return GetValue(ValueProperty);
			}
			set
			{
				SetValue(ValueProperty, value);
			}
		}
		
		internal MultiBinding MultiBinding { get; }

		internal BindingMode RealizedMode { get; set; }

		internal void SetValueSilent(BindableProperty property, object value)
		{
			bool suspended = this.SuspendValueChangeNotification;
			this.SuspendValueChangeNotification = true;
			try
			{
				SetValue(property, value);
			}
			finally
			{
				SuspendValueChangeNotification = suspended;
			}
		}

		void OnValueChanged(object oldValue, object newValue)
		{
			if (this.IsTarget)
				// Updates to target value are handled by MultiBinding.Apply
				return;

			if (!SuspendValueChangeNotification)
				this.MultiBinding.ApplyBindingProxyValues(this);
		}
	}
}
