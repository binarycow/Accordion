using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Expression = System.Linq.Expressions.Expression;

namespace Test;

internal static class Extensions
{
    [DoesNotReturn]
    public static void Throw(this Exception exception)
    {
        throw exception;
    }
    
    public static void RaiseEvent(
        this UIElement uiElement,
        RoutedEvent routedEvent
    )
    {
        uiElement.RaiseEvent(new (routedEvent));
    }
    
    [return: NotNullIfNotNull("specifiedDefault")]
    public static T? GetValue<T>(
        this DependencyObject obj,
        DependencyProperty property,
        T? specifiedDefault = default
    )
    {
        if (obj.GetValue(property) is T typed)
            return typed;
        return specifiedDefault;
    }
    
    public static T SetValue<T>(
        this DependencyObject obj,
        DependencyProperty property,
        T value
    )
    {
        obj.SetValue(property, value!);
        return value;
    }



    public static IEnumerable<T> Select<T>(
        this UIElementCollection uiElementCollection,
        Func<UIElement, T> selector
    )
    {
        foreach (UIElement element in uiElementCollection)
        {
            yield return selector(element);
        }
    }

    public static bool Is<T>(T? value, [MaybeNullWhen(false)] out T result)
        where T : class
    {
        result = value;
        return result is not null;
    }
    
    public static bool Is<T>(object? value, [MaybeNullWhen(false)] out T result)
    {
        result = default;
        if (value is not T typed)
            return false;
        result = typed;
        return true;
    }

    private static bool TryGetVisualChild<T>(
        this DependencyObject? visual,
        int index,
        [NotNullWhen(true)] out T? result
    ) where T : DependencyObject
    {
        result = default;
        var child = visual is null ? null : VisualTreeHelper.GetChild(visual, index);
        return child switch
        {
            null => Is<T>(null, out result),
            T typedChild => Is<T>(typedChild, out result),
            _ => TryGetVisualChild(child, out result),
        };
    }
    
    public static bool TryGetVisualChild<T>(
        this DependencyObject visual,
        [NotNullWhen(true)] out T? result
    ) where T : DependencyObject
    {
        result = default;
        var childrenCount = VisualTreeHelper.GetChildrenCount(visual);
        for (var index = 0; index < childrenCount; index++)
        {
            if (visual.TryGetVisualChild(index, out result))
                return true;
        }
        return false;
    }
    
    public static T? GetVisualChild<T>(this DependencyObject visual) 
        where T : DependencyObject 
        => visual.TryGetVisualChild<T>(out var result) ? result : default;

    private static readonly Lazy<Func<ItemsControl, Panel?>> getItemsHost
        = new (CreateGetItemsHostLambda);
    
    private static Func<ItemsControl, Panel?> CreateGetItemsHostLambda()
    {
        var parameter = Expression.Parameter(type: typeof(ItemsControl));
        
        return Expression.Lambda<Func<ItemsControl, Panel>>(
            body: Expression.Property(
                expression: parameter, 
                property: typeof(ItemsControl).GetProperty(
                    name: "ItemsHost",
                    bindingAttr: BindingFlags.NonPublic 
                        | BindingFlags.GetProperty 
                        | BindingFlags.Instance
                )!
            ),
            parameter
        ).Compile();
    }
    
    public static Panel? GetItemsPanel(this ItemsControl itemsControl) 
        => getItemsHost.Value(itemsControl);
    

}