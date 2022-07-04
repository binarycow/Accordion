using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Test;


public class Accordion : Selector
{
    static Accordion()
    {
        DefaultStyleKeyProperty?.OverrideMetadata(
            typeof(Accordion),
            new FrameworkPropertyMetadata(typeof(Accordion))
        );
        SelectedIndexProperty.OverrideMetadata(
            typeof(Accordion),
            new FrameworkPropertyMetadata(OnSelectedIndexChanged)
        );
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Accordion accordion && e.NewValue is -1 && accordion.Items.Count > 0)
        {
            accordion.SelectedIndex = 0;
        }
    }


    public Accordion()
    {
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.SelectedIndex = this.Items.Count is 0 ? -1 : 0;
        
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }



    #region Panel

    protected Panel? Panel => this.GetItemsPanel();

    #endregion Panel

    protected override void OnItemsPanelChanged(ItemsPanelTemplate oldItemsPanel, ItemsPanelTemplate newItemsPanel)
    {
        base.OnItemsPanelChanged(oldItemsPanel, newItemsPanel);
        this.UpdateItemsPanelProperties();
    }

    protected void UpdateItemsPanelProperties()
        => UpdateItemsPanelProperties(this.Panel);
    
    protected void UpdateItemsPanelProperties(Panel? itemsPanel)
    {
        switch (itemsPanel)
        {
            case StackPanel stackPanel:
                stackPanel.Orientation = this.ExpandDirection is ExpandDirection.Down or ExpandDirection.Up
                    ? Orientation.Vertical
                    : Orientation.Horizontal;
                break;
        }
    }


    protected override bool IsItemItsOwnContainerOverride(object item) => item is AccordionItem;
    protected override DependencyObject GetContainerForItemOverride()
        => new AccordionItem(this);

    #region ExpandDirection

    public static readonly DependencyProperty ExpandDirectionProperty = DependencyProperty.RegisterAttached(
        nameof(ExpandDirection),
        typeof(ExpandDirection),
        typeof(Accordion),
        new FrameworkPropertyMetadata(
            defaultValue: ExpandDirection.Down,
            flags: FrameworkPropertyMetadataOptions.Inherits,
            OnExpandDirectionValueChanged
        )
    );

    private static void OnExpandDirectionValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        (d as Accordion)?.UpdateItemsPanelProperties();
    }

    public static ExpandDirection GetExpandDirection(UIElement target)
        => target.GetValue<ExpandDirection>(ExpandDirectionProperty);

    public static void SetExpandDirection(UIElement target, ExpandDirection value)
        => target.SetValue<ExpandDirection>(ExpandDirectionProperty, value);

    public ExpandDirection ExpandDirection
    {
        get => this.GetValue<ExpandDirection>(ExpandDirectionProperty);
        set => this.SetValue<ExpandDirection>(ExpandDirectionProperty, value);
    }
    
    #endregion ExpandDirection
}