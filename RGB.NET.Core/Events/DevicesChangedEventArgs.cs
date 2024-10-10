using System;

namespace RGB.NET.Core
{

    public sealed class DevicesChangedEventArgs : EventArgs
    {
        public DevicesChangedEventArgs(IRGBDevice device, DevicesChangedEventArgs.DevicesChangedAction action)
        {
            Device = device;
            Action = action;
        }

        #region Properties & Fields

        public IRGBDevice Device { get; private set; }
        public DevicesChangedAction Action { get; private set; }

        #endregion

        #region Methods

        public static DevicesChangedEventArgs CreateDevicesAddedArgs(IRGBDevice addedDevice) => new(addedDevice, DevicesChangedAction.Added);
        public static DevicesChangedEventArgs CreateDevicesRemovedArgs(IRGBDevice removedDevice) => new(removedDevice, DevicesChangedAction.Removed);

        #endregion

        public enum DevicesChangedAction
        {
            Added,
            Removed
        }
    }
}