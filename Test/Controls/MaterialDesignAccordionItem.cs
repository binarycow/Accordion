using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace Test;

public class MaterialDesignAccordionItem : AccordionItem
{
    static MaterialDesignAccordionItem()
    {
        DefaultStyleKeyProperty?.OverrideMetadata(
            typeof(MaterialDesignAccordionItem),
            new FrameworkPropertyMetadata(typeof(MaterialDesignAccordionItem))
        );
    }
    public MaterialDesignAccordionItem(Accordion accordion) : base(accordion)
    {
    }

    #region HorizontalHeaderPadding

    public static readonly DependencyProperty HorizontalHeaderPaddingProperty =
        ExpanderAssist.HorizontalHeaderPaddingProperty.AddOwner(typeof(AccordionItem));

    public Thickness HorizontalHeaderPadding
    {
        get => this.GetValue<Thickness>(HorizontalHeaderPaddingProperty);
        set => this.SetValue<Thickness>(HorizontalHeaderPaddingProperty, value);
    }

    #endregion HorizontalHeaderPadding

    #region VerticalHeaderPadding

    public static readonly DependencyProperty VerticalHeaderPaddingProperty =
        ExpanderAssist.VerticalHeaderPaddingProperty.AddOwner(typeof(AccordionItem));

    public Thickness VerticalHeaderPadding
    {
        get => this.GetValue<Thickness>(VerticalHeaderPaddingProperty);
        set => this.SetValue<Thickness>(VerticalHeaderPaddingProperty, value);
    }

    #endregion VerticalHeaderPadding

    #region HeaderFontSize

    public static readonly DependencyProperty HeaderFontSizeProperty =
        ExpanderAssist.HeaderFontSizeProperty.AddOwner(typeof(AccordionItem));

    public double HeaderFontSize
    {
        get => this.GetValue<double>(HeaderFontSizeProperty);
        set => this.SetValue<double>(HeaderFontSizeProperty, value);
    }

    #endregion HeaderFontSize

    #region HeaderBackground

    public static readonly DependencyProperty HeaderBackgroundProperty =
        ExpanderAssist.HeaderBackgroundProperty.AddOwner(typeof(AccordionItem));

    public Brush? HeaderBackground
    {
        get => this.GetValue<Brush?>(HeaderBackgroundProperty);
        set => this.SetValue<Brush?>(HeaderBackgroundProperty, value);
    }

    #endregion HeaderBackground
}