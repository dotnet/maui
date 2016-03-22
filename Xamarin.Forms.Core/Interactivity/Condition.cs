using System;

namespace Xamarin.Forms
{
	public abstract class Condition
	{
		Action<BindableObject, bool, bool> _conditionChanged;

		bool _isSealed;

		internal Condition()
		{
		}

		internal Action<BindableObject, bool, bool> ConditionChanged
		{
			get { return _conditionChanged; }
			set
			{
				if (_conditionChanged == value)
					return;
				if (_conditionChanged != null)
					throw new InvalidOperationException("The same condition instance can not be reused");
				_conditionChanged = value;
			}
		}

		internal bool IsSealed
		{
			get { return _isSealed; }
			set
			{
				if (_isSealed == value)
					return;
				if (!value)
					throw new InvalidOperationException("What is sealed can not be unsealed.");
				_isSealed = value;
				OnSealed();
			}
		}

		internal abstract bool GetState(BindableObject bindable);

		internal virtual void OnSealed()
		{
		}

		internal abstract void SetUp(BindableObject bindable);
		internal abstract void TearDown(BindableObject bindable);
	}
}