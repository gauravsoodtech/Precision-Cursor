using System;
using System.Drawing;
using System.Windows.Forms;

namespace PrecisionCursor
{
    internal sealed class TrayAppContext : ApplicationContext
    {
        private readonly MouseLockService _mouseLockService;
        private readonly KeyboardHookService _keyboardHookService;
        private readonly NotifyIcon _trayIcon;
        private readonly Icon _appIcon;
        private readonly ToolStripMenuItem _statusItem;
        private readonly ToolStripMenuItem _toggleItem;
        private bool _disposed;

        public TrayAppContext()
        {
            _mouseLockService = new MouseLockService();
            _keyboardHookService = new KeyboardHookService(ToggleLineLock, DisableLineLock);

            _statusItem = new ToolStripMenuItem { Enabled = false };
            _toggleItem = new ToolStripMenuItem("Toggle\tCtrl+Alt+L", null, OnToggleClicked);
            _appIcon = LoadApplicationIcon();

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add(_statusItem);
            menu.Items.Add(_toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Exit", null, OnExitClicked));

            _trayIcon = new NotifyIcon
            {
                ContextMenuStrip = menu,
                Icon = _appIcon,
                Text = "Precision Cursor - Disabled",
                Visible = true
            };

            _trayIcon.DoubleClick += OnTrayIconDoubleClick;
            _mouseLockService.EnabledChanged += OnMouseLockEnabledChanged;

            try
            {
                _mouseLockService.Start();
                _keyboardHookService.Start();
                UpdateTrayState();
            }
            catch
            {
                DisposeResources();
                throw;
            }
        }

        private void ToggleLineLock()
        {
            _mouseLockService.Toggle();
        }

        private void DisableLineLock()
        {
            _mouseLockService.Disable();
        }

        private void UpdateTrayState()
        {
            bool enabled = _mouseLockService.Enabled;
            _statusItem.Text = enabled ? "Enabled" : "Disabled";
            _toggleItem.Text = enabled ? "Disable\tCtrl+Alt+L" : "Enable\tCtrl+Alt+L";
            _trayIcon.Text = enabled ? "Precision Cursor - Enabled" : "Precision Cursor - Disabled";
        }

        private void OnMouseLockEnabledChanged(object sender, EventArgs e)
        {
            UpdateTrayState();
        }

        private void OnToggleClicked(object sender, EventArgs e)
        {
            ToggleLineLock();
        }

        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            ToggleLineLock();
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            ExitApplication();
        }

        private void ExitApplication()
        {
            DisposeResources();
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeResources();
            }

            base.Dispose(disposing);
        }

        private void DisposeResources()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _keyboardHookService.Dispose();
            _mouseLockService.Dispose();

            _trayIcon.DoubleClick -= OnTrayIconDoubleClick;
            _mouseLockService.EnabledChanged -= OnMouseLockEnabledChanged;
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _appIcon.Dispose();
        }

        private static Icon LoadApplicationIcon()
        {
            Icon extractedIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            if (extractedIcon != null)
            {
                return extractedIcon;
            }

            return (Icon)SystemIcons.Application.Clone();
        }
    }
}
