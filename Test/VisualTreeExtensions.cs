using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Test;

public static class VisualTreeExtensions
{
    private static TReturn SetFieldAndReturn<TField, TReturn>(
        // ReSharper disable once RedundantAssignment
        ref TField field,
        TField value,
        TReturn returnValue
    )
    {
        field = value;
        return returnValue;
    }
    
    private static bool Is<T>(this object? value, [NotNullWhen(true)] out T? result)
        where T : class
    {
        result = default;
        if (value is not T typed) return false;
        result = typed;
        return true;
    }
    
    public static VisualChildrenEnumerator GetVisualChildren(this DependencyObject? parent) => new (parent);
    
    private static bool CheckOne<T, TArg>(
        DependencyObject? item,
        Func<T, TArg, bool>? predicate,
        TArg argument,
        [NotNullWhen(true)] out T? found
    )
        where T : DependencyObject 
        => item.Is(out found) && predicate?.Invoke(found, argument) is null or true;


    private static bool TryFindAncestor<T, TArg>(
        this DependencyObject? current, 
        Func<T, TArg, bool>? predicate,
        TArg argument,
        [NotNullWhen(true)] out T? ancestor
    )
        where T : DependencyObject
    {
        ancestor = default;
        while (current is not null)
        {
            current = VisualTreeHelper.GetParent(current);
            if(CheckOne(current, predicate, argument, out ancestor))
            {
                return true;
            }
        }
        return false;
    }


    
    public static bool TryFindAncestor<T>(
        this DependencyObject? current, 
        [NotNullWhen(true)] out T? ancestor
    )
        where T : DependencyObject
    {
        return current.TryFindAncestor<T, object?>(
            predicate: null,
            argument: null, 
            out ancestor
        );
    }

    public static bool TryFindAncestor<T>(
        this DependencyObject? current, 
        T lookupItem, 
        [NotNullWhen(true)] out T? ancestor
    )
        where T : DependencyObject
    {
        return current.TryFindAncestor(
            predicate: static (foundItem, lookupItem) => ReferenceEquals(foundItem, lookupItem),
            argument: lookupItem,
            out ancestor
        );
    }
    
    public static bool TryFindAncestor<T>(
        this DependencyObject? current, 
        string? parentName, 
        [NotNullWhen(true)] out T? ancestor
    )
        where T : FrameworkElement
    {
        Func<T, string, bool>? predicate = string.IsNullOrEmpty(parentName)
            ? null
            : static (foundItem, parentName) => foundItem.Name == parentName;
        parentName ??= string.Empty;
        return current.TryFindAncestor(
            predicate: predicate,
            argument: parentName,
            out ancestor
        );
    }

    public static T? FindAncestor<T>(
        this DependencyObject? current
    )
        where T : DependencyObject 
        => current.TryFindAncestor<T>(out var ancestor) ? ancestor : null;

    public static T? FindAncestor<T>(
        this DependencyObject? current,
        T lookupItem
    )
        where T : DependencyObject 
        => current.TryFindAncestor(lookupItem, out var ancestor) ? ancestor : null;

    public static T? TryFindAncestor<T>(
        this DependencyObject current,
        string parentName
    )
        where T : FrameworkElement
        => current.TryFindAncestor<T>(parentName, out var ancestor) ? ancestor : null;
    
    
    
    private static bool TryFindChild<T, TArg>(
        this DependencyObject? parent,
        Func<T, TArg, bool>? predicate,
        TArg argument,
        [NotNullWhen(true)] out T? foundChild
    ) where T : DependencyObject
    {
        foundChild = default;
        foreach (var child in parent.GetVisualChildren())
        {
            if (CheckOne(child, predicate, argument, out foundChild)
                || child.TryFindChild(predicate, argument, out foundChild))
            {
                return true;
            }
        }
        return false;
    }

    public static bool TryFindChild<T>(
        this DependencyObject? parent,
        [NotNullWhen(true)] out T? foundChild
    ) where T : DependencyObject
    {
        return parent.TryFindChild<T, object?>(
            predicate: null,
            argument: null,
            out foundChild
        );
    }

    public static bool TryFindChild<T>(
        this DependencyObject? current, 
        string? parentName, 
        [NotNullWhen(true)] out T? ancestor
    )
        where T : FrameworkElement
    {
        Func<T, string, bool>? predicate = string.IsNullOrEmpty(parentName)
            ? null
            : static (foundItem, childName) => foundItem.Name == childName;
        parentName ??= string.Empty;
        return current.TryFindChild(
            predicate: predicate,
            argument: parentName,
            out ancestor
        );
    }
    
    
    public static T? FindChild<T>(this DependencyObject? parent)
        where T : DependencyObject 
        => TryFindChild<T>(parent, out var foundChild) ? foundChild : null;
    
    public static T? FindChild<T>(this DependencyObject? parent, string name)
        where T : FrameworkElement 
        => TryFindChild<T>(parent, name, out var foundChild) ? foundChild : null;

    public struct VisualChildrenEnumerator
    {
        private DependencyObject? parent;
        private readonly int childrenCount;
        private int index;

        public VisualChildrenEnumerator(DependencyObject? parent)
        {
            this.parent = parent;
            this.childrenCount = parent is null ? 0 : VisualTreeHelper.GetChildrenCount(parent);
            this.index = -1;
            this.current = null;
        }

        public VisualChildrenEnumerator GetEnumerator() => this;

        public bool MoveNext() => this.parent switch
        {
            null => false,
            not null when this.TryIncrementIndex() is false => SetFieldAndReturn(ref this.parent, null, false),
            not null => SetFieldAndReturn(ref this.current, VisualTreeHelper.GetChild(this.parent, this.index), true)
        };


        private bool TryIncrementIndex() => ++this.index < this.childrenCount;

        private DependencyObject? current;

        public DependencyObject? Current => this.current;
    }

}