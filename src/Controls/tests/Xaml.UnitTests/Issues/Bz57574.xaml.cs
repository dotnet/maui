using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz57574NotificationEventArgs<T> : EventArgs
{
	public T Message { get; set; }
}

public class Bz57574Notificator<T> : View
{
	public void Notify(T message)
	{
		Notified?.Invoke(this, new Bz57574NotificationEventArgs<T> { Message = message });
	}

	public event EventHandler<Bz57574NotificationEventArgs<T>> Notified;
}

public partial class Bz57574
{
	public Bz57574()
	{
		notificator.Notified += OnNotified;
		InitializeComponent();
	}

	string notification;
	public void OnNotified(object sender, Bz57574NotificationEventArgs<string> args)
	{
		notification = args.Message;
	}


	public class Tests : IDisposable
	{


		public void Dispose() { }
		[Theory]
		[Values]
		public void EventWithGenericEventHandlers(XamlInflator inflator)
		{
			var layout = new Bz57574(inflator);
			// TODO: Convert Assume to Skip or Assert`r`nAssert.True(true);
			layout.notificator.Notify("Foo");
			Assert.Equal("Foo", layout.notification);
		}
	}
}