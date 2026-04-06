using System.Collections.ObjectModel;

namespace Bookmarkly.Uno;

public sealed partial class MainPage : Page
{
    private readonly ObservableCollection<int> datas = new();

    public MainPage()
    {
        datas.Add(1);
        datas.Add(1);
        datas.Add(1);
        datas.Add(1);
        datas.Add(1);
        datas.Add(1);
        datas.Add(1);
        datas.Add(1);
        datas.Add(1);
        this.InitializeComponent();
    }
}
