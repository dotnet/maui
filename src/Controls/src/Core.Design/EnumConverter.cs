// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Generic version of the <see cref="EnumConverter"/> for reuse.
	/// </summary>
	internal class EnumConverter<T> : EnumConverter
	{
		public EnumConverter() : base(typeof(T))
		{
		}
	}
}
