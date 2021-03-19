using System;

namespace Microsoft.Maui
{
	public interface ITimePicker : IView
	{
		string Format { get; }
		TimeSpan Time { get; set; }
	}
}