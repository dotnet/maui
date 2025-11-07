using System;
using System.Linq;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3821 : ContentPage
{
	public Gh3821() => InitializeComponent();

	string _text;
	public string Text
	{
		get => _text;
		set
		{
			_text = value;
			OnPropertyChanged();
		}
	}


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void NoConflictsInNamescopes(XamlInflator inflator)
		{
			var layout = new Gh3821(inflator) { Text = "root" };
			var label0 = (Label)((Gh3821View)((StackLayout)layout.Content).Children[0]).Content;
			Assert.Equal("root", label0.Text);
		}
	}
}