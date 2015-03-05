﻿// This is the blue dwarf
// more information at https://github.com/picrap/BlueDwarf
namespace BlueDwarf.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    public static class UIElementExtensions
    {
        public static IEnumerable<DependencyObject> GetVisualSelfAndParents(this DependencyObject dependencyObject)
        {
            while (dependencyObject != null)
            {
                yield return dependencyObject;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
        }

        public static IEnumerable<DependencyObject> GetLogicalSelfAndParents(this DependencyObject dependencyObject)
        {
            while (dependencyObject != null)
            {
                yield return dependencyObject;
                dependencyObject = LogicalTreeHelper.GetParent(dependencyObject);
            }
        }

        public static bool SetCommand(this UIElement uiElement, object targetProperty, ICommand command, object commandParameter)
        {
            return SetCommandAndParameter(uiElement, targetProperty, () => command, () => commandParameter);
        }

        private static bool SetCommandAndParameter(this UIElement uiElement, object targetProperty, Func<ICommand> commandSetter, Func<object> commandParameterSetter)
        {
            string propertyName = null;
            var dependencyProperty = targetProperty as DependencyProperty;
            if (dependencyProperty != null)
                propertyName = dependencyProperty.Name;
            var propertyInfo = targetProperty as PropertyInfo;
            if (propertyInfo != null)
                propertyName = propertyInfo.Name;

            if (propertyName == null)
                return false;


            if (commandSetter != null)
            {
                var commandProperty = uiElement.GetType().GetProperty(propertyName);
                if (commandProperty == null)
                    return false;
                commandProperty.SetValue(uiElement, commandSetter(), new object[0]);
            }

            if (commandParameterSetter != null)
            {
                var commandParameterProperty = uiElement.GetType().GetProperty(propertyName + "Parameter");
                if (commandParameterProperty == null)
                    return false;
                commandParameterProperty.SetValue(uiElement, commandParameterSetter(), new object[0]);
            }

            return true;
        }
    }
}
