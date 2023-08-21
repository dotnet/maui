// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.Maui
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class CustomKeyboard : Keyboard
	{
		internal CustomKeyboard(KeyboardFlags flags)
		{
			Flags = flags;
		}


		public KeyboardFlags Flags { get; private set; }
	}
}