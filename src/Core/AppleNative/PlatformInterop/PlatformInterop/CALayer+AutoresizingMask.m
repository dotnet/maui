#import <objc/runtime.h>
#import <UIKit/UIKit.h>
#import "CALayer+AutoresizingMask.h"

@implementation CALayer (AutoresizingMask)

static char CALayerAutoresizingMask;

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
    // Only if size has changed
    if (!CGSizeEqualToSize(self.bounds.size, bounds.size))
        for (CALayer *layer in self.sublayers)
            if (layer.platformAutoresizingMask != UIViewAutoresizingNone)
                [layer applyPlatformAutoresizingMask:self.bounds.size toSize:bounds.size];

    [self maui_setBounds:bounds];
}

- (UIViewAutoresizing)platformAutoresizingMask
{
    NSNumber *maskNumber = objc_getAssociatedObject(self, &CALayerAutoresizingMask);
    return (UIViewAutoresizing)[maskNumber unsignedIntegerValue];
}

- (void)setPlatformAutoresizingMask:(UIViewAutoresizing)mask
{
    objc_setAssociatedObject(self, &CALayerAutoresizingMask, @(mask), OBJC_ASSOCIATION_RETAIN_NONATOMIC);
}

- (void)applyPlatformAutoresizingMask:(CGSize)size toSize:(CGSize)newSize
{
    UIViewAutoresizing mask = self.platformAutoresizingMask;
    
    CGFloat dx = newSize.width - size.width;
    CGFloat dy = newSize.height - size.height;

    CGFloat flexibleLeftMargin = (mask & UIViewAutoresizingFlexibleLeftMargin) ? 1 : 0;
    CGFloat flexibleWidth = (mask & UIViewAutoresizingFlexibleWidth) ? 1 : 0;
    CGFloat flexibleRightMargin = (mask & UIViewAutoresizingFlexibleRightMargin) ? 1 : 0;
    CGFloat flexibleTopMargin = (mask & UIViewAutoresizingFlexibleTopMargin) ? 1 : 0;
    CGFloat flexibleHeight = (mask & UIViewAutoresizingFlexibleHeight) ? 1 : 0;
    CGFloat flexibleBottomMargin = (mask & UIViewAutoresizingFlexibleBottomMargin) ? 1 : 0;

    CGFloat totalXFactors = flexibleLeftMargin + flexibleWidth + flexibleRightMargin;
    CGFloat totalYFactors = flexibleTopMargin + flexibleHeight + flexibleBottomMargin;

    dx /= (totalXFactors > 0) ? totalXFactors : 1;
    dy /= (totalYFactors > 0) ? totalYFactors : 1;
    
    CGFloat scale = [UIScreen mainScreen].scale;
    if ((fabs(dx) < 1.0/scale) && (fabs(dy) < 1.0/scale))
        return;
    
    CGRect frame = self.frame;
    frame.origin.x += flexibleLeftMargin * dx;
    frame.origin.y += flexibleTopMargin * dy;
    frame.size.width += flexibleWidth * dx;
    frame.size.height += flexibleHeight * dy;
    [self setFrame:frame];
}

@end
