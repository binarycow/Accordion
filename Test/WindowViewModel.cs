using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Windows.Controls;
using System.Windows.Markup;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Test;

[ContentProperty(nameof(MediaTypeNames.Text))]
public partial class ItemViewModel : ObservableObject
{
    [ObservableProperty] private string? title;
    [ObservableProperty] private object? text;
}

public class ItemViewModelCollection : ObservableCollection<ItemViewModel>
{
}

[ContentProperty(nameof(Items))]
public partial class WindowViewModel : ObservableObject
{
    private ItemViewModelCollection? items;

    public ItemViewModelCollection Items
    {
        get => this.items ??= new();
        set => this.items = value;
    }


    [ObservableProperty] private ExpandDirection expandDirection;
    [ObservableProperty] private double fontSize = 12;
    
    
}