#nullable enable

using System;
using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui.Essentials.AI;

// @interface AIContentNative : NSObject
[Introduced(PlatformName.iOS, 26, 0)]
[Introduced(PlatformName.MacCatalyst, 26, 0)]
[Introduced(PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType(typeof(NSObject))]
[Internal]
interface AIContentNative
{
}

// This is essential to keep as we need to reference IAIToolNative in this file
interface IAIToolNative { }

// @protocol AIToolNative
[Introduced(PlatformName.iOS, 26, 0)]
[Introduced(PlatformName.MacCatalyst, 26, 0)]
[Introduced(PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[Protocol, Model]
[BaseType(typeof(NSObject))]
[Internal]
interface AIToolNative
{
	// @property (nonatomic, readonly, copy) NSString * _Nonnull name;
	[Abstract]
	[Export("name")]
	string Name { get; }

	// @property (nonatomic, readonly, copy) NSString * _Nonnull desc;
	[Abstract]
	[Export("desc")]
	string Desc { get; }

	// @property (nonatomic, readonly, copy) NSString * _Nonnull argumentsSchema;
	[Abstract]
	[Export("argumentsSchema")]
	string ArgumentsSchema { get; }

	// @property (nonatomic, readonly, copy) NSString * _Nonnull outputSchema;
	[Abstract]
	[Export("outputSchema")]
	string OutputSchema { get; }

	// - (void)callWithArguments:(NSString * _Nonnull)arguments completion:(void (^ _Nonnull)(NSString * _Nonnull))completion;
	[Abstract]
	[Export("callWithArguments:completion:")]
	void CallWithArguments(NSString arguments, Action<NSString> completion);
}

// @interface CancellationTokenNative : NSObject
[Introduced(PlatformName.iOS, 26, 0)]
[Introduced(PlatformName.MacCatalyst, 26, 0)]
[Introduced(PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType(typeof(NSObject))]
[DisableDefaultCtor]
[Internal]
interface CancellationTokenNative
{
	// - (void)cancel;
	[Export("cancel")]
	void Cancel();

	// @property (nonatomic, readonly) BOOL isCancelled;
	[Export("isCancelled")]
	bool IsCancelled { get; }
}

[Internal] delegate void OnResponseUpdate(ResponseUpdateNative update);

[Internal] delegate void OnResponseComplete([NullAllowed] ChatResponseNative response, [NullAllowed] NSError error);

// @interface ChatClientNative : NSObject
[Introduced(PlatformName.iOS, 26, 0)]
[Introduced(PlatformName.MacCatalyst, 26, 0)]
[Introduced(PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType(typeof(NSObject))]
[Internal]
interface ChatClientNative
{
	// - (CancellationTokenNative * _Nullable)streamResponseWithMessages:(NSArray<ChatMessageNative *> * _Nonnull)messages options:(ChatOptionsNative * _Nullable)options onUpdate:(void (^ _Nonnull)(ResponseUpdateNative * _Nonnull))onUpdate onComplete:(void (^ _Nonnull)(ChatResponseNative * _Nullable, NSError * _Nullable))onComplete SWIFT_WARN_UNUSED_RESULT;
	[Export("streamResponseWithMessages:options:onUpdate:onComplete:")]
	[return: NullAllowed]
	unsafe CancellationTokenNative StreamResponse(ChatMessageNative[] messages, [NullAllowed] ChatOptionsNative options, OnResponseUpdate onUpdate, OnResponseComplete onComplete);

	// - (CancellationTokenNative * _Nullable)getResponseWithMessages:(NSArray<ChatMessageNative *> * _Nonnull)messages options:(ChatOptionsNative * _Nullable)options onUpdate:(void (^ _Nonnull)(ResponseUpdateNative * _Nonnull))onUpdate onComplete:(void (^ _Nonnull)(ChatResponseNative * _Nullable, NSError * _Nullable))onComplete SWIFT_WARN_UNUSED_RESULT;
	[Export("getResponseWithMessages:options:onUpdate:onComplete:")]
	[return: NullAllowed]
	unsafe CancellationTokenNative GetResponse(ChatMessageNative[] messages, [NullAllowed] ChatOptionsNative options, OnResponseUpdate onUpdate, OnResponseComplete onComplete);
}

// @interface ChatMessageNative : NSObject
[Introduced(PlatformName.iOS, 26, 0)]
[Introduced(PlatformName.MacCatalyst, 26, 0)]
[Introduced(PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType(typeof(NSObject))]
[Internal]
interface ChatMessageNative
{
	// @property (nonatomic) enum ChatRoleNative role;
	[Export("role", ArgumentSemantic.Assign)]
	ChatRoleNative Role { get; set; }

	// @property (nonatomic, copy) NSArray<AIContentNative *> * _Nonnull contents;
	[Export("contents", ArgumentSemantic.Copy)]
	AIContentNative[] Contents { get; set; }
}

// @interface ChatOptionsNative : NSObject
[Introduced(PlatformName.iOS, 26, 0)]
[Introduced(PlatformName.MacCatalyst, 26, 0)]
[Introduced(PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType(typeof(NSObject))]
[Internal]
interface ChatOptionsNative
{
	// @property (nonatomic, strong) NSNumber * _Nullable topK;
	[NullAllowed, Export("topK", ArgumentSemantic.Strong)]
	NSNumber TopK { get; set; }

	// @property (nonatomic, strong) NSNumber * _Nullable seed;
	[NullAllowed, Export("seed", ArgumentSemantic.Strong)]
	NSNumber Seed { get; set; }

	// @property (nonatomic, strong) NSNumber * _Nullable temperature;
	[NullAllowed, Export("temperature", ArgumentSemantic.Strong)]
	NSNumber Temperature { get; set; }

	// @property (nonatomic, strong) NSNumber * _Nullable maxOutputTokens;
	[NullAllowed, Export("maxOutputTokens", ArgumentSemantic.Strong)]
	NSNumber MaxOutputTokens { get; set; }

	// @property (nonatomic, strong) NSString * _Nullable responseJsonSchema;
	[NullAllowed, Export("responseJsonSchema", ArgumentSemantic.Strong)]
	NSString ResponseJsonSchema { get; set; }

	// @property (nonatomic, copy) NSArray<id <AIToolNative>> * _Nullable tools;
	[NullAllowed, Export("tools", ArgumentSemantic.Copy)]
	IAIToolNative[] Tools { get; set; }
}

// @interface ChatResponseNative : NSObject
[Introduced(PlatformName.iOS, 26, 0)]
[Introduced(PlatformName.MacCatalyst, 26, 0)]
[Introduced(PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType(typeof(NSObject))]
[DisableDefaultCtor]
[Internal]
interface ChatResponseNative
{
	// @property (nonatomic, copy) NSArray<ChatMessageNative *> * _Nonnull messages;
	[Export("messages", ArgumentSemantic.Copy)]
	ChatMessageNative[] Messages { get; set; }

	// - (nonnull instancetype)initWithMessages:(NSArray<ChatMessageNative *> * _Nonnull)messages OBJC_DESIGNATED_INITIALIZER;
	[Export("initWithMessages:")]
	[DesignatedInitializer]
	NativeHandle Constructor(ChatMessageNative[] messages);
}

// @interface FunctionCallContentNative : AIContentNative
[BaseType(typeof(AIContentNative))]
[DisableDefaultCtor]
[Internal]
interface FunctionCallContentNative
{
	// @property (nonatomic, copy) NSString * _Nonnull callId;
	[Export("callId", ArgumentSemantic.Copy)]
	string CallId { get; set; }

	// @property (nonatomic, copy) NSString * _Nonnull name;
	[Export("name", ArgumentSemantic.Copy)]
	string Name { get; set; }

	// @property (nonatomic, copy) NSString * _Nonnull arguments;
	[Export("arguments", ArgumentSemantic.Copy)]
	string Arguments { get; set; }

	// - (nonnull instancetype)initWithCallId:(NSString * _Nonnull)callId name:(NSString * _Nonnull)name arguments:(NSString * _Nonnull)arguments OBJC_DESIGNATED_INITIALIZER;
	[Export("initWithCallId:name:arguments:")]
	[DesignatedInitializer]
	NativeHandle Constructor(string callId, string name, string arguments);
}

// @interface FunctionResultContentNative : AIContentNative
[BaseType(typeof(AIContentNative))]
[DisableDefaultCtor]
[Internal]
interface FunctionResultContentNative
{
	// @property (nonatomic, copy) NSString * _Nonnull callId;
	[Export("callId", ArgumentSemantic.Copy)]
	string CallId { get; set; }

	// @property (nonatomic, copy) NSString * _Nonnull result;
	[Export("result", ArgumentSemantic.Copy)]
	string Result { get; set; }

	// - (nonnull instancetype)initWithCallId:(NSString * _Nonnull)callId result:(NSString * _Nonnull)result OBJC_DESIGNATED_INITIALIZER;
	[Export("initWithCallId:result:")]
	[DesignatedInitializer]
	NativeHandle Constructor(string callId, string result);
}

// @interface TextContentNative : AIContentNative
[BaseType(typeof(AIContentNative))]
[DisableDefaultCtor]
[Internal]
interface TextContentNative
{
	// - (nonnull instancetype)initWithText:(NSString * _Nonnull)text OBJC_DESIGNATED_INITIALIZER;
	[Export("initWithText:")]
	[DesignatedInitializer]
	NativeHandle Constructor(string text);

	// @property (nonatomic, copy) NSString * _Nonnull text;
	[Export("text")]
	string Text { get; set; }
}

// @interface ResponseUpdateNative : NSObject
[Introduced(PlatformName.iOS, 26, 0)]
[Introduced(PlatformName.MacCatalyst, 26, 0)]
[Introduced(PlatformName.MacOSX, 26, 0)]
// [Introduced (PlatformName.VisionOS, 26, 0)]
[BaseType(typeof(NSObject))]
[DisableDefaultCtor]
[Internal]
interface ResponseUpdateNative
{
	// @property (nonatomic, readonly, copy) NSString * _Nullable text;
	[NullAllowed, Export("text")]
	string Text { get; }

	// @property (nonatomic, readonly, copy) NSString * _Nullable toolCallId;
	[NullAllowed, Export("toolCallId")]
	string ToolCallId { get; }

	// @property (nonatomic, readonly, copy) NSString * _Nullable toolCallName;
	[NullAllowed, Export("toolCallName")]
	string ToolCallName { get; }

	// @property (nonatomic, readonly, copy) NSString * _Nullable toolCallArguments;
	[NullAllowed, Export("toolCallArguments")]
	string ToolCallArguments { get; }

	// @property (nonatomic, readonly, copy) NSString * _Nullable toolCallResult;
	[NullAllowed, Export("toolCallResult")]
	string ToolCallResult { get; }
}
