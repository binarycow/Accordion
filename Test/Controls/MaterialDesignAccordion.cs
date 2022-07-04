using System.Windows;

namespace Test;

public class MaterialDesignAccordion : Accordion
{
    static MaterialDesignAccordion()
    {
        DefaultStyleKeyProperty?.OverrideMetadata(
            typeof(MaterialDesignAccordion),
            new FrameworkPropertyMetadata(typeof(MaterialDesignAccordion))
        );
    }


    protected override DependencyObject GetContainerForItemOverride() => new MaterialDesignAccordionItem(this);
}