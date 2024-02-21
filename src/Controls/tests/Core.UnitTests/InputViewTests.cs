using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class InputViewTests : BaseTestFixture
	{

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
