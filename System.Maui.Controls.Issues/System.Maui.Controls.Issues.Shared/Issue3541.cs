using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3541, "[WPF] Fix Local store not persistant when restarting App", PlatformAffected.WPF)]
	public class Issue3541 : TestContentPage
	{
		Entry _entry;
		Label _valueSaved = new Label();
		const string _success = "Success";
		const string _buttonText = "Run Test";

		protected override void Init()
		{
			var stack = new StackLayout();

			_entry = new Entry
			{
				Placeholder = "Enter a text then click on save",
				Text = GetText()
			};

			Button saveButton = new Button
			{
				Text = "Save"
			};

			saveButton.Clicked += SaveButton_Clicked;
			
			stack.Children.Add(_entry);
			stack.Children.Add(saveButton);
			stack.Children.Add(new Button()
			{
				Text = _buttonText,
				Command = new Command(async () =>
				{
					await Save("Some long text that's longer");
					await Save("");
					await Save(_success);
				})
			});
			stack.Children.Add(_valueSaved);
			Content = stack;
		}

		private async void SaveButton_Clicked(object sender, System.EventArgs e)
		{
			await this.Save(_entry.Text);
		}

		private async Task Save(string text)
		{
			Application.Current.Properties[nameof(Issue3541)] = text;
			await Application.Current.SavePropertiesAsync();
			await CheckWhatIsStored();
		}

		async Task CheckWhatIsStored()
		{
			var deserializer = DependencyService.Get<IDeserializer>();
			object result = null;
			(await deserializer.DeserializePropertiesAsync())?.TryGetValue(nameof(Issue3541), out result);
			_valueSaved.Text = $"{result}";
		}

		private string GetText()
		{
			if (Application.Current.Properties.ContainsKey(nameof(Issue3541)))
				return Application.Current.Properties[nameof(Issue3541)] as string;

			return null;
		}


#if UITEST
		[Test]
		public void SaveDifferentLengthValuesIntoPropertyStore()
		{
			RunningApp.WaitForElement(_buttonText);
			RunningApp.Tap(_buttonText);
			RunningApp.WaitForElement(_success);
		}
#endif
	}
}
