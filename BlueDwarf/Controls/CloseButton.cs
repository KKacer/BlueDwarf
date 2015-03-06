﻿// This is the blue dwarf
// more information at https://github.com/picrap/BlueDwarf
namespace BlueDwarf.Controls
{
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using ArxOne.MrAdvice.Utility;
    using Utility;

    public class CloseButton : FrameworkElement
    {
        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public CloseButton()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var parent = this.GetVisualSelfAndParents().OfType<Window>().FirstOrDefault();
            if (parent != null)
                parent.Closing += OnParentClosing;
        }

        private void OnParentClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }
    }
}
