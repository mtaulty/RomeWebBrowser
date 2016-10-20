namespace App56.ViewModels
{
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Threading;
  using Windows.System.RemoteSystems;
  using Windows.UI.Xaml.Controls;

  internal partial class BrowserViewModel : ViewModelBase, ICloseWebPages
  {
    internal BrowserViewModel(bool watchRemoteSystems = false)
    {
      this.webPages = new ObservableCollection<WebPageViewModel>();
      this.webPages.Add(WebPageViewModel.MakeDefaultItem());
      this.webPages.Add(WebPageViewModel.AddPageItem);
      this.remoteSystems = new ObservableCollection<RemoteSystem>();

      if (watchRemoteSystems)
      {
        this.syncContext = SynchronizationContext.Current;
        this.StartRemoteSystemDetectionAsync();
      }
    }
    internal static BrowserViewModel ForegroundInstance
    {
      get
      {
        if (instance == null)
        {
          instance = new BrowserViewModel(true);
        }
        return (instance);
      }
    }
    internal ObservableCollection<WebPageViewModel> WebPages
    {
      get
      {
        return (this.webPages);
      }
      set
      {
        base.SetProperty(ref this.webPages, value);
      }
    }
    public ObservableCollection<RemoteSystem> RemoteSystems
    {
      get
      {
        return (this.remoteSystems);
      }
      set
      {
        base.SetProperty(ref this.remoteSystems, value);
      }
    }
    public void Close(WebPageViewModel webPage)
    {
      this.internalProcessing = true;
      this.WebPages.Remove(webPage);
      this.internalProcessing = false;
      this.SelectedWebPage = this.WebPages[this.WebPages.Count - 2];
    }
    internal WebPageViewModel SelectedWebPage
    {
      get
      {
        return (this.selectedWebPage);
      }
      set
      {
        base.SetProperty(ref this.selectedWebPage, value);
      }
    }
    internal RemoteSystem SelectedRemoteSystem
    {
      get
      {
        return (this.selectedRemoteSystem);
      }
      set
      {
        base.SetProperty(ref this.selectedRemoteSystem, value);
      }
    }

    internal bool CanTransferToRemoteSystem
    {
      get
      {
        return (this.RemoteSystems.Count > 0);
      }
    }
    public bool IsTransferring
    {
      get
      {
        return (this.isTransferring);
      }
      set
      {
        base.SetProperty(ref this.isTransferring, value);
      }
    }
    public string TransferStatus
    {
      get
      {
        return (this.transferStatus);
      }
      set
      {
        base.SetProperty(ref this.transferStatus, value);
      }
    }
    internal void OnSelectionChanged(
      object sender,
      SelectionChangedEventArgs e)
    {
      // This is a bit 'naughty' in that I'm relying on this function being
      // called *AFTER* the new selection comes into the SelectedItem property.

      // If you just selected our 'new tab' pivot item 
      if (!this.internalProcessing && (this.SelectedWebPage == WebPageViewModel.AddPageItem))
      {
        // make a new blank pivot item
        var newItem = this.InsertNewItem();

        // and redirect your selection to that item.
        this.SelectedWebPage = newItem;
      }
    }
    WebPageViewModel InsertNewItem()
    {
      var newItem = WebPageViewModel.MakeDefaultItem(this);

      this.WebPages.Insert(this.WebPages.Count - 1, newItem);

      return (newItem);
    }
    internal void Dispatch(Action action)
    {
      if (this.syncContext != null)
      {
        this.syncContext.Post(
          _ =>
          {
            action();
          },
          null);
      }
      else
      {
        action();
      }
    }
    void AddRemoteSystem(RemoteSystem remoteSystem)
    {
      if (remoteSystem.Status != RemoteSystemStatus.Unavailable)
      {
        this.RemoteSystems.Add(remoteSystem);
        if (this.SelectedRemoteSystem == null)
        {
          this.SelectedRemoteSystem = remoteSystem;
        }
        base.OnPropertyChanged("CanTransferToRemoteSystem");
      }
    }
    internal string Serialize()
    {
      var list = 
        this.webPages
          .Where(p => p != WebPageViewModel.AddPageItem)
          .Select(p => p.BrowserUrl.ToString())
          .ToList();

      var serialized = JsonConvert.SerializeObject(list);

      return (serialized);
    }
    internal void Deserialize(string serialized)
    {
      var newUrls = JsonConvert.DeserializeObject<List<string>>(serialized);

      // for the moment, we decide to get rid of what we currently have.
      this.internalProcessing = true;
      foreach (var page in this.webPages.ToList())
      {
        if (page.IsClosable)
        {
          this.webPages.Remove(page);
        }
      }
      for (int i = 0; i < newUrls.Count; i++)
      {
        if (i == 0)
        {
          this.webPages[i].BrowserUrl = new Uri(newUrls[i]);
        }
        else
        {
          var webPage = this.InsertNewItem();
          webPage.BrowserUrl = new Uri(newUrls[i]);
        }
      }
      this.SelectedWebPage = this.webPages[0];

      this.internalProcessing = false;
    }
    string transferStatus;
    SynchronizationContext syncContext;
    ObservableCollection<RemoteSystem> remoteSystems;
    ObservableCollection<WebPageViewModel> webPages;   
    RemoteSystem selectedRemoteSystem;
    WebPageViewModel selectedWebPage;
    RemoteSystemWatcher remoteWatcher;
    bool internalProcessing;
    bool isTransferring;

    static BrowserViewModel instance;
  }
}