# RGB.NET-Unity

This is a Unity-compatible fork of [RGB.NET](https://github.com/DarthAffe/RGB.NET), an outstanding .NET library created by [DarthAffe](https://github.com/DarthAffe) for controlling RGB-devices.

> **IMPORTANT NOTE**   
This fork is an **interim** solution until Unity releases support for CoreCLR and .NET 8 within their engine. You should use the original project's libraries in Unity 7.0.

## Changes ##
The code within this repository has been modified to support .NET Standard 2.1, Unity 2022.x, and Corsair RGB devices. Full source code with modifications is provided in this repository per the existing LGPL license. Please send all accoldades to [DarthAffe](https://github.com/DarthAffe). Modifications are as follows:

- Remove file-scoped namespace definitions for compatibility with C# 9.0.
- Add default body to interface specifications.
- Remove platform checks and enforce selection of the Window platform.
- Change collection initialization syntax for compaatibility with C# 9.0.
- Enable nullable within file scopes.
- Add [NativeLibrary](https://github.com/udaken/Shim4DotNetFramework.NativeLibrary/tree/main) shim for compatibility with .NET Core Interop.

## Sample Unity Script ##

```csharp
using RGB.NET.Core;
using RGB.NET.Devices.Corsair;
using RGB.NET.Presets.Decorators;
using RGB.NET.Presets.Textures;
using RGB.NET.Presets.Textures.Gradients;
using System.Threading.Tasks;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private RGBSurface _Surface;

    private void Start()
    {
        Task.Run(() =>
        {
            UnityEngine.Debug.Log("A");
            CorsairDeviceProvider.PossibleX64NativePaths.Clear();
            CorsairDeviceProvider.PossibleX64NativePaths.Add("Assets/Plugins/RGB.NET/iCUESDK.x64_2019.dll");

            _Surface = new RGBSurface();
            _Surface.Exception += (e) => UnityEngine.Debug.Log(e.Exception.ToString());
            _Surface.Load(CorsairDeviceProvider.Instance);
            _Surface.AlignDevices();

            _Surface.RegisterUpdateTrigger(new TimerUpdateTrigger());

            ILedGroup allLeds = new ListLedGroup(_Surface, _Surface.Leds);
            RainbowGradient rainbow = new RainbowGradient();
            rainbow.AddDecorator(new MoveGradientDecorator(_Surface));
            ConicalGradientTexture texture = new ConicalGradientTexture(new Size(10, 10), rainbow);
            allLeds.Brush = new TextureBrush(texture);
            _Surface.Update();
        });
    }

    private void OnApplicationQuit()
    {
        if (_Surface != null)
            _Surface.Dispose();
    }
}
```
