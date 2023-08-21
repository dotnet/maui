// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.AppiumTests
{
	internal abstract class TypeConverter
	{
		public abstract bool CanConvertTo(object source, Type targetType);

		public abstract object ConvertTo(object source, Type targetType);
	}
}