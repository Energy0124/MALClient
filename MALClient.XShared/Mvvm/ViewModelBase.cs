using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MALClient.XShared.Mvvm
{
    /// <summary>
    ///     MvvmLight compatible view model base on top of CommunityToolkit.Mvvm, so existing view models
    ///     keep their RaisePropertyChanged(() => Property) calls.
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }

        public virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            OnPropertyChanged(GetPropertyName(propertyExpression));
        }

        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            return SetProperty(ref field, newValue, propertyName);
        }

        protected bool Set<T>(Expression<Func<T>> propertyExpression, ref T field, T newValue)
        {
            return SetProperty(ref field, newValue, GetPropertyName(propertyExpression));
        }

        protected bool Set<T>(string propertyName, ref T field, T newValue)
        {
            return SetProperty(ref field, newValue, propertyName);
        }

        protected static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            var body = propertyExpression.Body is UnaryExpression unary ? unary.Operand : propertyExpression.Body;
            if (body is MemberExpression member)
                return member.Member.Name;

            throw new ArgumentException("Expression must point to a property.", nameof(propertyExpression));
        }
    }
}
