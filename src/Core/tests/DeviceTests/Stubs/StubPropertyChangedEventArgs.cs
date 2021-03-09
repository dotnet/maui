using System;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class StubPropertyChangedEventArgs<T> : EventArgs
	{
		public StubPropertyChangedEventArgs(T oldValue, T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}

		public T OldValue { get; }

		public T NewValue { get; }
	}
}