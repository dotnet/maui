using System;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Graphics.CoreGraphics
{


	public static class AppKitConstants
	{
		static readonly IntPtr AppKitLibraryHandle = Dlfcn.dlopen(Constants.AppKitLibrary, 0);

		static NSString _printPanelAccessorySummaryItemNameKey;
		static NSString _printPanelAccessorySummaryItemDescriptionKey;
		static NSString _imageNameUserAccounts;
		static NSString _imageNamePreferencesGeneral;
		static NSString _imageNameAdvanced;
		static NSString _imageCompressionFactor;

		public static NSString NSPrintPanelAccessorySummaryItemNameKey => _printPanelAccessorySummaryItemNameKey ??
																		  (_printPanelAccessorySummaryItemNameKey =
																			  Dlfcn.GetStringConstant(AppKitLibraryHandle, "NSPrintPanelAccessorySummaryItemNameKey"));

		public static NSString NSPrintPanelAccessorySummaryItemDescriptionKey => _printPanelAccessorySummaryItemDescriptionKey ??
																				 (_printPanelAccessorySummaryItemDescriptionKey = Dlfcn.GetStringConstant(AppKitLibraryHandle,
																					 "NSPrintPanelAccessorySummaryItemDescriptionKey"));

		public static NSString NSImageNameUserAccounts => _imageNameUserAccounts ?? (_imageNameUserAccounts = Dlfcn.GetStringConstant(AppKitLibraryHandle, "NSImageNameUserAccounts"));

		public static NSString NSImageNamePreferencesGeneral =>
			_imageNamePreferencesGeneral ?? (_imageNamePreferencesGeneral = Dlfcn.GetStringConstant(AppKitLibraryHandle, "NSImageNamePreferencesGeneral"));

		public static NSString NSImageNameAdvanced => _imageNameAdvanced ?? (_imageNameAdvanced = Dlfcn.GetStringConstant(AppKitLibraryHandle, "NSImageNameAdvanced"));

		public static NSString NSImageCompressionFactor => _imageCompressionFactor ?? (_imageCompressionFactor = Dlfcn.GetStringConstant(AppKitLibraryHandle, "NSImageCompressionFactor"));
	}
}
