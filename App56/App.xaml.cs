namespace App56
{
  using Services;
  using System.Threading.Tasks;
  using ViewModels;
  using Windows.ApplicationModel;
  using Windows.ApplicationModel.Activation;
  using Windows.ApplicationModel.AppService;
  using Windows.ApplicationModel.Background;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  sealed partial class App : Application
  {
    public App()
    {
      this.InitializeComponent();
      this.LeavingBackground += OnLeavingBackground;
      this.EnteredBackground += OnEnteringBackground;
      this.background = true;
    }
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
      var storedStateWebPages = await Storage.LoadAsync();

      // Do we have any state to load?
      if (!string.IsNullOrEmpty(storedStateWebPages))
      {
        bool loadState =
          Storage.CheckAndClearRemoteRequestForNextLaunchFlag() ||
          (args.PreviousExecutionState == ApplicationExecutionState.Terminated);

        if (loadState)
        {
          BrowserViewModel.ForegroundInstance.Deserialize(storedStateWebPages);
        }
      }
    }
    async void OnEnteringBackground(object sender, EnteredBackgroundEventArgs e)
    {
      // NB: getting rid of the UI here means that if the user opens up
      // N tabs then when they come back to the app those tabs will still
      // be there but will all start loading fresh again. That might not
      // be the best user experience as it loses their position on the
      // page.
      if (BrowserViewModel.ForegroundInstance != null)
      {
        var deferral = e.GetDeferral();

        try
        {
          var serialized = BrowserViewModel.ForegroundInstance.Serialize();
          await Storage.StoreAsync(serialized);
        }
        finally
        {
          deferral.Complete();
        }
      }

      Window.Current.Content = null;
      this.background = true;
    }
    void OnLeavingBackground(object sender, LeavingBackgroundEventArgs e)
    {
      this.EnsureUI();
      this.background = false;
    }
    void EnsureUI()
    {
      Frame rootFrame = Window.Current.Content as Frame;

      // Do not repeat app initialization when the Window already has content,
      // just ensure that the window is active
      if (rootFrame == null)
      {
        // Create a Frame to act as the navigation context and navigate to the first page
        rootFrame = new Frame();

        // Place the frame in the current Window
        Window.Current.Content = rootFrame;
      }

      if (rootFrame.Content == null)
      {
        rootFrame.Navigate(typeof(MainPage), null);
      }
      // Ensure the current window is active
      Window.Current.Activate();
    }
    protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
    {
      var appServiceDetails = args.TaskInstance.TriggerDetails as
        AppServiceTriggerDetails;

      if ((appServiceDetails != null) &&
        (appServiceDetails.Name == AppServiceName) &&
        (appServiceDetails.IsRemoteSystemConnection))
      {
        var deferral = args.TaskInstance.GetDeferral();

        args.TaskInstance.Canceled += (s, e) =>
        {
          deferral.Complete();
        };

        appServiceDetails.AppServiceConnection.RequestReceived += async (s, e) =>
        {
          var requestDeferral = e.GetDeferral();

          await this.ProcessAppServiceRequestAsync(e);

          appServiceDetails.AppServiceConnection.Dispose();
          requestDeferral.Complete();
          deferral.Complete();
        };
      }
    }
    async Task ProcessAppServiceRequestAsync(AppServiceRequestReceivedEventArgs args)
    {
      var webPages = args.Request.Message[AppServiceParameterKey] as string;

      if (!string.IsNullOrEmpty(webPages))
      {
        // What we do now depends on whether we are running in the foreground
        // or not.
        if (!this.background)
        {
          BrowserViewModel.ForegroundInstance.Dispatch(() =>
          {
            BrowserViewModel.ForegroundInstance.Deserialize(webPages);
          });
        }
        else
        {
          // Store the pending web pages into our state file.
          await Storage.StoreAsync(webPages);

          // Flag that the app should read that state next time it launches
          // regardless of whether it was terminated or not.
          Storage.FlagRemoteRequestForNextLaunch();
        }
      }
    }
    public static string PackageFamilyName
    {
      get
      {
        return (Package.Current.Id.FamilyName);
      }
    }
    bool background;
    public static readonly string AppServiceName = "OpenTabsService";
    public static readonly string AppServiceParameterKey = "WebPages";
  }
}
