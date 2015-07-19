using System.Windows;
using System.Windows.Input;

namespace UrsaLabs.L2K.Commands {
    public class ShowApp : CommandBase<ShowApp> {
        public override void Execute(object parameter) {
            GetTaskbarWindow(parameter).Show();
            CommandManager.InvalidateRequerySuggested();
        }

        public override bool CanExecute(object parameter) {
            Window win = GetTaskbarWindow(parameter);
            return win != null && !win.IsVisible;
        }
    }
}