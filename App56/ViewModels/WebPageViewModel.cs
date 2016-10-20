namespace App56.ViewModels
{
  using System;
  using Windows.ApplicationModel.AppService;
  using Windows.System.RemoteSystems;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Navigation;

  internal class WebPageViewModel : ViewModelBase
  {
    public WebPageViewModel(ICloseWebPages webPageClosingService)
    {
      this.Label = loadingLabel;
      this.IsEditable = false;
      this.webPageCloser = webPageClosingService;
    }
    internal bool IsClosable
    {
      get
      {
        return (this.webPageCloser != null);
      }
    }
    internal bool IsEditable
    {
      get
      {
        return (this.isEditable);
      }
      set
      {
        base.SetProperty(ref this.isEditable, value);
      }
    }
    public string Label
    {
      get
      {
        return (this.label);
      }
      set
      {
        if (base.SetProperty(ref this.label, value) &&
          (value != loadingLabel) &&
          (value != newTabLabel))
        {
          this.TextUrl = this.BrowserUrl.ToString();
          this.IsEditable = true;
        }
      }
    }
    internal Uri BrowserUrl
    {
      get
      {
        return (this.browserUrl);
      }
      set
      {
        base.SetProperty(ref this.browserUrl, value);
      }
    }
    internal string TextUrl
    {
      get
      {
        return (this.textUrl);
      }
      set
      {
        base.SetProperty(ref this.textUrl, value);
      }
    }
    internal void OnGo()
    {
      if (this.BrowserUrl.ToString() != this.TextUrl)
      {
        this.IsEditable = false;
        this.BrowserUrl = new Uri(this.TextUrl);
      }
    }
    internal void OnClose()
    {
      this.webPageCloser?.Close(this);
    }
    internal static WebPageViewModel MakeDefaultItem(ICloseWebPages webPageCloser = null)
    {
      return (new WebPageViewModel(webPageCloser)
      {
        BrowserUrl = new Uri(microsoftDotCom)
      });
    }
    internal static WebPageViewModel AddPageItem
    {
      get
      {
        if (addPageItem == null)
        {
          addPageItem = new WebPageViewModel(null)
          {
            Label = newTabLabel
          };
        }
        return (addPageItem);
      }
    }
    bool isEditable;
    string label;
    Uri browserUrl;
    string textUrl;
    ICloseWebPages webPageCloser;

    static WebPageViewModel addPageItem;
    const string loadingLabel = "loading...";
    const string newTabLabel = "new tab +";
    const string microsoftDotCom = "http://www.microsoft.com";
  }
}
