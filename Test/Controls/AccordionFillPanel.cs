using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Test;

public class AccordionFillPanel : DockPanel
{
    private Accordion? Accordion => this.TemplatedParent switch
    {
        Accordion accordion => accordion,
        ItemsPresenter presenter => presenter.FindAncestor<Accordion>(),
        _ => null,
    };
    private bool IsHorizontal => this.Accordion?.ExpandDirection is ExpandDirection.Left or ExpandDirection.Right;
    private int SelectedIndex => this.Accordion?.SelectedIndex ?? -1;

    private readonly record struct ChildWithDock(
        UIElement? Child,
        Dock? Dock
    );

    private ChildEnumerator GetChildren() => new ChildEnumerator(this.InternalChildren, this.SelectedIndex, this.IsHorizontal);



    protected override Size MeasureOverride(Size constraint) 
        => MeasureChildren(this.GetChildren(), constraint);


    protected override Size ArrangeOverride(Size arrangeSize) 
        => ArrangeChildren(this.GetChildren(), arrangeSize);


    
    
    private static Size MeasureChildren(
        ChildEnumerator children,
        Size constraint
    )
    {
        double parentWidth = 0; // Our current required width due to children thus far.
        double parentHeight = 0; // Our current required height due to children thus far.
        double accumulatedWidth = 0; // Total width consumed by children.
        double accumulatedHeight = 0; // Total height consumed by children.

        foreach(var (child, dock) in children)
        {
            Size childConstraint; // Contains the suggested input constraint for this child.
            Size childDesiredSize; // Contains the return size from child measure.

            if (child is null)
            {
                continue;
            }

            // Child constraint is the remaining size; this is total size minus size consumed by previous children.
            childConstraint = new Size(Math.Max(0.0, constraint.Width - accumulatedWidth),
                Math.Max(0.0, constraint.Height - accumulatedHeight));

            // Measure child.
            child.Measure(childConstraint);
            childDesiredSize = child.DesiredSize;

            // Now, we adjust:
            // 1. Size consumed by children (accumulatedSize).  This will be used when computing subsequent
            //    children to determine how much space is remaining for them.
            // 2. Parent size implied by this child (parentSize) when added to the current children (accumulatedSize).
            //    This is different from the size above in one respect: A Dock.Left child implies a height, but does
            //    not actually consume any height for subsequent children.
            // If we accumulate size in a given dimension, the next child (or the end conditions after the child loop)
            // will deal with computing our minimum size (parentSize) due to that accumulation.
            // Therefore, we only need to compute our minimum size (parentSize) in dimensions that this child does
            //   not accumulate: Width for Top/Bottom, Height for Left/Right.
            switch (dock)
            {
                case Dock.Left:
                case Dock.Right:
                    parentHeight = Math.Max(parentHeight, accumulatedHeight + childDesiredSize.Height);
                    accumulatedWidth += childDesiredSize.Width;
                    break;

                case Dock.Top:
                case Dock.Bottom:
                    parentWidth = Math.Max(parentWidth, accumulatedWidth + childDesiredSize.Width);
                    accumulatedHeight += childDesiredSize.Height;
                    break;
            }
        }

        // Make sure the final accumulated size is reflected in parentSize.
        parentWidth = Math.Max(parentWidth, accumulatedWidth);
        parentHeight = Math.Max(parentHeight, accumulatedHeight);

        return new Size(parentWidth, parentHeight);
    }
    

    private static Size ArrangeChildren(
        ChildEnumerator children,
        Size arrangeSize
    )
    {
        double accumulatedLeft = 0;
        double accumulatedTop = 0;
        double accumulatedRight = 0;
        double accumulatedBottom = 0;

        foreach(var (child, dock) in children)
        {
            if (child is null)
            {
                continue;
            }

            var childDesiredSize = child.DesiredSize;
            var rcChild = new Rect(
                accumulatedLeft,
                accumulatedTop,
                Math.Max(0.0, arrangeSize.Width - (accumulatedLeft + accumulatedRight)),
                Math.Max(0.0, arrangeSize.Height - (accumulatedTop + accumulatedBottom)));

            switch (dock)
            {
                case Dock.Left:
                    accumulatedLeft += childDesiredSize.Width;
                    rcChild.Width = childDesiredSize.Width;
                    break;

                case Dock.Right:
                    accumulatedRight += childDesiredSize.Width;
                    rcChild.X = Math.Max(0.0, arrangeSize.Width - accumulatedRight);
                    rcChild.Width = childDesiredSize.Width;
                    break;

                case Dock.Top:
                    accumulatedTop += childDesiredSize.Height;
                    rcChild.Height = childDesiredSize.Height;
                    break;

                case Dock.Bottom:
                    accumulatedBottom += childDesiredSize.Height;
                    rcChild.Y = Math.Max(0.0, arrangeSize.Height - accumulatedBottom);
                    rcChild.Height = childDesiredSize.Height;
                    break;
            }

            child.Arrange(rcChild);
        }
        return arrangeSize;
    }


    private enum ItemPosition
    {
        Before,
        After,
        At,
    }
    

    private struct ChildEnumerator
    {
        private readonly bool isHorizontal;
        private UiElementCollectionSliceAccessor.Enumerator before;
        private UiElementCollectionSliceAccessor.Enumerator after;
        private UiElementCollectionSliceAccessor.Enumerator at;

        public ChildEnumerator(UIElementCollection children, int selectedIndex, bool isHorizontal)
        {
            this.isHorizontal = isHorizontal;
            selectedIndex = selectedIndex is not -1 ? selectedIndex : 0;
            (this.before, this.at, this.after) = GetEnumerators(new (children), selectedIndex);
            this.current = default;


        }

        private static (
            UiElementCollectionSliceAccessor.Enumerator Before,
            UiElementCollectionSliceAccessor.Enumerator At,
            UiElementCollectionSliceAccessor.Enumerator After
        ) GetEnumerators(UiElementCollectionSliceAccessor accessor, int selectedIndex)
        {
            return (accessor.Count, SelectedIndex: selectedIndex) switch
            {
                (Count: <= 0, SelectedIndex: _) => default,
                (Count: _, SelectedIndex: 0) => (
                    Before: default,
                    At: accessor[..1].GetEnumerator(),
                    After: accessor[1..].GetEnumerator(isReverse: true)
                ),
                (Count: _, SelectedIndex: _) when selectedIndex == accessor.Count - 1 => (
                    Before: accessor[..selectedIndex].GetEnumerator(),
                    At: accessor[^1..].GetEnumerator(),
                    After: default
                ),
                (Count: _, SelectedIndex: _) => (
                    Before: accessor[..selectedIndex].GetEnumerator(),
                    At: accessor[selectedIndex..(selectedIndex + 1)].GetEnumerator(),
                    After: accessor[(selectedIndex + 1)..].GetEnumerator(isReverse: true)
                ),
            };
        }

        private ChildWithDock current;
        public ChildWithDock Current => this.current;

        public bool MoveNext()
        {
            return this.TryOne(ref this.before, ItemPosition.Before)
                || this.TryOne(ref this.after, ItemPosition.After)
                || this.TryOne(ref this.at, ItemPosition.At);


        }

        private bool TryOne(
            ref UiElementCollectionSliceAccessor.Enumerator enumerator,
            ItemPosition itemPosition
        )
        {
            if (enumerator.IsFinished || !enumerator.MoveNext())
            {
                return false;
            }
            this.current = new (enumerator.Current, GetDock(itemPosition, this.isHorizontal));
            return true;
        }


        private static Dock? GetDock(ItemPosition position, bool isHorizontal) => (position, isHorizontal) switch
        {
            (position: not (ItemPosition.After or ItemPosition.Before), isHorizontal: _) => null,
            (position: ItemPosition.Before, isHorizontal: true) => Dock.Left,
            (position: ItemPosition.After, isHorizontal: true) => Dock.Right,
            (position: ItemPosition.Before, isHorizontal: false) => Dock.Top,
            (position: ItemPosition.After, isHorizontal: false) => Dock.Bottom,
        };

        public ChildEnumerator GetEnumerator() => this;
    }
}