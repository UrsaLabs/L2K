using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;

namespace UrsaLabs.L2K.Commands {
    public abstract class CommandBase<T> : MarkupExtension, ICommand
        where T : class, ICommand, new() {
        private static T command;

        public override object ProvideValue(IServiceProvider serviceProvider) {
            if (command == null) command = new T();
            return command;
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public abstract void Execute(object parameter);

        public virtual bool CanExecute(object parameter) {
            return IsDesignMode ? false : true;
        }

        public static bool IsDesignMode {
            get {
                return (bool)
                    DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                        typeof(FrameworkElement))
                        .Metadata.DefaultValue;
            }
        }
        
        protected Window GetTaskbarWindow(object commandParameter) {
            if (IsDesignMode) return null;

            var tb = commandParameter as TaskbarIcon;
            return tb == null ? null : TryFindParent<Window>(tb);
        }

        #region TryFindParent helper

        public static T TryFindParent<T>(DependencyObject child)
            where T : DependencyObject {
            DependencyObject parentObject = GetParentObject(child);
            
            if (parentObject == null) return null;
            
            T parent = parentObject as T;
            if (parent != null) {
                return parent;
            } else {
                return TryFindParent<T>(parentObject);
            }
        }

        public static DependencyObject GetParentObject(DependencyObject child) {
            if (child == null) return null;
            ContentElement contentElement = child as ContentElement;

            if (contentElement != null) {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            return VisualTreeHelper.GetParent(child);
        }

        #endregion
    }
}