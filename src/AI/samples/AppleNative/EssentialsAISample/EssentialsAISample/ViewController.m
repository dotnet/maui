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
typedef void (^AICompletionBlock)(ChatResponseNative *_Nullable result,
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

// Weather tool that returns dummy weather for a specific date
@interface GetWeatherTool : NSObject <AIToolNative>
@end

@implementation GetWeatherTool

- (NSString *)name {
    return @"getWeather";
}

- (NSString *)desc {
    return @"Gets the weather forecast for a specific date. Requires the date in YYYY-MM-DD format.";
}

- (NSString *)argumentsSchema {
    return @"{"
        "\"type\":\"object\","
        "\"properties\":{"
            "\"date\":{"
                "\"type\":\"string\","
                "\"description\":\"The date to get weather for in YYYY-MM-DD format\""
            "}"
        "},"
        "\"required\":[\"date\"]"
    "}";
}

- (void)callWithArguments:(NSString *)arguments
               completion:(void (^)(NSString *))completion {
    // Parse the arguments to extract the date
    NSError *error = nil;
    NSDictionary *argsDict = [NSJSONSerialization JSONObjectWithData:[arguments dataUsingEncoding:NSUTF8StringEncoding]
                                                             options:0
                                                               error:&error];
    
    NSString *dateString = argsDict[@"date"];
    if (!dateString) {
        dateString = @"unknown";
    }
    
    // Generate dummy weather data
    NSArray *conditions = @[@"sunny", @"cloudy", @"rainy", @"partly cloudy", @"windy"];
    NSString *condition = conditions[arc4random_uniform((uint32_t)conditions.count)];
    NSInteger temperature = 60 + (arc4random_uniform(30)); // 60-89°F
    NSInteger humidity = 30 + (arc4random_uniform(50)); // 30-79%
    
    NSString *result = [NSString stringWithFormat:
        @"{\"date\":\"%@\",\"condition\":\"%@\",\"temperature\":%ld,\"humidity\":%ld}",
        dateString, condition, (long)temperature, (long)humidity];
    
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
        [[TextContentNative alloc] initWithText:@"What's the weather like today?"];

    // 3. Create the message and attach the content
    ChatMessageNative *message = [[ChatMessageNative alloc] init];
    message.role = ChatRoleNativeUser;
    message.contents = @[ textContent ];

    // 4. (Optional) create options
    ChatOptionsNative *options = [[ChatOptionsNative alloc] init];
    options.temperature = @(0.7);
    options.maxOutputTokens = @(512);
    // Remove JSON schema to allow natural language responses
    options.responseJsonSchema = nil;

    // Create tool instances - the AI should call getCurrentTime first, 
    // then use that date to call getWeather
    GetCurrentTimeTool *timeTool = [[GetCurrentTimeTool alloc] init];
    GetWeatherTool *weatherTool = [[GetWeatherTool alloc] init];
    options.tools = @[ timeTool, weatherTool ];

    // 5. Call getResponse

    AICompletionBlock completion =
        ^(ChatResponseNative *_Nullable result, NSError *_Nullable error) {
          if (error) {
              NSLog(@"Error from AI: %@", error);
              return;
          }
          
          // Log the response with all messages
          NSLog(@"AI Response received with %lu messages", (unsigned long)result.messages.count);
          
          for (ChatMessageNative *message in result.messages) {
              NSLog(@"Message role: %ld", (long)message.role);
              
              for (AIContentNative *content in message.contents) {
                  if ([content isKindOfClass:[TextContentNative class]]) {
                      TextContentNative *textContent = (TextContentNative *)content;
                      NSLog(@"  Text: %@", textContent.text);
                  } else if ([content isKindOfClass:[FunctionCallContentNative class]]) {
                      FunctionCallContentNative *funcCall = (FunctionCallContentNative *)content;
                      NSLog(@"  Function Call: %@ (%@)", funcCall.name, funcCall.callId);
                      NSLog(@"    Arguments: %@", funcCall.arguments);
                  } else if ([content isKindOfClass:[FunctionResultContentNative class]]) {
                      FunctionResultContentNative *funcResult = (FunctionResultContentNative *)content;
                      NSLog(@"  Function Result (%@): %@", funcResult.callId, funcResult.result);
                  }
              }
          }
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
