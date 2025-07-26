#import <objc/runtime.h>
#import <UIKit/UIKit.h>
#import "CALayer+AutoSizeToSuperLayer.h"

@implementation CALayer (AutoSizeToSuperLayer)

static char CALayerAutoSizeToSuperLayer;

+ (void)load
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        Method originalSetBounds = class_getInstanceMethod(self, @selector(setBounds:));
        Method swizzledSetBounds = class_getInstanceMethod(self, @selector(maui_setBounds:));
        method_exchangeImplementations(originalSetBounds, swizzledSetBounds);
    });
}

- (void)maui_setBounds:(CGRect)bounds
{
    // Loop through sublayers and adjust their frames if autoSizeToSuperLayer is enabled
    for (CALayer *layer in self.sublayers)
        if (layer.autoSizeToSuperLayer && !CGRectEqualToRect(self.frame, bounds))
            [layer setFrame:bounds];

    // Call the original setBounds: method (exchanged with maui_setBounds:)
    [self maui_setBounds:bounds];
}

- (BOOL)autoSizeToSuperLayer
{
    NSNumber *autoSizeNumber = objc_getAssociatedObject(self, &CALayerAutoSizeToSuperLayer);
    return (BOOL)[autoSizeNumber boolValue];
}

- (void)setAutoSizeToSuperLayer:(BOOL)autoSize
{
    objc_setAssociatedObject(self, &CALayerAutoSizeToSuperLayer, @(autoSize), OBJC_ASSOCIATION_RETAIN_NONATOMIC);

    if (autoSize && self.superlayer)
        [self setFrame:self.superlayer.bounds];
}

@end
