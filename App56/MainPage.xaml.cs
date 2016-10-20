
namespace App56
{
  using ViewModels;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.ViewModel = BrowserViewModel.ForegroundInstance;
    }
    internal BrowserViewModel ViewModel { get; set; }
  }
}
