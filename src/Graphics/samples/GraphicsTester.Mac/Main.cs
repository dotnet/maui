using System;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace GraphicsTester.Mac
{
	class MainClass
	{
		static void Main(string[] args)
		{
			NSApplication.Init();
			NSApplication.Main(args);
		}
	}
}
