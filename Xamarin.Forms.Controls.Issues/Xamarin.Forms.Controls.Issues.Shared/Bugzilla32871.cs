using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.ComponentModel;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32871, "Numeric Keyboard does not work when text has a binding to a value", PlatformAffected.Default)]
	public class Bugzilla32871 : TestContentPage
	{

		public static class Id
		{
			public const string ValueEntry = nameof(ValueEntry);
			public const string ValueLabel = nameof(ValueLabel);
			public const string TypeEntry = nameof(TypeEntry);
			public const string AddHorizontal = nameof(AddHorizontal);
			public const string TypeLabel = nameof(TypeLabel);
			public const string BindButton = nameof(BindButton);
		}

		[Preserve(AllMembers = true)]
		public class ViewModel<T> : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public void OnPropertyChanged(string propertyName)
				=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

			public ViewModel() { }

			public T _value;
			public T Value
			{
				get { return _value; }
				set
				{
					_value = value;
					s_labelValue.Text = $"value: {value}";
					OnPropertyChanged(nameof(Value));
				}
			}
		}

		public static Label s_labelValue;

		protected override void Init()
		{
			var stack = new StackLayout();

			// text to be coerced
			var entryValue = new Entry() { AutomationId = Id.ValueEntry };
			stack.Children.Add(entryValue);

			// feedback of the value coerced from text
			s_labelValue = new Label() { Text = "value: [none]", AutomationId = Id.ValueLabel };
			stack.Children.Add(s_labelValue);

			// the type being coerced from text
			var entryType = new Entry() { AutomationId = Id.TypeEntry };
			stack.Children.Add(entryType);

			// feedback of the type being coerced from text
			var labelType = new Label() { Text = "type: [none]", AutomationId = Id.TypeLabel };
			stack.Children.Add(labelType);

			// create view model of the type being coerced; clear text
			var bindButton = new Button() { Text = "Bind", AutomationId = Id.BindButton };
			stack.Children.Add(bindButton);
			bindButton.Clicked += (o, e) =>
			{
				try
				{
					var type = Type.GetType(entryType.Text);
					BindingContext = Activator.CreateInstance(
						typeof(ViewModel<>).MakeGenericType(type)
					);
					entryValue.Text = string.Empty;
					labelType.Text = $"type: {type.FullName}";
				}
				catch (Exception ex)
				{
					labelType.Text = $"type: {ex.Message}";
				}

				entryValue.SetBinding(Entry.TextProperty, "Value");
			};

			this.Content = stack;
		}
#if UITEST
		[Test]
		public void Issue32871Test()
		{
			RunningApp.WaitForElement(q => q.Marked(Id.BindButton));

			var types = new Type[] {
				typeof(double),
				typeof(int),
				typeof(float),
				typeof(decimal),
			};

			var values = new string[] {
				"0.1",
				"0",
				"-0.1",
				"-1.1",
				"-.1",
				".1",
				"00",
				"01",
			};

			foreach (var type in types)
				Theory(type, values);
		}
		public bool TryParse(Type type, string value, ref string result)
		{

			if (value.EndsWith("."))
				return false;

			if (value == "-0")
				return false;

			try
			{
				result = $"{Convert.ChangeType(value, type)}";
				return true;
			}
			catch
			{
				return false;
			}
		}
		public void Theory(Type type, string[] values)
		{
			RunningApp.ClearText(Id.TypeEntry);
			RunningApp.EnterText(Id.TypeEntry, type.FullName);
			RunningApp.Tap(Id.BindButton);
			RunningApp.WaitForElement(q => q.Marked($"type: {type.FullName}"));

			foreach (var value in values)
				Theory(type, value);
		}
		public void Theory(Type type, string value)
		{
			try { Convert.ChangeType(value, type); } catch { return; }

			Console.WriteLine($"TEST CASE: type={type.FullName}, value={value}");

			RunningApp.ClearText(Id.ValueEntry);

			var input = string.Empty;
			var output = string.Empty;
			foreach (var c in value)
			{
				input += c;
				RunningApp.EnterText(Id.ValueEntry, $"{c}");

				if (TryParse(type, input, ref output))
					input = output;

				if (string.IsNullOrEmpty(output))
					continue;

				RunningApp.WaitForElement(q => q.Marked($"value: {output}"));
			}

			if (!Equals(Convert.ChangeType(value, type), Convert.ChangeType(output, type)))
				throw new Exception($"Value '{value}' entered as '{output}'.");
		}
#endif
	}
}
