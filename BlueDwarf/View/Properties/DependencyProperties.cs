﻿// This is the blue dwarf
// more information at https://github.com/picrap/BlueDwarf
namespace BlueDwarf.View.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows;
    using Utility;
    using ViewModel.Properties;

    /// <summary>
    /// This class holds all auto DependencyProperties, grouped by control type
    /// </summary>
    public static class DependencyProperties
    {
        private static readonly IDictionary<Type, IDictionary<string, System.Windows.DependencyProperty>> RegisteredTypes = new Dictionary<Type, IDictionary<string, System.Windows.DependencyProperty>>();

        /// <summary>
        /// Gets the dependency property matching the given PropertyInfo.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns></returns>
        public static System.Windows.DependencyProperty GetDependencyProperty(this PropertyInfo propertyInfo)
        {
            var dependencyProperties = GetDependencyProperties(propertyInfo);
            System.Windows.DependencyProperty property;
            dependencyProperties.TryGetValue(propertyInfo.Name, out property);
            return property;
        }

        /// <summary>
        /// Creates the dependency property related to a property.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="defaultValue">The default value (null if none).</param>
        /// <param name="notification">The notification type, in order to have a callback.</param>
        public static void CreateDependencyProperty(this PropertyInfo propertyInfo, object defaultValue, DependencyPropertyNotification notification)
        {
            var dependencyProperties = GetDependencyProperties(propertyInfo);
            var ownerType = propertyInfo.DeclaringType;
            var propertyName = propertyInfo.Name;
            var defaultPropertyValue = defaultValue ?? propertyInfo.PropertyType.Default();
            var onPropertyChanged = GetPropertyChangedCallback(notification, propertyName, ownerType);
            if (propertyInfo.IsStatic())
            {
                // property type is very specific here, because it comes from the second argument of the generic
                var propertyType = propertyInfo.PropertyType.GetGenericArguments()[1];
                dependencyProperties[propertyName] = System.Windows.DependencyProperty.RegisterAttached(propertyName, propertyType, ownerType,
                    new PropertyMetadata(defaultPropertyValue ?? ObjectTypeConverter.CreateDefault(propertyType), onPropertyChanged));
            }
            else
            {
                dependencyProperties[propertyName] = System.Windows.DependencyProperty.Register(propertyName, propertyInfo.PropertyType, ownerType,
                    new PropertyMetadata(defaultPropertyValue ?? ObjectTypeConverter.CreateDefault(propertyInfo.PropertyType), onPropertyChanged));
            }
        }

        /// <summary>
        /// Gets the property changed callback, based on notification type.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="ownerType">Type of the owner.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">notification</exception>
        private static PropertyChangedCallback GetPropertyChangedCallback(DependencyPropertyNotification notification, string propertyName, Type ownerType)
        {
            switch (notification)
            {
                case DependencyPropertyNotification.None:
                    return null;
                case DependencyPropertyNotification.OnPropertyNameChanged:
                    return GetOnPropertyNameChangedCallback(propertyName, ownerType);
                default:
                    throw new ArgumentOutOfRangeException("notification");
            }
        }

        private static PropertyChangedCallback GetOnPropertyNameChangedCallback(string propertyName, Type ownerType)
        {
            PropertyChangedCallback onPropertyChanged;
            var methodName = string.Format("On{0}Changed", propertyName);
            var method = ownerType.GetMethod(methodName);
            if (method == null)
                throw new InvalidOperationException("Callback method not found (WTF?)");
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
                onPropertyChanged = delegate(DependencyObject d, DependencyPropertyChangedEventArgs e) { method.Invoke(d, new object[0]); };
            else
                throw new InvalidOperationException("Unhandled method overload");
            return onPropertyChanged;
        }

        /// <summary>
        /// Gets the dependency properties group, based on property.
        /// </summary>
        /// <param name="propertyInfo">The property.</param>
        /// <returns></returns>
        private static IDictionary<string, System.Windows.DependencyProperty> GetDependencyProperties(PropertyInfo propertyInfo)
        {
            IDictionary<string, System.Windows.DependencyProperty> dependencyProperties;
            var ownerType = propertyInfo.DeclaringType;
            if (!RegisteredTypes.TryGetValue(ownerType, out dependencyProperties))
                RegisteredTypes[ownerType] = dependencyProperties = new Dictionary<string, System.Windows.DependencyProperty>();
            return dependencyProperties;
        }
    }
}
