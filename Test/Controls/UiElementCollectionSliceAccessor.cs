using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Diagnostics;

namespace Test;

internal readonly struct UiElementCollectionSliceAccessor
{
    private const string IndexMustBeLessOrEqual = "Index was out of range. Must be non-negative and less than or equal to the size of the collection.";
    private const string IndexMustBeLess = "Index was out of range. Must be non-negative and less than the size of the collection.";
    private const string CollectionNull = "The underlying collection is null.";
    private const string NonNegativeNumber = "Non-negative number required.";
    private const string InvalidOffsetLength = "Offset and count were out of bounds for the collection or count is greater than the number of elements from index to the end of the source collection.";

    public UiElementCollectionSliceAccessor(UIElementCollection collection)
        : this(collection, 0, collection.Count)
    {
    }
    public UiElementCollectionSliceAccessor(
        UIElementCollection collection, 
        int offset,
        int count
    )
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (collection is null || (uint)offset > (uint)collection.Count || (uint)count > (uint)(collection.Count - offset))
            GetException(collection, offset, count).Throw();
        this.collection = collection;
        this.Offset = offset;
        this.Count = count;
    }

    private static Exception GetException(
        UIElementCollection? collection,
        int offset, 
        int count
    )
    {          
        if (collection is null)
            return new ArgumentNullException(nameof(collection));
        if (offset < 0) return new ArgumentOutOfRangeException(nameof(offset), NonNegativeNumber);
        if (count < 0) return new ArgumentOutOfRangeException(nameof(count), NonNegativeNumber);
        return new ArgumentException(InvalidOffsetLength);
    }

    public int Count { get; }

    public UIElement? this[int index] => (uint)index >= (uint)this.Count 
        ? ThrowHelper.ThrowArgumentOutOfRangeException<UIElement?>(nameof(index), index, IndexMustBeLess) 
        : this.Collection[this.Offset + index];

    public int Offset { get; }
    private readonly UIElementCollection? collection;
    public UIElementCollection Collection => this.collection!;

    public Enumerator GetEnumerator() => new Enumerator(this, false);
    public Enumerator GetEnumerator(bool isReverse) => new Enumerator(this, isReverse);
    
    [MemberNotNull("collection")]
    private void ThrowInvalidOperationIfDefault()
    {
        if (this.collection is null)
        {
            ThrowHelper.ThrowInvalidOperationException(CollectionNull);
        }
    }
    public UiElementCollectionSliceAccessor Slice (int index)
    {            
        this.ThrowInvalidOperationIfDefault();
        return (uint)index > (uint)this.Count 
            ? ThrowHelper.ThrowArgumentOutOfRangeException<UiElementCollectionSliceAccessor>(IndexMustBeLessOrEqual) 
            : new (this.collection, this.Offset + index, this.Count - index);
    }
    public UiElementCollectionSliceAccessor Slice (int index, int count)
    {
        this.ThrowInvalidOperationIfDefault();
        return (uint)index > (uint)this.Count || (uint)count > (uint)(this.Count - index)
            ? ThrowHelper.ThrowArgumentOutOfRangeException<UiElementCollectionSliceAccessor>( IndexMustBeLessOrEqual)
            : new(this.collection, this.Offset + index, count);
    }


    
    
    public struct Enumerator
    {
        private int index;
        private readonly int increment;
        private readonly UiElementCollectionSliceAccessor slice;

        public Enumerator(
            UiElementCollectionSliceAccessor slice,
            bool isReverse
        )
        {
            (this.index, this.increment) = isReverse
                ? (slice.Count, -1)
                : (-1, 1);
            this.slice = slice;
            this.current = default;
            this.IsFinished = false;
        }

        private int NextIndex => this.index + this.increment;
        public int Count => this.slice.Count;
        private bool IsInvalidIndex(int proposedIndex) => (uint)proposedIndex >= (uint)this.Count;

        public bool MoveNext()
        {
            if (this.IsInvalidIndex(this.NextIndex))
            {
                this.current = default;
                this.IsFinished = true;
                return false;
            }
            this.index = this.NextIndex;
            this.current = this.slice[this.index];
            return true;
        }
        public bool IsFinished { get; private set; }

        private UIElement? current;
        public UIElement? Current => this.current;
    }

}