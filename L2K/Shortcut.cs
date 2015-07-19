using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UrsaLabs.L2K {
    [Serializable()]
    public class Shortcut {

        private List<String> modsList;

        public Shortcut(Key key, ModifierKeys modifiers, bool toggles, int group) {
            AssignedKey = key.ToString();
            isToggle = toggles;
            belongsToGroup = group;
            if (modifiers != ModifierKeys.None) {
                hasMods = true;
                modsList = new List<string>();
                if (modifiers.HasFlag(ModifierKeys.Control)) {
                    modsList.Add("Ctrl");
                }
                if (modifiers.HasFlag(ModifierKeys.Alt)) {
                    modsList.Add("Alt");
                }
                if (modifiers.HasFlag(ModifierKeys.Shift)) {
                    modsList.Add("Shift");
                }
            } else {
                hasMods = false;
            }
        }

        public Shortcut(SerializationInfo info, StreamingContext ctxt) {
            AssignedKey = (String)info.GetValue("AssignedKey", typeof(string));
            hasMods = (bool)info.GetValue("hasMods", typeof(bool));
            modsList = (List<String>)info.GetValue("modsList", typeof(List<String>));
            isToggle = (bool)info.GetValue("isToggle", typeof(bool));
            belongsToGroup = (int)info.GetValue("belongsToGroup", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
            info.AddValue("AssignedKey", AssignedKey);
            info.AddValue("hasMods", hasMods);
            info.AddValue("modsList", modsList);
            info.AddValue("isToggle", isToggle);
            info.AddValue("belongsToGroup", belongsToGroup);
        }

        public bool hasMods { get; }

        public bool isToggle { get; set; }

        public int belongsToGroup { get; set; }

        public string AssignedKey { get; }

        public List<string> Mods {
            get { return modsList; }
        }
    }
}
