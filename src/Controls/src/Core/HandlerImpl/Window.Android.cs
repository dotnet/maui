#nullable enable

using System;
using AndroidX.AppCompat.App;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal AppCompatActivity NativeActivity
		{
			get => MauiContext.Context?.GetActivity() as AppCompatActivity ?? throw new InvalidOperationException("Root Activity should not be null here");
		}
	}
}
