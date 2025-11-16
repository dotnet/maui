# Edge Case Discovery

**CRITICAL**: Don't just test the PR author's scenario. Test edge cases they may have missed.

## Required edge cases for UI/Layout PRs:

- **Empty state**: Empty collections, null values, no data
- **Single item**: Collections with exactly one item
- **Large data sets**: 100+ items to test scrolling/virtualization
- **Dynamic changes**: Rapidly toggle properties (e.g., toggle FlowDirection 10 times)
- **Property combinations**: Test the fix with other properties (e.g., RTL + IsVisible + Margin)
- **Nested scenarios**: Control inside control (e.g., CollectionView in ScrollView)
- **Platform-specific**: Test on all affected platforms (iOS, Android, Windows, Mac)
- **Orientation**: Portrait vs landscape (mobile/tablet)
- **Screen sizes**: Different screen sizes and densities

## For layout/positioning PRs, also test:

- **Header/Footer**: With and without headers/footers
- **Padding/Margin**: Various padding and margin combinations
- **Alignment**: Different HorizontalOptions/VerticalOptions
- **Parent constraints**: Different parent sizes and constraints

## For behavior/interaction PRs, also test:

- **Timing**: Rapid interactions, delayed interactions
- **State transitions**: Page appearing/disappearing, backgrounding/foregrounding
- **User interaction**: Tap, scroll, swipe during state changes

## Document findings:

For each edge case tested, document:
- What you tested
- Expected behavior
- Actual behavior
- Whether it works correctly or reveals an issue
