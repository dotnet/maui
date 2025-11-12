#nullable enable

using System;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Essentials.AI;

// @interface AIContentNative
[Introduced (PlatformName.iOS, 26, 0)]
[Introduced (PlatformName.MacCatalyst, 26, 0)]
[Introduced (PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
[Internal] interface AIContentNative
{
}

// @interface CancellationTokenNative
[Introduced (PlatformName.iOS, 26, 0)]
[Introduced (PlatformName.MacCatalyst, 26, 0)]
[Introduced (PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType (typeof(NSObject))]
[DisableDefaultCtor]
[Internal] interface CancellationTokenNative
{
	// -(void)cancel;
	[Export ("cancel")]
	void Cancel ();

	// @property (readonly, nonatomic) int isCancelled;
	[Export ("isCancelled")]
	int IsCancelled { get; }
}

[Internal] delegate void OnGetResponseComplete ([NullAllowed] NSString response, [NullAllowed] NSError error);

[Internal] delegate void OnStreamUpdate ([NullAllowed] NSString update);

[Internal] delegate void OnStreamComplete ([NullAllowed] NSString finalResult, [NullAllowed] NSError error);

// @interface ChatClientNative
[Introduced (PlatformName.iOS, 26, 0)]
[Introduced (PlatformName.MacCatalyst, 26, 0)]
[Introduced (PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType (typeof(NSObject))]
[Internal] interface ChatClientNative
{
	// - (CancellationTokenNative * _Nullable)streamResponseWithMessages:(NSArray<ChatMessageNative *> * _Nonnull)messages options:(ChatOptionsNative * _Nullable)options onUpdate:(void (^ _Nonnull)(NSString * _Nonnull))onUpdate onComplete:(void (^ _Nonnull)(NSString * _Nullable, NSError * _Nullable))onComplete SWIFT_WARN_UNUSED_RESULT;
	[Export ("streamResponseWithMessages:options:onUpdate:onComplete:")]
	[return: NullAllowed]
	unsafe CancellationTokenNative StreamResponse (ChatMessageNative[] messages, [NullAllowed] ChatOptionsNative options, OnStreamUpdate onUpdate, OnStreamComplete onComplete);

	// - (CancellationTokenNative * _Nullable)getResponseWithMessages:(NSArray<ChatMessageNative *> * _Nonnull)messages options:(ChatOptionsNative * _Nullable)options onComplete:(void (^ _Nonnull)(NSObject * _Nullable, NSError * _Nullable))onComplete SWIFT_WARN_UNUSED_RESULT;
	[Export ("getResponseWithMessages:options:onComplete:")]
	[return: NullAllowed]
	unsafe CancellationTokenNative GetResponse (ChatMessageNative[] messages, [NullAllowed] ChatOptionsNative options, OnGetResponseComplete onComplete);
}

// @interface ChatMessageNative
[Introduced (PlatformName.iOS, 26, 0)]
[Introduced (PlatformName.MacCatalyst, 26, 0)]
[Introduced (PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType (typeof(NSObject))]
[Internal] interface ChatMessageNative
{
	// @property (nonatomic) enum ChatRoleNative role;
	[Export ("role", ArgumentSemantic.Assign)]
	ChatRoleNative Role { get; set; }

	// @property (nonatomic, copy) NSArray<AIContentNative *> * _Nonnull contents;
	[Export ("contents", ArgumentSemantic.Copy)]
	AIContentNative[] Contents { get; set; }
}

// @interface ChatOptionsNative
[Introduced (PlatformName.iOS, 26, 0)]
[Introduced (PlatformName.MacCatalyst, 26, 0)]
[Introduced (PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType (typeof(NSObject))]
[Internal] interface ChatOptionsNative
{
	// @property (nonatomic, strong) NSNumber * _Nullable topK;
	[NullAllowed, Export ("topK", ArgumentSemantic.Strong)]
	NSNumber TopK { get; set; }

	// @property (nonatomic, strong) NSNumber * _Nullable seed;
	[NullAllowed, Export ("seed", ArgumentSemantic.Strong)]
	NSNumber Seed { get; set; }

	// @property (nonatomic, strong) NSNumber * _Nullable temperature;
	[NullAllowed, Export ("temperature", ArgumentSemantic.Strong)]
	NSNumber Temperature { get; set; }

	// @property (nonatomic, strong) NSNumber * _Nullable maxOutputTokens;
	[NullAllowed, Export ("maxOutputTokens", ArgumentSemantic.Strong)]
	NSNumber MaxOutputTokens { get; set; }

	// @property (nonatomic, strong) NSString * _Nullable responseJsonSchema;
	[NullAllowed, Export ("responseJsonSchema", ArgumentSemantic.Strong)]
	NSString ResponseJsonSchema { get; set; }
}

// @interface TextContentNative : AIContentNative
[BaseType (typeof(AIContentNative))]
[DisableDefaultCtor]
[Internal] interface TextContentNative
{
	// -(instancetype _Nonnull)initWithText:(NSString * _Nonnull)text __attribute__((objc_designated_initializer));
	[Export ("initWithText:")]
	[DesignatedInitializer]
	NativeHandle Constructor (string text);

	// @property (copy, nonatomic) NSString * _Nonnull text;
	[Export ("text")]
	string Text { get; set; }
}
