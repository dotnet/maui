using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Xunit;
using NativeVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
		[Fact(DisplayName = "Vertical text alignment uses the current template content")]
		public async Task VerticalTextAlignmentUsesCurrentTemplateContent()
		{
			var editor = new EditorStub();

			await AttachAndRun(editor, handler =>
			{
				var textBox = GetNativeEditor(handler);
				MauiTextBox.SetVerticalTextAlignment(textBox, NativeVerticalAlignment.Bottom);

				var previousContentElement = textBox.GetDescendantByName<ScrollViewer>("ContentElement");
				Assert.NotNull(previousContentElement);
				Assert.Equal(NativeVerticalAlignment.Bottom, previousContentElement.VerticalAlignment);

				textBox.Template = CreateTextBoxTemplate();
				textBox.ApplyTemplate();

				var currentContentElement = textBox.GetDescendantByName<ScrollViewer>("ContentElement");
				Assert.NotNull(currentContentElement);
				Assert.NotSame(previousContentElement, currentContentElement);

				MauiTextBox.SetVerticalTextAlignment(textBox, NativeVerticalAlignment.Top);

				Assert.Equal(NativeVerticalAlignment.Top, currentContentElement.VerticalAlignment);
				Assert.Equal(NativeVerticalAlignment.Bottom, previousContentElement.VerticalAlignment);
			});
		}

		static ControlTemplate CreateTextBoxTemplate() =>
			(ControlTemplate)XamlReader.Load(
				"""
				<ControlTemplate
					xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
					xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
					TargetType="TextBox">
					<Grid>
						<ScrollViewer x:Name="ContentElement" />
						<TextBlock x:Name="PlaceholderTextContentPresenter" />
					</Grid>
				</ControlTemplate>
				""");

		static TextBox GetNativeEditor(EditorHandler editorHandler) =>
			editorHandler.PlatformView;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;

		internal static void SetNativeText(EditorHandler editorHandler, string text) =>
			GetNativeEditor(editorHandler).Text = text;

		internal static int GetCursorStartPosition(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).GetCursorPosition();

		internal static void UpdateCursorStartPosition(EditorHandler editorHandler, int position) =>
			GetNativeEditor(editorHandler).SelectionStart = position;

		string GetNativePlaceholderText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).PlaceholderText;

		Color GetNativePlaceholderColor(EditorHandler editorHandler) =>
			((SolidColorBrush)GetNativeEditor(editorHandler).PlaceholderForeground).ToColor();

		//double GetNativeCharacterSpacing(EditorHandler editorHandler) =>
		//	GetNativeEditor(editorHandler).CharacterSpacing;

		//double GetNativeUnscaledFontSize(EditorHandler editorHandler) =>
		//	GetNativeEditor(editorHandler).FontSize;

		bool GetNativeIsReadOnly(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).IsReadOnly;

		bool GetNativeIsTextPredictionEnabled(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).IsTextPredictionEnabled;

		bool GetNativeIsSpellCheckEnabled(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).IsSpellCheckEnabled;

		Color GetNativeTextColor(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Resources.TryGet<SolidColorBrush>("TextControlForeground").ToColor();

		//NativeTextAlignment GetNativeHorizontalTextAlignment(EditorHandler editorHandler) =>
		//	GetNativeEditor(editorHandler).TextAlignment;

		NativeVerticalAlignment GetNativeVerticalTextAlignment(EditorHandler editorHandler)
		{
			var textBox = GetNativeEditor(editorHandler);

			var sv = textBox.GetDescendantByName<ScrollViewer>("ContentElement");
			var placeholder = textBox.GetDescendantByName<TextBlock>("PlaceholderTextContentPresenter");

			Assert.Equal(sv.VerticalAlignment, placeholder.VerticalAlignment);

			return sv.VerticalAlignment;
		}

		NativeVerticalAlignment GetNativeVerticalTextAlignment(TextAlignment textAlignment) =>
			textAlignment.ToPlatformVerticalAlignment();

		bool IsInputScopeEquals(InputScope inputScope, InputScopeNameValue nameValue)
		{
			if (inputScope == null || inputScope.Names.Count == 0)
				return false;

			return inputScope.Names[0].NameValue == nameValue;
		}

		bool GetNativeIsNumericKeyboard(EditorHandler editorHandler) =>
			IsInputScopeEquals(GetNativeEditor(editorHandler).InputScope, InputScopeNameValue.Number);

		bool GetNativeIsEmailKeyboard(EditorHandler editorHandler) =>
			IsInputScopeEquals(GetNativeEditor(editorHandler).InputScope, InputScopeNameValue.EmailSmtpAddress);

		bool GetNativeIsTelephoneKeyboard(EditorHandler editorHandler) =>
			IsInputScopeEquals(GetNativeEditor(editorHandler).InputScope, InputScopeNameValue.TelephoneNumber);

		bool GetNativeIsUrlKeyboard(EditorHandler editorHandler) =>
			IsInputScopeEquals(GetNativeEditor(editorHandler).InputScope, InputScopeNameValue.Url);

		bool GetNativeIsTextKeyboard(EditorHandler editorHandler) =>
			IsInputScopeEquals(GetNativeEditor(editorHandler).InputScope, InputScopeNameValue.Default);

		bool GetNativeIsChatKeyboard(EditorHandler editorHandler) =>
			IsInputScopeEquals(GetNativeEditor(editorHandler).InputScope, InputScopeNameValue.Chat);

		int GetNativeCursorPosition(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).GetCursorPosition();

		int GetNativeSelectionLength(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).SelectionLength;
	}
}
