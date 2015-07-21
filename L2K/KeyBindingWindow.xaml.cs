using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UrsaLabs.L2K {
    /// <summary>
    /// Interaction logic for KeyBindingWindow.xaml
    /// </summary>
    public partial class KeyBindingWindow : Window {

        private Shortcut shortcut;
        private Shortcut tempShortcut;
        private Shortcut existingShortcut;
        private DataStore data;
        private bool isGrid;
        private bool isTop;
        private bool isToggled;
        private int group;
        private int x;
        private int y;
        private string name;


        public KeyBindingWindow(Shortcut cut, string btnName, bool top) { 
            data = DataStore.Instance;
            isTop = top;
            name = btnName;
            InitializeComponent();
            if (isTop) {
                toggleBox.IsChecked = false;
                toggleBox.Visibility = Visibility.Hidden;
            }
            isToggled = false;
            groupBox.SelectedIndex = 0;
            groupBox.Visibility = Visibility.Hidden;
            groupLabel.Visibility = Visibility.Hidden;
            group = groupBox.SelectedIndex;
            if (cut != null) {
                existingShortcut = cut;
                tempShortcut = cut;
                string textBoxVal = "";
                if (cut.hasMods) {
                    textBoxVal = cut.Mods.Aggregate(textBoxVal, (current, mod) => current + (mod + " + "));
                }
                textBoxVal += cut.AssignedKey;
                keyboardInput.Text = textBoxVal;
                isToggled = cut.isToggle;
                toggleBox.IsChecked = isToggled;
            }
        }

        public KeyBindingWindow(Shortcut cut, int xG, int yG) {
            data = DataStore.Instance;
            x = xG;
            y = yG;
            name = y.ToString() + x;
            isGrid = true;
            InitializeComponent();
            isToggled = false;
            if (cut != null) {
                existingShortcut = cut;
                tempShortcut = cut;
                string textBoxVal = "";
                if (cut.hasMods) {
                    textBoxVal = cut.Mods.Aggregate(textBoxVal, (current, mod) => current + (mod + " + "));
                }
                textBoxVal += cut.AssignedKey;
                keyboardInput.Text = textBoxVal;
                groupBox.SelectedIndex = cut.belongsToGroup;
                group = cut.belongsToGroup;
                isToggled = cut.isToggle;
                toggleBox.IsChecked = isToggled;
            }
        }

        public Shortcut KeyBinding => shortcut;

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            shortcut = tempShortcut;
            group = groupBox.SelectedIndex;
            if (shortcut != null) {
                if (existingShortcut != null) {
                    var oldGroup = existingShortcut.belongsToGroup;
                    if (oldGroup > 0) {
                        data.removeFromGroup(oldGroup, name);
                    }
                }
                isToggled = toggleBox.IsChecked == true;
                shortcut.belongsToGroup = group;
                shortcut.isToggle = isToggled;
                DialogResult = true;
            } else {
                DialogResult = false;
            }
        }

        private void keyboardInput_KeyDown(object sender, KeyEventArgs e) {
            var modifiers = Keyboard.Modifiers;
            var keys = e.Key;
            if (e.Key != Key.None && e.Key != Key.Back && e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl && e.Key != Key.LeftAlt && e.Key != Key.RightAlt && e.Key != Key.LeftShift && e.Key != Key.RightShift) {
                if (e.Key == Key.System && e.SystemKey == Key.F10) {
                    keys = Key.F10;
                }
                isToggled = toggleBox.IsChecked == true;
                group = groupBox.SelectedIndex;
                var cut = new Shortcut(keys, modifiers, isToggled, group);
                tempShortcut = cut;
                string textBoxVal = "";
                if (cut.hasMods) {
                    foreach (var mod in cut.Mods) {
                        textBoxVal += mod + " + ";
                    }
                }
                var keyString = cut.AssignedKey;
                if (keyString.Length == 2 && keyString[0].ToString().ToLower() == "d") {
                    keyString = keyString[1].ToString();
                }
                textBoxVal += keyString;
                keyboardInput.Text = textBoxVal;
                e.Handled = true;
            } else {
                e.Handled = false;
                if (e.Key == Key.Back) {
                    if (isGrid) {
                        data.SetShortcut(x, y, null);
                    } else if (isTop) {
                        data.SetTopShortcut(name, null);
                    } else {
                        data.SetSideShortcut(name, null);
                    }
                    keyboardInput.Text = "";
                    tempShortcut = null;
                    data.Save();
                }
            }

        }
    }
}
