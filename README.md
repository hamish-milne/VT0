# VT0
**V**irtual **T**exturing with **Zero** Effort (for Unity)

## Quick Start

* Click *VT0 -> Virtualize Project*

## Slightly less quick start

* Swap your shader for an equivalent VT0 one for each material. For instance, switch Standard to VT0/Standard.
* Go to *VT0 -> Settings* and check all your texture formats are correct for each channel

## Making custom shaders

* Write your shaders as normal, then select them and click *VT0 -> Virtualize Shaders*

### If that doesn't work...

* Add `[Toggle] _VT0 ("VT0", Float) = 1` to your Properties section.
* Swap `tex2D(texture, uv)` for `VT0_sample(texture, uv)` for all textures you want to virtualize.
* If you get compile errors because you used a different texture name, add it to `VT0_channels.cginc` in the form `VT0_def(MyTextureName)`, or just click 'Fix' in the VT0 Settings screen.

## Configuration

* The main metric is the *Virtual Texture Size* in the Settings screen. Based on the channels being used, you'll get an indication of the VRAM required as a result. This can be changed at runtime too.
* *Thumbnail Size* is the minimum resolution for each texture, and will have a small impact on build sizes and VRAM that scales linearly with the number of textures in the project.

## Caveats

* Mixing virtualized and non-virtualized use of the same texture will work, but it will (naturally) use extra memory
* Referencing a texture object in a component or other asset will potentially cause it to be loaded in VRAM. To fix this, add the `[VT0Reference]` attribute to your texture property, or see the How It Works section for more info.
* Most texture formats are supported, but they must be uncompressed or block-based: crunched and stream-based (e.g. PVRTC) won't work.
* VT0 uses standard Unity functionality with no native code, so a wide variety of platforms are supported, but *Copy texture support* is required for the fastest and lightest operation.

## How It Works

VT0 works by replacing references to texture assets with 'thumbnails'--small, conventially-loaded textures--at edit time. The full texture assets are then not referenced directly by anything, and can be loaded and unloaded into the virtual texture as needed. When building, the image hash of the thumbnail is used to name the full texture asset which is (assuming it is actually required by the build) moved, temporarily, to the Resources folder.

During play mode every frame, the following loop happens:
* *Priority*: All the active Renderers are found and given a weighting based on their approximate size on screen. A Material's weighting is the maximum of those Renderers it is attached to.
* *Arranger*: The weighting of each Material is used to determine whether its space in the virtual texture should be increased or decreased. By default, if there is enough room to do so the smallest increase in size happens; failing that, the largest decrease.
* *Output*: Texture assets are loaded from disk and compressed in-memory with LZ4, copied to a staging area then to the GPU. If texture copying isn't available, the staging area is the size of the entire virtual texture rather than that of a single element.
