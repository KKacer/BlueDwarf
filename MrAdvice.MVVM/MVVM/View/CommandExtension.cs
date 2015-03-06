﻿
namespace ArxOne.MrAdvice.MVVM.View
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using Utility;

    /// <summary>
    /// Allows to bind commands directly to view-model methods
    /// Syntax: Command="{controls:Command {Binding methodName}}"
    /// </summary>
    public class CommandExtension : MarkupExtension
    {
        private readonly object _name;

        public object Parameter { get; set; }

        public CommandExtension(object name)
        {
            _name = name;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            var element = provideValueTarget.TargetObject as FrameworkElement;
            if (element == null)
                return null;

            var targetProperty = provideValueTarget.TargetProperty;
            element.DataContextChanged += delegate
            {
                var viewModel = element.DataContext;
                if (viewModel == null)
                    return;

                var name = _name;
                var bindingParameter = name as Binding;
                // because we bind to a method, this allows us to have a syntax control in XAML editor
                if (bindingParameter != null)
                    name = viewModel.GetType().GetMember(bindingParameter.Path.Path).FirstOrDefault();

                var command = new Command(viewModel, name);
                element.SetCommand(targetProperty, command, Parameter);
            };

            return null;
        }
    }
}