using System;

namespace RGB.NET.Core
{
    public sealed class ActionDisposable : IDisposable
    {
        private Action onDispose;

        public ActionDisposable(Action action)
        {
            onDispose = action;
        }

        #region Methods

        public void Dispose() => onDispose();

        #endregion
    }
}