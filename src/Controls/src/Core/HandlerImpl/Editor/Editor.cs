using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{		

		public static IPropertyMapper<Editor, EditorHandler> ControlsEditorMapper = new PropertyMapper<Editor, EditorHandler>(EditorHandler.EditorMapper)
		{
			[nameof(AutoSize)] = MapAutoSize
		};

		public new static void RemapForControls()
		{
			EditorHandler.EditorMapper = ControlsEditorMapper;
		}

		public static void MapAutoSize(EditorHandler handler, Editor editor)
		{
			
		}
	}
}