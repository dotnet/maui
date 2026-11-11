using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Android.OS;
using Android.Util;
using Microsoft.Maui.Controls;

namespace MauiPackageSkills.Evaluation;

internal static class ShellObservation
{
	static readonly Handler Dispatcher = new(Looper.MainLooper!);

	[ModuleInitializer]
	internal static void Initialize()
	{
		Dispatcher.PostDelayed(Capture, 500);
	}

	static void Capture()
	{
		var shell = Shell.Current;
		if (shell?.Handler?.PlatformView is Android.Views.View platform && platform.IsAttachedToWindow)
		{
			Log.Info("MauiSkillsProbe",
				"run=__RUN_TOKEN__;handler=" + Chain(shell.Handler) +
				";platform=" + Chain(platform) +
				";assembly=" + typeof(Shell).Assembly.GetName().Version);
		}
		Dispatcher.PostDelayed(Capture, 500);
	}

	static string Chain(object value)
	{
		var names = new List<string>();
		for (Type? type = value.GetType(); type is not null; type = type.BaseType)
			names.Add((type.FullName ?? type.Name) + "," + type.Assembly.GetName().Name);
		return string.Join("|", names);
	}
}
