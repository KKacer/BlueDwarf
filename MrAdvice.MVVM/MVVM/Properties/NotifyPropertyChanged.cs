﻿
namespace ArxOne.MrAdvice.MVVM.Properties
{
    using System;
    using System.Linq;
    using Advice;
    using Annotation;
    using ViewModel;
    using Utility;

    /// <summary>
    /// Invokes notify property changed... If the property has actually changed
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [Priority(AspectPriority.Notification)]
    public class NotifyPropertyChanged : Attribute, IPropertyAdvice
    {
        public void Advise(PropertyAdviceContext context)
        {
            if (context.IsGetter)
                context.Proceed();
            else
            {
                var oldValue = context.TargetProperty.GetValue(context.Target, context.Index.ToArray());

                // first, set the value
                context.Proceed();

                // then, notify, if it has changed
                var newValue = context.Value;
                if (!oldValue.SafeEquals(newValue))
                {
                    var viewModel = context.Target as INotifyPropertyChangedViewModel;
                    if (viewModel == null)
                        throw new InvalidOperationException("ViewModel must implement INotifyPropertyChangedViewModel");
                    viewModel.OnPropertyChanged(context.TargetProperty, this);
                }
            }
        }
    }
}