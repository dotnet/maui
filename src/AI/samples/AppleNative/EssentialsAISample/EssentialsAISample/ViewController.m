//
//  ViewController.m
//  EssentialsAISample
//
//  Created by Matthew Leibowitz on 2025/11/17.
//

#import "ViewController.h"

#import "EssentialsAI/EssentialsAI-Swift.h"

@interface ViewController ()

@end

@implementation ViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view.
    
    // 1. Create the client
    ChatClientNative *client = [[ChatClientNative alloc] init];
    
    // 2. Create a text content
    TextContentNative *textContent = [[TextContentNative alloc] initWithText:@"Hello, how are you?"];
    
    // 3. Create the message and attach the content
    ChatMessageNative *message = [[ChatMessageNative alloc] init];
    message.role = ChatRoleNativeUser;
    message.contents = @[ textContent ];
    
    // 4. (Optional) create options
    ChatOptionsNative *options = [[ChatOptionsNative alloc] init];
    options.temperature = @(0.7);
    options.maxOutputTokens = @(256);
    
    // 5. Call getResponse
    CancellationTokenNative *token = [client getResponseWithMessages:@[message]
                            options:options
                         onComplete:^(NSObject * _Nullable result, NSError * _Nullable error) {
        if (error != nil) {
            NSLog(@"Error from AI: %@", error);
            return;
        }
        
        if ([result isKindOfClass:NSString.class]) {
            NSString *responseText = (NSString *)result;
            NSLog(@"AI Response: %@", responseText);
        } else {
            NSLog(@"Unexpected result type: %@", result);
        }
    }];
    
//    [token cancel];

}


@end
