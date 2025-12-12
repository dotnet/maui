//
//  ViewController.m
//  EssentialsAISample
//
//  Created by Matthew Leibowitz on 2025/11/17.
//

#import "ViewController.h"

#import "EssentialsAI/EssentialsAI-Swift.h"

// Block typedefs for AI callbacks
typedef void (^AIUpdateBlock)(ResponseUpdateNative *_Nullable result);
typedef void (^AICompletionBlock)(ChatResponseNative *_Nullable result,
                                  NSError *_Nullable error);

#pragma mark - Tools

// Find points of interest tool for Maui itinerary
@interface FindPointsOfInterestTool : NSObject <AIToolNative>
@end

@implementation FindPointsOfInterestTool

- (NSString *)name {
    return @"findPointsOfInterest";
}

- (NSString *)desc {
    return @"Finds points of interest for a landmark.";
}

- (NSString *)argumentsSchema {
    return @"{"
        "\"type\":\"object\","
        "\"properties\":{"
            "\"pointOfInterest\":{"
                "\"description\":\"This is the type of destination to look up for.\","
                "\"type\":\"string\","
                "\"enum\":[\"Cafe\",\"Campground\",\"Hotel\",\"Marina\",\"Museum\",\"NationalMonument\",\"Restaurant\"]"
            "},"
            "\"naturalLanguageQuery\":{"
                "\"description\":\"The natural language query of what to search for.\","
                "\"type\":\"string\""
            "}"
        "},"
        "\"required\":[\"pointOfInterest\",\"naturalLanguageQuery\"]"
    "}";
}

- (NSString *)outputSchema {
    return @"{\"type\":\"string\"}";
}

- (void)callWithArguments:(NSString *)arguments
               completion:(void (^)(NSString *))completion {
    // Parse the arguments
    NSError *error = nil;
    NSDictionary *argsDict = [NSJSONSerialization JSONObjectWithData:[arguments dataUsingEncoding:NSUTF8StringEncoding]
                                                             options:0
                                                               error:&error];
    
    NSString *poiType = argsDict[@"pointOfInterest"] ?: @"unknown";
    
    // Generate results matching the C# implementation format
    NSString *result;
    if ([poiType isEqualToString:@"Cafe"]) {
        result = @"There are these Cafe in Maui: Cafe 1, Cafe 2, Cafe 3";
    } else if ([poiType isEqualToString:@"Campground"]) {
        result = @"There are these Campground in Maui: Campground 1, Campground 2, Campground 3";
    } else if ([poiType isEqualToString:@"Hotel"]) {
        result = @"There are these Hotel in Maui: Hotel 1, Hotel 2, Hotel 3";
    } else if ([poiType isEqualToString:@"Marina"]) {
        result = @"There are these Marina in Maui: Marina 1, Marina 2, Marina 3";
    } else if ([poiType isEqualToString:@"Museum"]) {
        result = @"There are these Museum in Maui: Museum 1, Museum 2, Museum 3";
    } else if ([poiType isEqualToString:@"NationalMonument"]) {
        result = @"There are these NationalMonument in Maui: The National Rock 1, The National Rock 2, The National Rock 3";
    } else if ([poiType isEqualToString:@"Restaurant"]) {
        result = @"There are these Restaurant in Maui: Restaurant 1, Restaurant 2, Restaurant 3";
    } else {
        result = [NSString stringWithFormat:@"There are no %@ in Maui", poiType];
    }
    
    completion(result);
}

@end

#pragma mark - View Controller

@implementation ViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    // Set up logging using simple block
    AppleIntelligenceLogger.log = ^(NSString *message) {
        NSLog(@"[Native] %@", message);
    };
    
    // 1. Create the client
    ChatClientNative *client = [[ChatClientNative alloc] init];

    // 2. Create system message with Maui description
    ChatMessageNative *systemMessage = [[ChatMessageNative alloc] init];
    systemMessage.role = ChatRoleNativeSystem;
    systemMessage.contents = @[
        [[TextContentNative alloc] initWithText:@"Your job is to create an itinerary for the person."],
        [[TextContentNative alloc] initWithText:@"Each day needs an activity, hotel and restaurant."],
        [[TextContentNative alloc] initWithText:@"Always use the findPointsOfInterest tool to find businesses and activities in Maui, especially hotels and restaurants.\n\nThe point of interest categories may include:"],
        [[TextContentNative alloc] initWithText:@"Cafe, Campground, Hotel, Marina, Museum, NationalMonument, Restaurant"],
        [[TextContentNative alloc] initWithText:@"Here is a description of Maui for your reference when considering what activities to generate:"],
        [[TextContentNative alloc] initWithText:@"The second-largest island in Hawaii, Maui offers a stunning tapestry of volcanic landscapes, lush rainforests, pristine beaches, and dramatic coastal cliffs. Known as the \"Valley Isle,\" Maui is dominated by two massive volcanoes: the dormant Haleakalā in the east and the older, eroded West Maui Mountains. Haleakalā National Park features a massive volcanic crater that reaches over 10,000 feet in elevation, offering breathtaking sunrise views and unique high-altitude ecosystems.\n\nThe island's diverse geography creates distinct climate zones, from arid leeward coasts to verdant windward rainforests receiving over 400 inches of annual rainfall. The famous Road to Hana winds through tropical paradise, passing countless waterfalls, bamboo forests, and dramatic ocean vistas. Maui's volcanic soil supports rich agriculture, including sugarcane, pineapple, coffee, and exotic tropical fruits.\n\nMarine life thrives in Maui's warm Pacific waters. Humpback whales migrate to the shallow channels between the Hawaiian islands from December to May, making Maui one of the world's premier whale-watching destinations. Hawaiian green sea turtles, spinner dolphins, and vibrant coral reefs attract snorkelers and divers year-round. The island's beaches range from golden sand to unique red and black volcanic shores. Native Hawaiian plants such as the silversword, which grows only on Haleakalā's slopes, and endemic bird species like the nēnē (Hawaiian goose) highlight the island's ecological significance and ongoing conservation efforts."]
    ];
    
    // 3. Create user message
    ChatMessageNative *userMessage = [[ChatMessageNative alloc] init];
    userMessage.role = ChatRoleNativeUser;
    userMessage.contents = @[
        [[TextContentNative alloc] initWithText:@"Generate a 3-day itinerary to Maui."],
        [[TextContentNative alloc] initWithText:@"Give it a fun title and description."],
        [[TextContentNative alloc] initWithText:@"Here is an example, but don't copy it:"],
        [[TextContentNative alloc] initWithText:@"{\"title\":\"Onsen Trip to Japan\",\"destinationName\":\"Mt. Fuji\",\"description\":\"Sushi, hot springs, and ryokan with a toddler!\",\"rationale\":\"You are traveling with a child, so climbing Mt. Fuji is probably not an option,\nbut there is lots to do around Kawaguchiko Lake, including Fujikyu.\nI recommend staying in a ryokan because you love hotsprings.\",\"days\":[{\"title\":\"Sushi and Shopping Near Kawaguchiko\",\"subtitle\":\"Spend your final day enjoying sushi and souvenir shopping.\",\"destination\":\"Kawaguchiko Lake\",\"activities\":[{\"type\":\"FoodAndDining\",\"title\":\"The Restaurant serving Sushi\",\"description\":\"Visit an authentic sushi restaurant for lunch.\"},{\"type\":\"Shopping\",\"title\":\"The Plaza\",\"description\":\"Enjoy souvenir shopping at various shops.\"},{\"type\":\"Sightseeing\",\"title\":\"The Beautiful Cherry Blossom Park\",\"description\":\"Admire the beautiful cherry blossom trees in the park.\"},{\"type\":\"HotelAndLodging\",\"title\":\"The Hotel\",\"description\":\"Spend one final evening in the hotspring before heading home.\"}]}]}"]
    ];

    // 4. Create options with JSON schema and tool
    ChatOptionsNative *options = [[ChatOptionsNative alloc] init];
    
    // JSON schema for structured itinerary output
    options.responseJsonSchema = @"{\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"description\":\"A travel itinerary with days and activities.\",\"type\":\"object\",\"properties\":{\"title\":{\"description\":\"An exciting name for the trip.\",\"type\":\"string\"},\"destinationName\":{\"type\":\"string\"},\"description\":{\"type\":\"string\"},\"rationale\":{\"description\":\"An explanation of how the itinerary meets the person's special requests.\",\"type\":\"string\"},\"days\":{\"description\":\"A list of day-by-day plans.\",\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"title\":{\"description\":\"A unique and exciting title for this day plan.\",\"type\":\"string\"},\"subtitle\":{\"type\":\"string\"},\"destination\":{\"type\":\"string\"},\"activities\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"type\":{\"type\":\"string\",\"enum\":[\"Sightseeing\",\"FoodAndDining\",\"Shopping\",\"HotelAndLodging\"]},\"title\":{\"type\":\"string\"},\"description\":{\"type\":\"string\"}},\"required\":[\"type\",\"title\",\"description\"],\"title\":\"Activity\",\"additionalProperties\":false},\"minItems\":3,\"maxItems\":3}},\"required\":[\"title\",\"subtitle\",\"destination\",\"activities\"],\"title\":\"DayPlan\",\"additionalProperties\":false},\"minItems\":3,\"maxItems\":3}},\"required\":[\"title\",\"destinationName\",\"description\",\"rationale\",\"days\"],\"title\":\"Itinerary\",\"additionalProperties\":false}";

    // Create tool instance
    FindPointsOfInterestTool *poiTool = [[FindPointsOfInterestTool alloc] init];
    options.tools = @[ poiTool ];

    // 5. Define callbacks
    AICompletionBlock completion =
        ^(ChatResponseNative *_Nullable result, NSError *_Nullable error) {
          if (error) {
              NSLog(@"Error from AI: %@", error);
              return;
          }
          
          NSLog(@"AI Response received with %lu messages", (unsigned long)result.messages.count);
          
          for (ChatMessageNative *msg in result.messages) {
              NSLog(@"Message role: %ld", (long)msg.role);
              
              for (AIContentNative *content in msg.contents) {
                  if ([content isKindOfClass:[TextContentNative class]]) {
                      TextContentNative *text = (TextContentNative *)content;
                      NSLog(@"  Text: %@", text.text);
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

    AIUpdateBlock update = ^(ResponseUpdateNative *_Nullable result) {
      if (result.text) {
          NSLog(@"Stream update: text = %@", result.text);
      }
      if (result.toolCallName) {
          NSLog(@"Stream update: tool = %@ (id=%@)", result.toolCallName, result.toolCallId);
          if (result.toolCallArguments) {
              NSLog(@"  arguments = %@", result.toolCallArguments);
          }
          if (result.toolCallResult) {
              NSLog(@"  result = %@", result.toolCallResult);
          }
      }
    };

    // 6. Call streamResponse with system and user messages
    NSArray *messages = @[ systemMessage, userMessage ];
    
    CancellationTokenNative *streamToken =
        [client streamResponseWithMessages:messages
                                   options:options
                                  onUpdate:update
                                onComplete:completion];
    
    // Keep reference to prevent deallocation
    (void)streamToken;
}

@end
