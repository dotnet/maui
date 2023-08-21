// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class XamlFilePathAttribute : Attribute
	{
		public XamlFilePathAttribute([CallerFilePath] string filePath = "") => FilePath = filePath;

		public string FilePath { get; }

		internal static string GetFilePathForObject(object view) => (view?.GetType().GetCustomAttributes(typeof(XamlFilePathAttribute), false).FirstOrDefault() as XamlFilePathAttribute)?.FilePath;
	}
}