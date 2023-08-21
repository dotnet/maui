// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IExpressionSearch
	{
		List<T> FindObjects<T>(Expression expression) where T : class;
	}
}