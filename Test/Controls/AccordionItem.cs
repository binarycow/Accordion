using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace Test;


public class AccordionItem : HeaderedContentControl
{

    static AccordionItem()
    {
        DefaultStyleKeyProperty?.OverrideMetadata(
            typeof(AccordionItem),
            new FrameworkPropertyMetadata(typeof(AccordionItem))
        );
    }
    
    private readonly Accordion accordion;
    public AccordionItem(Accordion accordion) => this.accordion = accordion;


    #region ExpandDirection

    public static readonly DependencyProperty ExpandDirectionProperty = Accordion.ExpandDirectionProperty.AddOwner(
        typeof(AccordionItem),
        new FrameworkPropertyMetadata(
            defaultValue: ExpandDirection.Down
        )
    );

    public ExpandDirection ExpandDirection
    {
        get => this.GetValue<ExpandDirection>(ExpandDirectionProperty);
        set => this.SetValue<ExpandDirection>(ExpandDirectionProperty, value);
    }
    
    #endregion ExpandDirection
    
    #region IsSelected
    
    public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(
        typeof(AccordionItem),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnIsSelectedChanged
        )
    );

    public bool IsSelected
    {
        get => this.GetValue<bool>(IsSelectedProperty);
        set => this.SetValue<bool>(IsSelectedProperty, value);
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AccordionItem item && e.NewValue is true)
        {
            item.SelectItem();
        }
    }

    private void SelectItem()
    {
        var dc = this.DataContext;
        if (ReferenceEquals(this.accordion.SelectedItem, dc) is false)
        {
            this.accordion.SelectedItem = dc;
        }
    }

    #endregion IsSelected
    

}