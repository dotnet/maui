// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Controls.Xaml
{
	internal interface IExpressionParser
	{
		object Parse(string match, ref string expression, IServiceProvider serviceProvider);
	}

	internal interface IExpressionParser<out T> : IExpressionParser
		where T : class
	{
		new T Parse(string match, ref string expression, IServiceProvider serviceProvider);
	}
}