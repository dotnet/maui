using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22036
{
	public Maui22036()
	{
		InitializeComponent();
	}

	public Maui22036(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Fact]
		public async Task StyleWithTriggerLeak(bool useCompiledXaml)
		{
			var style = new Style(typeof(ContentPage));
			var trigger = new EventTrigger { Event = nameof(Appearing) };
			trigger.Actions.Add(new EmptyTriggerAction());
			style.Triggers.Add(trigger);

			Application.Current.Resources.Add(style);

			var pagewr = new WeakReference(new Maui22036(useCompiledXaml));
			await Task.Delay(10);
			GC.Collect();
			Assert.Null(pagewr.Target, "Page leaked");
		}
	}

	class EmptyTriggerAction : TriggerAction<ContentPage>
	{
		protected override void Invoke(ContentPage sender)
		{
		}
	}
}
