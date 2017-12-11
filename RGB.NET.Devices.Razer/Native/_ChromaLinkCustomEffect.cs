﻿using System.Runtime.InteropServices;

namespace RGB.NET.Devices.Razer.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct _ChromaLinkCustomEffect
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = _Defines.CHROMALINK_MAX_LEDS)]
        public _Color[] Color;
    }
}
