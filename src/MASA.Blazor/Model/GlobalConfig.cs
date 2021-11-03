using MASA.Blazor.Model;
using System;

namespace BlazorComponent
{
    /// <summary>
    /// Cascading this will cause additional render,we may just cascading rtl in the feature
    /// </summary>
    public class GlobalConfig
    {
        private bool _rtl;

        public bool RTL
        {
            get
            {
                return _rtl;
            }
            set
            {
                if (_rtl != value)
                {
                    _rtl = value;
                    OnRTLChange?.Invoke(_rtl);
                }
            }
        }

        public event Action<bool> OnRTLChange;

        private Application _application = new();

        public Application Application
        {
            get
            {
                return _application;
            }
            set
            {
                if (_application != value)
                {
                    _application = value;
                    OnApplicationChange?.Invoke(_application);
                }
            }
        }

        public event Action<Application> OnApplicationChange;
    }
}
