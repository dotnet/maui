using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Xunit;

//using NativeTextAlignment = Microsoft.UI.Xaml.TextAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
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

		Color GetNativeTextColor(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Resources.TryGet<SolidColorBrush>("TextControlForeground").ToColor();

		//NativeTextAlignment GetNativeHorizontalTextAlignment(EditorHandler editorHandler) =>
		//	GetNativeEditor(editorHandler).TextAlignment;

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
