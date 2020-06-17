using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TribeSim
{
    class UserPreferences
    {
        private double _windowTop;

        public double WindowTop
        {
            get { return _windowTop; }
            set { _windowTop = value; }
        }
        private double _windowLeft;

        public double WindowLeft
        {
            get { return _windowLeft; }
            set { _windowLeft = value; }
        }
        private double _windowHeight;

        public double WindowHeight
        {
            get { return _windowHeight; }
            set { _windowHeight = value; }
        }
        private double _windowWidth;

        public double WindowWidth
        {
            get { return _windowWidth; }
            set { _windowWidth = value; }
        }
        private System.Windows.WindowState _windowState;

        public System.Windows.WindowState WindowState
        {
            get { return _windowState; }
            set { _windowState = value; }
        }

        public UserPreferences()
        {
            //Load the settings
            Load();

            //Size it to fit the current screen
            SizeToFit();

            //Move the window at least partially into view
            MoveIntoView();
        }


        private void Load()
        {
            _windowTop = Properties.Settings.Default.WindowTop;
            _windowLeft = Properties.Settings.Default.WindowLeft;
            _windowHeight = Properties.Settings.Default.WindowHeight;
            _windowWidth = Properties.Settings.Default.WindowWidth;
            _windowState = Properties.Settings.Default.WindowState;
        }

        public void Save()
        {
            if (_windowState != System.Windows.WindowState.Minimized)
            {
                Properties.Settings.Default.WindowTop = _windowTop;
                Properties.Settings.Default.WindowLeft = _windowLeft;
                Properties.Settings.Default.WindowHeight = _windowHeight;
                Properties.Settings.Default.WindowWidth = _windowWidth;
                Properties.Settings.Default.WindowState = _windowState;

                Properties.Settings.Default.Save();
            }
        }

        public void MoveIntoView()
        {
            if (_windowTop + _windowHeight / 2 >
                 System.Windows.SystemParameters.VirtualScreenHeight)
            {
                _windowTop =
                  System.Windows.SystemParameters.VirtualScreenHeight -
                  _windowHeight;
            }

            if (_windowLeft + _windowWidth / 2 >
                     System.Windows.SystemParameters.VirtualScreenWidth)
            {
                _windowLeft =
                  System.Windows.SystemParameters.VirtualScreenWidth -
                  _windowWidth;
            }

            if (_windowTop < 0)
            {
                _windowTop = 0;
            }

            if (_windowLeft < 0)
            {
                _windowLeft = 0;
            }
        }

        public void SizeToFit()
        {
            if (_windowHeight > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                _windowHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            }

            if (_windowWidth > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                _windowWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            }
        }
    }
}
