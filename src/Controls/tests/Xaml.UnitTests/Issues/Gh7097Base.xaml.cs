// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh7097Base : ContentPage
	{
		public Gh7097Base() => InitializeComponent();
		public Gh7097Base(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
	}
}
