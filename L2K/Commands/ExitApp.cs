using System.Windows;
using System.Windows.Input;

namespace UrsaLabs.L2K.Commands {
    public class ExitApp : CommandBase<ExitApp> {
        public override void Execute(object parameter) {
            GetTaskbarWindow(parameter).Close();
            CommandManager.InvalidateRequerySuggested();
        }

        public override bool CanExecute(object parameter) {
            Window win = GetTaskbarWindow(parameter);
            return win != null;
        }
    }
}