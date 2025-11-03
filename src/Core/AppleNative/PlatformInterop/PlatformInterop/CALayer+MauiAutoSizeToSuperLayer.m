#import <objc/runtime.h>
#import <UIKit/UIKit.h>
#import "CALayer+MauiAutoSizeToSuperLayer.h"

@implementation CALayer (MauiAutoSizeToSuperLayer)

static char CALayerMauiAutoSizeToSuperLayer;

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
    // Loop through sublayers and adjust their frames if mauiAutoSizeToSuperLayer is enabled
    for (CALayer *layer in self.sublayers)
        if (layer.mauiAutoSizeToSuperLayer && !CGRectEqualToRect(self.frame, bounds))
            [layer setFrame:bounds];

    // Call the original setBounds: method (exchanged with maui_setBounds:)
    [self maui_setBounds:bounds];
}

- (BOOL)mauiAutoSizeToSuperLayer
{
    NSNumber *autoSizeNumber = objc_getAssociatedObject(self, &CALayerMauiAutoSizeToSuperLayer);
    return (BOOL)[autoSizeNumber boolValue];
}

- (void)setMauiAutoSizeToSuperLayer:(BOOL)autoSize
{
    objc_setAssociatedObject(self, &CALayerMauiAutoSizeToSuperLayer, @(autoSize), OBJC_ASSOCIATION_RETAIN_NONATOMIC);

    if (autoSize && self.superlayer)
        [self setFrame:self.superlayer.bounds];
}

@end
