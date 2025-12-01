//
//  ViewController.m
//  EssentialsAISample
//
//  Created by Matthew Leibowitz on 2025/11/17.
//

#import "ViewController.h"

#import "EssentialsAI/EssentialsAI-Swift.h"

// Block typedefs for AI callbacks
typedef void (^AIUpdateBlock)(StreamUpdateNative *_Nullable result);
typedef void (^AICompletionBlock)(NSString *_Nullable result,
                                  NSError *_Nullable error);

// Simple test tool that returns the current time
@interface GetCurrentTimeTool : NSObject <AIToolNative>
@end

@implementation GetCurrentTimeTool

- (NSString *)name {
    return @"getCurrentTime";
}

- (NSString *)desc {
    return @"Gets the current date and time. No parameters needed.";
}

- (NSString *)argumentsSchema {
    return @"{\"type\":\"object\",\"properties\":{}}";
}

- (void)callWithArguments:(NSString *)arguments
               completion:(void (^)(NSString *))completion {
    NSDateFormatter *formatter = [[NSDateFormatter alloc] init];
    formatter.dateFormat = @"yyyy-MM-dd HH:mm:ss";
    NSString *currentTime = [formatter stringFromDate:[NSDate date]];

    NSString *result =
        [NSString stringWithFormat:@"{\"currentTime\":\"%@\"}", currentTime];
    completion(result);
}

@end

@interface ViewController ()

@end

@implementation ViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view.

    // 1. Create the client
    ChatClientNative *client = [[ChatClientNative alloc] init];

    // 2. Create a text content
    TextContentNative *textContent =
        [[TextContentNative alloc] initWithText:@"What time is it right now?"];

    // 3. Create the message and attach the content
    ChatMessageNative *message = [[ChatMessageNative alloc] init];
    message.role = ChatRoleNativeUser;
    message.contents = @[ textContent ];

    // 4. (Optional) create options
    ChatOptionsNative *options = [[ChatOptionsNative alloc] init];
    options.temperature = @(0.7);
    options.maxOutputTokens = @(256);
    options.responseJsonSchema = @"{"
        "\"$schema\":\"https://json-schema.org/draft/2020-12/schema\","
        "\"title\":\"ReplyWithHappiness\","
        "\"type\":\"object\","
        "\"properties\":{"
            "\"reply\":{"
                "\"type\":\"string\","
                "\"description\":\"Some string message returned by the system\""
            "},"
            "\"happiness\":{"
                "\"type\":\"number\","
                "\"description\":\"A numeric happiness score\""
            "},"
            "\"alternate-reply\":{"
                "\"type\":\"string\","
                "\"description\":\"Some alternate string message returned by the system\""
            "},"
            "\"anger\":{"
                "\"type\":\"number\","
                "\"description\":\"A numeric anger score\""
            "}"
        "},"
        "\"required\":[\"reply\",\"happiness\",\"alternate-reply\",\"anger\"],"
        "\"x-order\":[\"anger\",\"reply\",\"happiness\",\"alternate-reply\"],"
        "\"additionalProperties\":false"
    "}";

    // Create a test tool instance
    GetCurrentTimeTool *timeTool = [[GetCurrentTimeTool alloc] init];
    options.tools = @[ timeTool ];

    // 5. Call getResponse

    AICompletionBlock completion =
        ^(NSString *_Nullable result, NSError *_Nullable error) {
          if (error) {
              NSLog(@"Error from AI: %@", error);
              return;
          }
          NSLog(@"AI Response: %@", result);
        };

    // Define reusable update block
    AIUpdateBlock update = ^(StreamUpdateNative *_Nullable result) {
      NSLog(@"Stream update: text = %@", [result text]);
      NSLog(@"Stream update: tool = %@", [result toolCallName]);
      NSLog(@"Stream update: tool result = %@", [result toolCallResult]);
    };

    // Use typedef blocks for getResponse
    CancellationTokenNative *token =
        [client getResponseWithMessages:@[ message ]
                                options:options
                             onComplete:completion];

    // Use typedef blocks for streamResponse
    CancellationTokenNative *streamToken =
        [client streamResponseWithMessages:@[ message ]
                                   options:options
                                  onUpdate:update
                                onComplete:completion];

    //    [token cancel];
}

@end
