using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25141 : ContentPage
{
	public Maui25141()
	{
		InitializeComponent();
		BindingContext = new Maui25141ViewModel
		{
			Text = "Hello, Maui!",
			TriggerFlag = true
		};
	}

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Test]
		public void BindingsInDataTriggerAndMultiBindingAreCompiledCorrectly()
		{
			MockCompiler.Compile(typeof(Maui25141), treatWarningsAsErrors: true);
		}
	}
}

public class Maui25141ViewModel : INotifyPropertyChanged
{
	private bool _triggerFlag;
	public bool TriggerFlag
	{
		get => _triggerFlag;
		set
		{
			_triggerFlag = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TriggerFlag)));
		}
	}

	private string _text;
	public string Text
	{
		get => _text;
		set
		{
			_text = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
}
