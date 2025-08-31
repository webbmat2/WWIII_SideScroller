# WWIII SideScroller URP 2D Settings

This directory contains the Universal Render Pipeline configuration for WWIII SideScroller, optimized for mobile 60 FPS performance on iPhone 16+ (Backbone), tvOS (PS5), and macOS targets.

## Created Assets:
- `WWIII_URP_Asset.asset` - Main URP Pipeline configuration
- `WWIII_2D_Renderer.asset` - 2D Renderer for sprite-based gameplay
- `WWIII_Mobile_Quality.asset` - Mobile-optimized quality settings

## Optimization Focus:
- 60 FPS target on iPhone 16+
- Minimized draw calls via sprite batching
- Optimized for 2D side-scrolling gameplay
- Battery life considerations for mobile

## Unity 6 URP 17.2.0 Features:
- Enhanced 2D lighting support
- Improved sprite batching
- Mobile GPU optimization
- SRP Batcher enabled