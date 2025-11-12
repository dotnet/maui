//
//  ViewController.m
//  EssentialsAISample
//
//  Created by Matthew Leibowitz on 2025/11/17.
//

#import "ViewController.h"

#import "EssentialsAI/EssentialsAI-Swift.h"

// Block typedefs for AI callbacks
typedef void (^AIUpdateBlock)(NSString *_Nullable result);
typedef void (^AICompletionBlock)(NSString *_Nullable result,
                                  NSError *_Nullable error);

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
        [[TextContentNative alloc] initWithText:@"Hello, how are you?"];

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
    AIUpdateBlock update = ^(NSString *_Nullable result) {
      NSLog(@"Stream update: %@", result);
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
