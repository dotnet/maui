using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class InputViewTests : BaseTestFixture
	{
		[Theory]
		[InlineData(TextTransform.Uppercase, "d", "D")]
		[InlineData(TextTransform.Lowercase, "D", "d")]
		[InlineData(TextTransform.None, "d", "d")]
		public void HandlerTextChangesRaiseOneEventWithTransformedText(TextTransform transform, string input, string expected)
		{
			foreach (var view in new InputView[] { new Entry(), new Editor(), new SearchBar() })
			{
				view.TextTransform = transform;
				view.Text = "123";
				var changes = new List<TextChangedEventArgs>();
				view.TextChanged += (_, args) => changes.Add(args);

				var textInput = (ITextInput)view;
				textInput.Text = input;
				textInput.Text = expected; // Simulate the native echo after mapping.

				Assert.Equal(expected, view.Text);
				var change = Assert.Single(changes);
				Assert.Equal("123", change.OldTextValue);
				Assert.Equal(expected, change.NewTextValue);
			}
		}

		[Theory]
		[InlineData(TextTransform.Uppercase)]
		[InlineData(TextTransform.Lowercase)]
		public void HandlerTextChangesPreservePasswordCasing(TextTransform transform)
		{
			var entry = new Entry { IsPassword = true, TextTransform = transform };
			var boundEntry = new Entry();
			entry.SetBinding(InputView.TextProperty, new Binding(nameof(InputView.Text), source: boundEntry));
			var changes = new List<TextChangedEventArgs>();
			entry.TextChanged += (_, args) => changes.Add(args);

			((ITextInput)entry).Text = "aB";

			Assert.Equal("aB", entry.Text);
			Assert.Equal("aB", boundEntry.Text);
			Assert.Equal("aB", Assert.Single(changes).NewTextValue);
		}

		[Theory]
		[InlineData(TextTransform.Uppercase, "A", "a")]
		[InlineData(TextTransform.Lowercase, "a", "A")]
		public void CaseOnlyHandlerEditRemapsTextWithoutNotification(TextTransform transform, string expected, string input)
		{
			foreach (var view in new InputView[] { new Entry(), new Editor(), new SearchBar() })
			{
				view.TextTransform = transform;
				view.Text = expected;
				var handler = new TextMapperHandler();
				view.Handler = handler;
				var changes = 0;
				view.TextChanged += (_, _) => changes++;

				handler.PlatformText = input;
				((ITextInput)view).UpdateText(input);

				Assert.Equal(expected, handler.PlatformText);
				Assert.Equal(expected, view.Text);
				Assert.Equal(0, changes);
			}
		}

		class TextMapperHandler : ViewHandler<IView, object>
		{
			static readonly PropertyMapper<IView, TextMapperHandler> Mapper = new()
			{
				[nameof(ITextInput.Text)] = (handler, view) => handler.PlatformText = ((ITextInput)view).Text,
			};

			public TextMapperHandler() : base(Mapper)
			{
			}

			public string PlatformText { get; set; }

			protected override object CreatePlatformView() => new object();
		}

		[Theory]
		[InlineData(typeof(Entry))]
		[InlineData(typeof(Editor))]
		[InlineData(typeof(SearchBar))]
		public void TwoWayBindingsStayWorking(Type type)
		{
			var inputView = Activator.CreateInstance(type) as InputView;
			var bindInputView = Activator.CreateInstance(type) as InputView;

			inputView.BindingContext = bindInputView;
			inputView.SetBinding(InputView.TextProperty, nameof(InputView.Text), BindingMode.TwoWay);
			(inputView as ITextInput).Text = "Some other text";

			Assert.Equal(inputView.Text, bindInputView.Text);
			bindInputView.Text = "Different Text";
			Assert.Equal(inputView.Text, bindInputView.Text);
		}

		[Theory]
		[InlineData(typeof(Entry))]
		[InlineData(typeof(Editor))]
		[InlineData(typeof(SearchBar))]
		public void TwoWayBindingsStayWorkingSelectionLength(Type type)
		{
			var inputView = Activator.CreateInstance(type) as InputView;
			var bindToInputView = Activator.CreateInstance(type) as InputView;

			inputView.Text = "This is some text";
			bindToInputView.Text = "This is some other text";

			inputView.BindingContext = bindToInputView;
			inputView.SetBinding(InputView.SelectionLengthProperty, nameof(InputView.SelectionLength), BindingMode.TwoWay);
			(inputView as ITextInput).SelectionLength = 10;

			Assert.Equal(inputView.SelectionLength, bindToInputView.SelectionLength);
			bindToInputView.SelectionLength = 5;
			Assert.Equal(inputView.SelectionLength, bindToInputView.SelectionLength);
		}

		[Theory]
		[InlineData(typeof(Entry))]
		[InlineData(typeof(Editor))]
		[InlineData(typeof(SearchBar))]
		public void TwoWayBindingsStayWorkingCursorPosition(Type type)
		{
			var inputView = Activator.CreateInstance(type) as InputView;
			var bindToInputView = Activator.CreateInstance(type) as InputView;

			inputView.Text = "This is some text";
			bindToInputView.Text = "This is some other text";

			inputView.BindingContext = bindToInputView;
			inputView.SetBinding(InputView.CursorPositionProperty, nameof(InputView.CursorPosition), BindingMode.TwoWay);
			(inputView as ITextInput).CursorPosition = 10;

			Assert.Equal(inputView.CursorPosition, bindToInputView.CursorPosition);
			bindToInputView.CursorPosition = 5;
			Assert.Equal(inputView.CursorPosition, bindToInputView.CursorPosition);
		}
	}
}
