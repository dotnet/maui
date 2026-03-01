import Foundation
import WidgetKit

@objc(MauiWidgetHelper)
public class MauiWidgetHelper: NSObject {
    
    @objc public static func reloadTimelines(_ kind: String) {
        if #available(iOS 14.0, *) {
            WidgetCenter.shared.reloadTimelines(ofKind: kind)
        }
    }
    
    @objc public static func reloadAllTimelines() {
        if #available(iOS 14.0, *) {
            WidgetCenter.shared.reloadAllTimelines()
        }
    }
}
