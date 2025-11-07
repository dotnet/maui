using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21495
{
	public Maui21495() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public async Task AppThemeLeak(XamlInflator inflator)
		{
			Application.Current.Resources.Add("labelColor", Colors.HotPink);
			var page = new Maui21495(inflator);
			Application.Current.MainPage = page;
			var pagewr = new WeakReference(page);

			Application.Current.MainPage = page = null;
			await Task.Delay(10);
			GC.Collect();
			Assert.Null(pagewr.Target);
		}

		public void Dispose()
		{
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}
	}
}
