using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests.Elements
{
	class EditorTests
	{



		[Theory]
		[InlineData(EditorAutoSizeOption.Disabled)]
		[InlineData(EditorAutoSizeOption.TextChanges)]
		public async Task AutoSizeInitializesCorrectly(EditorAutoSizeOption option)
		{
			var editor = new EditorStub
			{
				AutoSize = option,
				Text = "Test"
			};

			await ValidatePropertyInitValue(editor, () => editor.AutoSize, GetNativeAutoSize, editor.AutoSize);
		}
	}
}
