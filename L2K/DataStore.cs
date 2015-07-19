using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace UrsaLabs.L2K {
    [Serializable()]
    public class DataStore {

        private Shortcut[,] gridShortcuts;
        private Dictionary<string, Shortcut> sideShortcuts;
        private Dictionary<string, Shortcut> topShortcuts;
        private List<String>[] groups;
        private string docsPath;

        static readonly DataStore _instance = new DataStore();

        private DataStore() {
            docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UrsaLabs\\L2K\\";

            gridShortcuts = new Shortcut[8, 8];
            sideShortcuts = new Dictionary<string, Shortcut>(8);
            topShortcuts = new Dictionary<string, Shortcut>(8);
            groups = new List<string>[4];
            for (var i = 0; i < groups.Length; i++) {
                groups[i] = new List<string>();
            }
        }

        public static DataStore Instance {
            get { return _instance; }
        }

        public DataStore(SerializationInfo info, StreamingContext ctxt) {
            gridShortcuts = (Shortcut[,])info.GetValue("gridShortcuts", typeof(int));
            sideShortcuts = (Dictionary<string, Shortcut>)info.GetValue("sideShortcuts", typeof(Dictionary<string, Shortcut>));
            topShortcuts = (Dictionary<string, Shortcut>)info.GetValue("topShortcuts", typeof(Dictionary<string, Shortcut>));
            groups = (List<String>[])info.GetValue("groups", typeof(List<String>[]));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
            info.AddValue("gridShortcuts", gridShortcuts);
            info.AddValue("sideShortcuts", sideShortcuts);
            info.AddValue("topShortcuts", topShortcuts);
            info.AddValue("groups", groups);
        }

        public Shortcut GetShortcut(int x, int y) {
            return gridShortcuts[x, y];
        }

        public void SetShortcut(int x, int y, Shortcut shortcut) {
            gridShortcuts[x, y] = shortcut;
        }

        public Shortcut GetSideShortcut(string name) {
            Shortcut result;
            var success = sideShortcuts.TryGetValue(name, out result);
            if (success) {
                return result;
            } else {
                return null;
            }
        }

        public void SetSideShortcut(string name, Shortcut shortcut) {
            sideShortcuts[name] = shortcut;
        }

        public Shortcut GetTopShortcut(string name) {
            Shortcut result;
            var success = topShortcuts.TryGetValue(name, out result);
            if (success) {
                return result;
            } else {
                return null;
            }
        }

        public void SetTopShortcut(string name, Shortcut shortcut) {
            topShortcuts[name] = shortcut;
        }

        public void addToGroup(int group, string gridName) {
            if (group == 0) {
                return;
            }
            groups[group].Add(gridName);
        }

        public void removeFromGroup(int group, string gridName) {
            if (group == 0) {
                return;
            }
            groups[group].Remove(gridName);
        }

        public List<String> getGroup(int group) {
            return groups[group];
        } 

        public void Save() {
            if (!Directory.Exists(docsPath)) {
                Directory.CreateDirectory(docsPath);
            }
            Stream stream = File.Open(docsPath+"launchpadkb.ulf", FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, _instance);
            stream.Close();
        }

        public void Load() {
            try {
                Stream stream = File.Open(docsPath+"launchpadkb.ulf", FileMode.Open);
                BinaryFormatter bformatter = new BinaryFormatter();

                DataStore tmpLoad = (DataStore)bformatter.Deserialize(stream);
                gridShortcuts = tmpLoad.gridShortcuts;
                topShortcuts = tmpLoad.topShortcuts;
                sideShortcuts = tmpLoad.sideShortcuts;
                groups = tmpLoad.groups;
                stream.Close();
            } catch {
                //nothing to do we just continue using the new fresh data
                return;
            }
        }

    }
}
