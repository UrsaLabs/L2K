using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;
using WindowsInput.Native;
using IntelOrca.Launchpad;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace UrsaLabs.L2K {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        #region Dll Imports
        private const uint HWND_BROADCAST = 0xFFFF;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        #endregion Dll Imports

        LaunchpadDevice device;
        private IKeyboardSimulator keyboard;
        private DataStore data;

        public MainWindow() {

            try {
                device = new LaunchpadDevice();
                keyboard = new InputSimulator().Keyboard;
                data = DataStore.Instance;


                device.DoubleBuffered = true;

                device.DoubleBuffered = false;

                Application.Current.Exit += ClearLaunchpad;

                device.ButtonPressed += HandleLaunchPadInput;

                InitializeComponent();


                InitData();
                RefreshPad();
            } catch (Exception ex) {
                MessageBoxResult result = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                if (result == MessageBoxResult.OK) {
                    Application.Current.Shutdown();
                }
            }
        }

        private void ClearLaunchpad(object sender, EventArgs e) {
            if (device != null) {
                for (int y = 0; y < 8; y++) {
                    for (int x = 0; x < 8; x++) {
                        device[x, y].TurnOffLight();
                    }
                }
                foreach (SideButton button in Enum.GetValues(typeof(SideButton)).Cast<SideButton>()) {
                    device.GetButton(button).TurnOffLight();
                }
                foreach (ToolbarButton button in Enum.GetValues(typeof(ToolbarButton)).Cast<ToolbarButton>()) {
                    device.GetButton(button).TurnOffLight();
                }
            }
        }

        private void InitData() {
            data.Load();
            flagBoundButtons();
        }

        private void flagBoundButtons() {
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    string targetName = "_" + y + x;
                    var cut = data.GetShortcut(x, y);
                    if (cut != null) {
                        lightUpGui(targetName);
                    }
                }
            }
            foreach (SideButton button in Enum.GetValues(typeof(SideButton)).Cast<SideButton>()) {
                ButtonPressEventArgs e = new ButtonPressEventArgs(button);
                string name = processButtonName(e);
                var cut = data.GetSideShortcut(name);
                if (cut != null) {
                    lightUpGui("side" + name);
                } else {
                    lightDownGui("side" + name);
                }
            }
            foreach (ToolbarButton button in Enum.GetValues(typeof(ToolbarButton)).Cast<ToolbarButton>()) {
                ButtonPressEventArgs e = new ButtonPressEventArgs(button);
                string name = processButtonName(e);
                var cut = data.GetTopShortcut(name);
                if (cut != null) {
                    lightUpGui("top" + name);
                } else {
                    lightDownGui("top" + name);
                }
            }
        }

        private void lightUpGui(string targetName) {
            Application.Current.Dispatcher.Invoke(() => {
                var topBtn = (Button)this.FindName(targetName);
                if (topBtn.Background != Brushes.Green) {
                    topBtn.Background = Brushes.Orange;
                }
            });
        }

        private void lightUpGuiForce(string targetName) {
            Application.Current.Dispatcher.Invoke(() => {
                var topBtn = (Button)this.FindName(targetName);
                topBtn.Background = Brushes.Orange;
            });
        }

        private void groupSelectedGui(string targetName) {
            Application.Current.Dispatcher.Invoke(() => {
                var topBtn = (Button) this.FindName(targetName);
                topBtn.Background = Brushes.Green;
            });
        }

        private void lightDownGui(string targetName) {
            Application.Current.Dispatcher.Invoke(() => {
                var topBtn = (Button)this.FindName(targetName);
                topBtn.ClearValue(Button.BackgroundProperty);
            });
        }

        private void toggleGui(string targetName, bool toggleOn) {
            Application.Current.Dispatcher.Invoke(() => {
                var topBtn = (Button)this.FindName(targetName);
                if (toggleOn) {
                    topBtn.Background = Brushes.Green;
                } else {
                    topBtn.Background = Brushes.Orange;
                }
            });
        }

        private void toggleButton( LaunchpadButton button, ButtonType btype, String name) {
            string targetName;
            if (btype == ButtonType.Side) {
                targetName = "side" + name;   
            } else {
                targetName = "_" + name;
            }
            var toggleOn = false;
            if (button != null) {
                if (button.RedBrightness == ButtonBrightness.Full && button.GreenBrightness == ButtonBrightness.Full) {
                    button.SetBrightness(ButtonBrightness.Off, ButtonBrightness.Full);
                    toggleOn = true;
                } else if (button.RedBrightness == ButtonBrightness.Off &&
                           button.GreenBrightness == ButtonBrightness.Full) {
                    button.SetBrightness(ButtonBrightness.Full, ButtonBrightness.Full);
                }
            }
            toggleGui(targetName, toggleOn);
        }

        private void RefreshPad() {
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    if (data.GetShortcut(x, y) != null) {
                        var btn = device[x, y];
                        if (btn.RedBrightness == ButtonBrightness.Off && btn.GreenBrightness == ButtonBrightness.Off)
                            btn.TurnOnLight();
                    } else {
                        var btn = device[x, y];
                        if ((btn.RedBrightness == ButtonBrightness.Full && btn.GreenBrightness == ButtonBrightness.Full) ||
                            (btn.RedBrightness == ButtonBrightness.Off && btn.GreenBrightness == ButtonBrightness.Full))
                            btn.TurnOffLight();
                    }
                }
            }
            foreach (SideButton button in Enum.GetValues(typeof (SideButton)).Cast<SideButton>()) {
                if (data.GetSideShortcut(button.ToString()) != null) {
                    var btn = device.GetButton(button);
                    if (btn.RedBrightness == ButtonBrightness.Off && btn.GreenBrightness == ButtonBrightness.Off)
                        btn.TurnOnLight();
                }
                else {
                    var btn = device.GetButton(button);
                    if ((btn.RedBrightness == ButtonBrightness.Full && btn.GreenBrightness == ButtonBrightness.Full) ||
                        (btn.RedBrightness == ButtonBrightness.Off && btn.GreenBrightness == ButtonBrightness.Full))
                        btn.TurnOffLight();
                }
            }
            foreach (ToolbarButton button in Enum.GetValues(typeof (ToolbarButton)).Cast<ToolbarButton>()) {
                if (data.GetTopShortcut(button.ToString()) != null) {
                    device.GetButton(button).TurnOnLight();
                }
                else {
                    var btn = device.GetButton(button);
                    if ((btn.RedBrightness == ButtonBrightness.Full && btn.GreenBrightness == ButtonBrightness.Full) ||
                        (btn.RedBrightness == ButtonBrightness.Off && btn.GreenBrightness == ButtonBrightness.Full))
                        btn.TurnOffLight();
                }
            }
        }

        private void CleanGroup(int group) {
            foreach (var buttonName in data.getGroup(group)) {
                var x = Convert.ToInt32(buttonName[0].ToString());
                var y = Convert.ToInt32(buttonName[1].ToString());
                device[x,y].TurnOnLight();
                lightUpGuiForce("_"+y.ToString() + x);
            }
        }

        private void AssignKeyBinding(int x, int y) {
            var cut = data.GetShortcut(y, x);
            var result = new KeyBindingWindow(cut, x, y);
            if (result.ShowDialog() == true) {
                data.SetShortcut(y, x, result.KeyBinding);
                int group = result.KeyBinding.belongsToGroup;
                if (group > 0) {
                    data.addToGroup(group, y.ToString() + x);
                }
                data.Save();
            }
            RefreshPad();
            flagBoundButtons();
        }

        private void AssignKeyBinding(string name, bool isTop) {
            Shortcut cut;
            if (isTop) {
                cut = data.GetTopShortcut(name);
            } else {
                cut = data.GetSideShortcut(name);
            }
            var result = new KeyBindingWindow(cut, name, isTop);
            if (result.ShowDialog() == true) {
                if (result.KeyBinding != null) {
                    if (isTop) {
                        data.SetTopShortcut(name, result.KeyBinding);
                    }
                    else {
                        data.SetSideShortcut(name, result.KeyBinding);
                    }
                    data.Save();
                }
            }
            RefreshPad();
            flagBoundButtons();
        }

        protected void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            var btnPressed = e.OriginalSource as Button;
            var name = btnPressed.Name;
            if (name == "hideButton") {
                appWindow.Visibility = Visibility.Hidden;
                tIcon.Visibility = Visibility.Visible;
            } else {
                name = name.Replace("_", "");
                if (name.Length > 2) {
                    bool isTop = false;
                    if (name[0] == 't') {
                        isTop = true;
                        name = name.Replace("top", "");
                    } else {
                        name = name.Replace("side", "");
                    }
                    AssignKeyBinding(name, isTop);
                } else {
                    var x = name[0].ToString();
                    var y = name[1].ToString();
                    AssignKeyBinding(Convert.ToInt32(x), Convert.ToInt32(y));
                }
            }
        }

        public async void visualizeButtonPress(LaunchpadButton button, ButtonPressEventArgs e, string name) {
            string targetName = "";
            switch (e.Type) {
                case ButtonType.Grid:
                    targetName = "_" + name;
                    break;
                case ButtonType.Side:
                    targetName = "side" + name;
                    break;
                case ButtonType.Toolbar:
                    targetName = "top" + name;
                    break;
            }
            button.SetBrightness(ButtonBrightness.Off, ButtonBrightness.Full);
            Application.Current.Dispatcher.Invoke(() => {
                var topBtn = (Button)this.FindName(targetName);
                topBtn.Background = Brushes.Green;
            });
            await Task.Delay(500);
            button.SetBrightness(ButtonBrightness.Full, ButtonBrightness.Full);
            Application.Current.Dispatcher.Invoke(() => {
                var topBtn = (Button)this.FindName(targetName);
                topBtn.Background = Brushes.Orange;
            });
        }

        private void HandleLaunchPadInput(object sender, ButtonPressEventArgs e) {
            if (e.Type == ButtonType.Grid) {
                LaunchpadButton button = device[e.X, e.Y];
                Shortcut cut = data.GetShortcut(e.X, e.Y);
                if (cut != null) {
                    string name = e.Y + e.X.ToString();
                    var group = cut.belongsToGroup;
                    if (cut.belongsToGroup > 0) {
                        CleanGroup(group);
                        button.SetBrightness(ButtonBrightness.Off, ButtonBrightness.Full);
                        groupSelectedGui("_"+name);
                    } else if (cut.isToggle) {
                        toggleButton(button, e.Type, name);
                    } else {
                        visualizeButtonPress(button, e, name);
                    }
                    processShortcut(cut);
                }
            } else if (e.Type == ButtonType.Side) {
                string name = processButtonName(e);
                if (name != "") {
                    Shortcut cut = data.GetSideShortcut(name);
                    if (cut != null) {
                        var button = device.GetButton(e.SidebarButton);
                        if (cut.isToggle) {
                            toggleButton(button, e.Type, name);
                        }
                        else {
                            visualizeButtonPress(button, e, name);
                        }
                        processShortcut(cut);
                    }
                }
            } else if (e.Type == ButtonType.Toolbar) {
                string name = processButtonName(e);
                if (name != "") {
                    Shortcut cut = data.GetTopShortcut(name);
                    if (cut != null) {
                        processShortcut(cut);
                        var button = device.GetButton(e.ToolbarButton);
                        visualizeButtonPress(button, e, name);
                    }
                }
            }
        }

        private string processButtonName(ButtonPressEventArgs e) {
            var name = "";
            if (e.Type == ButtonType.Side) {
                switch (e.SidebarButton) {
                    case SideButton.Volume:
                        name = "Volume";
                        break;
                    case SideButton.Pan:
                        name = "Pan";
                        break;
                    case SideButton.SoundA:
                        name = "SoundA";
                        break;
                    case SideButton.SoundB:
                        name = "SoundB";
                        break;
                    case SideButton.Stop:
                        name = "Stop";
                        break;
                    case SideButton.TrackOn:
                        name = "TrackOn";
                        break;
                    case SideButton.Solo:
                        name = "Solo";
                        break;
                    case SideButton.Arm:
                        name = "Arm";
                        break;
                }
            } else if (e.Type == ButtonType.Toolbar) {
                switch (e.ToolbarButton) {
                    case ToolbarButton.Up:
                        name = "Up";
                        break;
                    case ToolbarButton.Down:
                        name = "Down";
                        break;
                    case ToolbarButton.Left:
                        name = "Left";
                        break;
                    case ToolbarButton.Right:
                        name = "Right";
                        break;
                    case ToolbarButton.Session:
                        name = "Session";
                        break;
                    case ToolbarButton.User1:
                        name = "User1";
                        break;
                    case ToolbarButton.User2:
                        name = "User2";
                        break;
                    case ToolbarButton.Mixer:
                        name = "Mixer";
                        break;
                }
            }
            return name;
        }

        private void processShortcut(Shortcut cut) {
            if (cut.hasMods) {
                VirtualKeyCode[] modifiers = new VirtualKeyCode[cut.Mods.Count];
                var cursor = 0;
                foreach (var mod in cut.Mods) {
                    switch (mod) {
                        case "Ctrl":
                            modifiers[cursor] = VirtualKeyCode.CONTROL;
                            cursor++;
                            break;
                        case "Alt":
                            modifiers[cursor] = VirtualKeyCode.MENU;
                            cursor++;
                            break;
                        case "Shift":
                            modifiers[cursor] = VirtualKeyCode.SHIFT;
                            cursor++;
                            break;
                    }
                }
                VirtualKeyCode result;
                bool test;
                if (cut.AssignedKey.Length < 2) {
                    test = VirtualKeyCode.TryParse("VK_" + cut.AssignedKey.ToUpper(), out result);
                } else {
                    test = VirtualKeyCode.TryParse(cut.AssignedKey.ToUpper(), out result);
                }
                if (test) {
                    keyboard.ModifiedKeyStroke(modifiers, result);
                }
            } else {
                VirtualKeyCode result;
                bool test;
                if (cut.AssignedKey.Length < 2) {
                    test = VirtualKeyCode.TryParse("VK_" + cut.AssignedKey.ToUpper(), out result);
                } else {
                    test = VirtualKeyCode.TryParse(cut.AssignedKey.ToUpper(), out result);
                }
                if (test) {
                    keyboard.KeyPress(result);
                }
            }
        }
    }
}
