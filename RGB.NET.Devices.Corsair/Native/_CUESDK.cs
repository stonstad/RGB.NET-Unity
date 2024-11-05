#nullable enable

#pragma warning disable IDE1006 // Naming Styles
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RGB.NET.Core;

namespace RGB.NET.Devices.Corsair.Native
{

    internal delegate void CorsairSessionStateChangedHandler(nint context, _CorsairSessionStateChanged eventData);

    [AttributeUsage(AttributeTargets.Method)]
    internal class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type t) { }
    }

    // ReSharper disable once InconsistentNaming
    internal static unsafe class _CUESDK
    {
        #region Constants

        /// <summary>
        /// iCUE-SDK: small string length
        /// </summary>
        internal const int CORSAIR_STRING_SIZE_S = 64;

        /// <summary>
        /// iCUE-SDK: medium string length
        /// </summary>
        internal const int CORSAIR_STRING_SIZE_M = 128;

        /// <summary>
        /// iCUE-SDK: maximum level of layer’s priority that can be used in CorsairSetLayerPriority
        /// </summary>
        internal const int CORSAIR_LAYER_PRIORITY_MAX = 255;

        /// <summary>
        /// iCUE-SDK: maximum number of devices to be discovered
        /// </summary>
        internal const int CORSAIR_DEVICE_COUNT_MAX = 64;

        /// <summary>
        /// iCUE-SDK: maximum number of LEDs controlled by device
        /// </summary>
        internal const int CORSAIR_DEVICE_LEDCOUNT_MAX = 512;

        #endregion

        #region Properties & Fields

        // ReSharper disable once NotAccessedField.Local - This is important, the delegate can be collected if it's not stored!
        private static readonly CorsairSessionStateChangedHandler SESSION_STATE_CHANGED_CALLBACK;

        internal static bool IsConnected => SesionState == CorsairSessionState.Connected;
        internal static CorsairSessionState SesionState { get; private set; }

        #endregion

        #region Events

        internal static event EventHandler<CorsairSessionState>? SessionStateChanged;

        #endregion

        #region Constructors

        static _CUESDK()
        {
            SESSION_STATE_CHANGED_CALLBACK = CorsairSessionStateChangedCallback;
        }

        #endregion

        #region Methods

        [MonoPInvokeCallback(typeof(CorsairSessionStateChangedHandler))]
        private static void CorsairSessionStateChangedCallback(nint context, _CorsairSessionStateChanged eventdata)
        {
            SesionState = eventdata.state;
            SessionStateChanged?.Invoke(null, eventdata.state);
        }

        #endregion

        #region Libary Management

        private static nint _handle = 0;

        /// <summary>
        /// Reloads the SDK.
        /// </summary>
        internal static void Reload()
        {
            UnloadCUESDK();
            LoadCUESDK();
        }

        private static void LoadCUESDK()
        {
            if (_handle != 0) return;

            string? dllPath = CorsairDeviceProvider.PossibleX64NativePaths.FirstOrDefault(File.Exists);
            if (dllPath == null)
                throw new RGBDeviceException($"Can't find the CUE-SDK at one of the expected locations:\r\n '{string.Join("\r\n", CorsairDeviceProvider.PossibleX64NativePaths.Select(Path.GetFullPath))}'");

            if (!NativeLibrary.TryLoad(dllPath, out _handle))
                throw new RGBDeviceException($"Corsair LoadLibrary failed");
            //throw new RGBDeviceException($"Corsair LoadLibrary failed with error code {Marshal.GetLastPInvokeError()}");

            _corsairConnectPtr = Marshal.GetDelegateForFunctionPointer<CorsairConnectPtrDelegate>(LoadFunction("CorsairConnect"));
            _corsairGetSessionDetails = Marshal.GetDelegateForFunctionPointer<CorsairGetSessionDetailsDelegate>(LoadFunction("CorsairGetSessionDetails"));
            _corsairDisconnect = Marshal.GetDelegateForFunctionPointer<CorsairDisconnectDelegate>(LoadFunction("CorsairDisconnect"));
            _corsairGetDevices = Marshal.GetDelegateForFunctionPointer<CorsairGetDevicesDelegate>(LoadFunction("CorsairGetDevices"));
            _corsairGetDeviceInfo = Marshal.GetDelegateForFunctionPointer<CorsairGetDeviceInfoDelegate>(LoadFunction("CorsairGetDeviceInfo"));
            _corsairGetLedPositions = Marshal.GetDelegateForFunctionPointer<CorsairGetLedPositionsDelegate>(LoadFunction("CorsairGetLedPositions"));
            _corsairSetLedColors = Marshal.GetDelegateForFunctionPointer<CorsairSetLedColorsDelegate>(LoadFunction("CorsairSetLedColors"));
            _corsairSetLayerPriority = Marshal.GetDelegateForFunctionPointer<CorsairSetLayerPriorityDelegate>(LoadFunction("CorsairSetLayerPriority"));
            _corsairGetLedLuidForKeyName = Marshal.GetDelegateForFunctionPointer<CorsairGetLedLuidForKeyNameDelegate>(LoadFunction("CorsairGetLedLuidForKeyName"));
            _corsairRequestControl = Marshal.GetDelegateForFunctionPointer<CorsairRequestControlDelegate>(LoadFunction("CorsairRequestControl"));
            _corsairReleaseControl = Marshal.GetDelegateForFunctionPointer<CorsairReleaseControlDelegate>(LoadFunction("CorsairReleaseControl"));
            _getDevicePropertyInfo = Marshal.GetDelegateForFunctionPointer<GetDevicePropertyInfoDelegate>(LoadFunction("CorsairGetDevicePropertyInfo"));
            _readDeviceProperty = Marshal.GetDelegateForFunctionPointer<ReadDevicePropertyDelegate>(LoadFunction("CorsairReadDeviceProperty"));
        }

        private static nint LoadFunction(string function)
        {
            if (!NativeLibrary.TryGetExport(_handle, function, out nint ptr))
                throw new RGBDeviceException($"Failed to load Corsair function '{function}'");
            return ptr;
        }

        internal static void UnloadCUESDK()
        {
            if (_handle == 0) return;

            _corsairConnectPtr = null;
            _corsairGetSessionDetails = null;
            _corsairDisconnect = null;
            _corsairGetDevices = null;
            _corsairGetDeviceInfo = null;
            _corsairGetLedPositions = null;
            _corsairSetLedColors = null;
            _corsairSetLayerPriority = null;
            _corsairGetLedLuidForKeyName = null;
            _corsairRequestControl = null;
            _corsairReleaseControl = null;
            _getDevicePropertyInfo = null;
            _readDeviceProperty = null;

            NativeLibrary.Free(_handle);
            _handle = 0;
        }

        #endregion

        #region SDK-METHODS

        #region Pointers

        public delegate void CorsairSessionStateChangedHandler(nint session, _CorsairSessionStateChanged eventData);

        public delegate CorsairError CorsairConnectPtrDelegate(CorsairSessionStateChangedHandler handler, nint context);
        public delegate CorsairError CorsairGetSessionDetailsDelegate(nint details);
        public delegate CorsairError CorsairDisconnectDelegate();
        public delegate CorsairError CorsairGetDevicesDelegate(_CorsairDeviceFilter filter, int count, nint devices, out int result);
        public delegate CorsairError CorsairGetDeviceInfoDelegate(string deviceId, _CorsairDeviceInfo info);
        public delegate CorsairError CorsairGetLedPositionsDelegate(string deviceId, int ledCount, nint ledPositions, out int result);
        public delegate CorsairError CorsairSetLedColorsDelegate(string deviceId, int ledCount, nint ledColors);
        public delegate CorsairError CorsairSetLayerPriorityDelegate(uint priority);
        public delegate CorsairError CorsairGetLedLuidForKeyNameDelegate(string deviceId, char keyName, out uint luid);
        public delegate CorsairError CorsairRequestControlDelegate(string deviceId, CorsairAccessLevel accessLevel);
        public delegate CorsairError CorsairReleaseControlDelegate(string deviceId);
        public delegate CorsairError GetDevicePropertyInfoDelegate(string deviceId, CorsairDevicePropertyId propertyId, uint propertyIndex, out CorsairDataType dataType, out CorsairPropertyFlag propertyFlag);
        public delegate CorsairError ReadDevicePropertyDelegate(string deviceId, CorsairDevicePropertyId propertyId, uint propertyIndex, nint propertyValue);

        private static CorsairConnectPtrDelegate? _corsairConnectPtr;
        private static CorsairGetSessionDetailsDelegate? _corsairGetSessionDetails;
        private static CorsairDisconnectDelegate? _corsairDisconnect;
        private static CorsairGetDevicesDelegate? _corsairGetDevices;
        private static CorsairGetDeviceInfoDelegate? _corsairGetDeviceInfo;
        private static CorsairGetLedPositionsDelegate? _corsairGetLedPositions;
        private static CorsairSetLedColorsDelegate? _corsairSetLedColors;
        private static CorsairSetLayerPriorityDelegate? _corsairSetLayerPriority;
        private static CorsairGetLedLuidForKeyNameDelegate? _corsairGetLedLuidForKeyName;
        private static CorsairRequestControlDelegate? _corsairRequestControl;
        private static CorsairReleaseControlDelegate? _corsairReleaseControl;
        private static GetDevicePropertyInfoDelegate? _getDevicePropertyInfo;
        private static ReadDevicePropertyDelegate? _readDeviceProperty;

        #endregion

        internal static CorsairError CorsairConnect()
        {
            if (_corsairConnectPtr == null) throw new RGBDeviceException("The Corsair-SDK is not initialized.");
            if (IsConnected) throw new RGBDeviceException("The Corsair-SDK is already connected.");
            return _corsairConnectPtr(SESSION_STATE_CHANGED_CALLBACK, 0);
        }

        internal static CorsairError CorsairGetSessionDetails(out _CorsairSessionDetails? details)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");

            nint sessionDetailPtr = Marshal.AllocHGlobal(Marshal.SizeOf<_CorsairSessionDetails>());
            try
            {
                if (_corsairGetSessionDetails != null)
                {
                    CorsairError error = _corsairGetSessionDetails(sessionDetailPtr);
                    details = Marshal.PtrToStructure<_CorsairSessionDetails>(sessionDetailPtr);
                    return error;
                }
                details = default;
                return CorsairError.InvalidOperation;
            }
            finally
            {
                Marshal.FreeHGlobal(sessionDetailPtr);
            }
        }

        internal static CorsairError CorsairDisconnect()
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");
            if (_corsairDisconnect != null)
                return _corsairDisconnect();
            else
                return CorsairError.InvalidOperation;
        }

        internal static CorsairError CorsairGetDevices(_CorsairDeviceFilter filter, out _CorsairDeviceInfo[] devices)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");

            int structSize = Marshal.SizeOf<_CorsairDeviceInfo>();
            nint devicePtr = Marshal.AllocHGlobal(structSize * CORSAIR_DEVICE_COUNT_MAX);
            try
            {
                if (_corsairGetDevices != null)
                {
                    CorsairError error = _corsairGetDevices(filter, CORSAIR_DEVICE_COUNT_MAX, devicePtr, out int size);
                    devices = devicePtr.ToArray<_CorsairDeviceInfo>(size);
                    return error;
                }
                devices = new _CorsairDeviceInfo[0];
                return CorsairError.InvalidOperation;
            }
            finally
            {
                Marshal.FreeHGlobal(devicePtr);
            }
        }

        internal static CorsairError CorsairGetDeviceInfo(string deviceId, _CorsairDeviceInfo deviceInfo)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");
            if (_corsairGetDeviceInfo != null)
                return _corsairGetDeviceInfo(deviceId, deviceInfo);
            return CorsairError.InvalidOperation;
        }

        internal static CorsairError CorsairGetLedPositions(string deviceId, out _CorsairLedPosition[] ledPositions)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");

            int structSize = Marshal.SizeOf<_CorsairLedPosition>();
            nint ledPositionsPtr = Marshal.AllocHGlobal(structSize * CORSAIR_DEVICE_LEDCOUNT_MAX);
            try
            {
                if (_corsairGetLedPositions != null)
                {
                    CorsairError error = _corsairGetLedPositions(deviceId, CORSAIR_DEVICE_LEDCOUNT_MAX, ledPositionsPtr, out int size);
                    ledPositions = ledPositionsPtr.ToArray<_CorsairLedPosition>(size);
                    return error;
                }
                ledPositions = new _CorsairLedPosition[0];
                return CorsairError.InvalidOperation;
            }
            finally
            {
                Marshal.FreeHGlobal(ledPositionsPtr);
            }
        }

        internal static CorsairError CorsairSetLedColors(string deviceId, int ledCount, nint ledColorsPtr)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");
            if (_corsairSetLedColors != null)
                return _corsairSetLedColors(deviceId, ledCount, ledColorsPtr);
            return CorsairError.InvalidOperation;
        }

        internal static CorsairError CorsairSetLayerPriority(uint priority)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");
            if (_corsairSetLayerPriority != null)
                return _corsairSetLayerPriority(priority);
            return CorsairError.InvalidOperation;
        }

        internal static CorsairError CorsairGetLedLuidForKeyName(string deviceId, char keyName, out uint ledId)
        {
            if (!IsConnected)
                throw new RGBDeviceException("The Corsair-SDK is not connected.");
            if (_corsairGetLedLuidForKeyName != null)
                return _corsairGetLedLuidForKeyName(deviceId, keyName, out ledId);

            ledId = default;
            return CorsairError.InvalidOperation;
        }

        internal static CorsairError CorsairRequestControl(string deviceId, CorsairAccessLevel accessLevel)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");
            if (_corsairRequestControl != null)
                return _corsairRequestControl(deviceId, accessLevel);
            return CorsairError.InvalidOperation;
        }

        internal static CorsairError CorsairReleaseControl(string deviceId)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");
            if (_corsairReleaseControl != null)
                return _corsairReleaseControl.Invoke(deviceId);
            return CorsairError.InvalidOperation;
        }

        internal static CorsairError GetDevicePropertyInfo(string deviceId, CorsairDevicePropertyId propertyId, uint index, out CorsairDataType dataType, out CorsairPropertyFlag flags)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");
            if (_getDevicePropertyInfo != null)
                return _getDevicePropertyInfo(deviceId, propertyId, index, out dataType, out flags);
            dataType = default;
            flags = default;
            return CorsairError.InvalidOperation;
        }

        internal static CorsairError ReadDeviceProperty(string deviceId, CorsairDevicePropertyId propertyId, uint index, out _CorsairProperty? property)
        {
            if (!IsConnected) throw new RGBDeviceException("The Corsair-SDK is not connected.");

            nint propertyPtr = Marshal.AllocHGlobal(Marshal.SizeOf<_CorsairProperty>());
            try
            {
                if (_readDeviceProperty != null)
                {
                    CorsairError error = _readDeviceProperty(deviceId, propertyId, index, propertyPtr);
                    property = Marshal.PtrToStructure<_CorsairProperty>(propertyPtr);
                    return error;
                }
                property = default;
                return CorsairError.InvalidOperation;
            }
            finally
            {
                Marshal.FreeHGlobal(propertyPtr);
            }
        }

        internal static int ReadDevicePropertySimpleInt32(string deviceId, CorsairDevicePropertyId propertyId, uint index = 0) => ReadDevicePropertySimple(deviceId, propertyId, CorsairDataType.Int32, index).int32;

        internal static int[] ReadDevicePropertySimpleInt32Array(string deviceId, CorsairDevicePropertyId propertyId, uint index = 0)
        {
            _CorsairDataValue dataValue = ReadDevicePropertySimple(deviceId, propertyId, CorsairDataType.Int32Array, index);
            return dataValue.int32Array.items.ToArray<int>((int)dataValue.int32Array.count);
        }

        internal static _CorsairDataValue ReadDevicePropertySimple(string deviceId, CorsairDevicePropertyId propertyId, CorsairDataType expectedDataType, uint index = 0)
        {
            CorsairError errorCode = GetDevicePropertyInfo(deviceId, propertyId, index, out CorsairDataType dataType, out CorsairPropertyFlag flags);
            if (errorCode != CorsairError.Success)
                throw new RGBDeviceException($"Failed to read device-property-info '{propertyId}' for corsair device '{deviceId}'. (ErrorCode: {errorCode})");

            if (dataType != expectedDataType)
                throw new RGBDeviceException($"Failed to read device-property-info '{propertyId}' for corsair device '{deviceId}'. (Wrong data-type '{dataType}', expected: '{expectedDataType}')");

            if (!flags.HasFlag(CorsairPropertyFlag.CanRead))
                throw new RGBDeviceException($"Failed to read device-property-info '{propertyId}' for corsair device '{deviceId}'. (Not readable)");

            errorCode = ReadDeviceProperty(deviceId, propertyId, index, out _CorsairProperty? property);
            if (errorCode != CorsairError.Success)
                throw new RGBDeviceException($"Failed to read device-property '{propertyId}' for corsair device '{deviceId}'. (ErrorCode: {errorCode})");

            if (property == null)
                throw new RGBDeviceException($"Failed to read device-property '{propertyId}' for corsair device '{deviceId}'. (Invalid return value)");

            if (property.Value.type != expectedDataType)
                throw new RGBDeviceException($"Failed to read device-property '{propertyId}' for corsair device '{deviceId}'. (Wrong data-type '{dataType}', expected: '{expectedDataType}')");

            return property.Value.value;
        }

        #endregion
    }
}