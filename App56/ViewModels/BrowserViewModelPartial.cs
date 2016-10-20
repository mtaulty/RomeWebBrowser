namespace App56.ViewModels
{
  using System;
  using System.Collections.ObjectModel;
  using System.Threading.Tasks;
  using Windows.ApplicationModel.AppService;
  using Windows.Foundation.Collections;
  using Windows.System.RemoteSystems;

  internal partial class BrowserViewModel : ViewModelBase, ICloseWebPages
  {
    async Task StartRemoteSystemDetectionAsync()
    {
      var result = await RemoteSystem.RequestAccessAsync();

      if (result == RemoteSystemAccessStatus.Allowed)
      {
        this.RemoteSystems = new ObservableCollection<RemoteSystem>();
        this.remoteWatcher = RemoteSystem.CreateWatcher();
        this.remoteWatcher.RemoteSystemAdded += (s, e) =>
        {
          this.Dispatch(() =>
          {
            this.AddRemoteSystem(e.RemoteSystem);
          });
        };
        this.remoteWatcher.Start();
      }
    }

    internal async void OnTransfer()
    {
      var errorString = string.Empty;
      this.IsTransferring = true;
      this.TransferStatus = "connecting...";

      using (var connection = new AppServiceConnection())
      {
        connection.PackageFamilyName = App.PackageFamilyName;
        connection.AppServiceName = App.AppServiceName;

        var remoteRequest = new RemoteSystemConnectionRequest(this.SelectedRemoteSystem);

        var result = await connection.OpenRemoteAsync(remoteRequest);

        if (result == AppServiceConnectionStatus.Success)
        {
          this.TransferStatus = "Connected, calling...";

          var message = new ValueSet();
          var content = this.Serialize();
          message[App.AppServiceParameterKey] = content;

          var response = await connection.SendMessageAsync(message);

          if (response.Status != AppServiceResponseStatus.Success)
          {
            this.TransferStatus = $"Failed to call - status was [{response.Status}]";
          }
          this.TransferStatus = "Succeeded";
        }
        else
        {
          this.TransferStatus = $"Failed to open connection with status {result}";
        }
        this.transferStatus = "Closing";
      }
      // time for the display of errors etc. to be seen.
      await Task.Delay(2000);

      this.IsTransferring = false;
    }
  }
}
