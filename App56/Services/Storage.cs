namespace App56.Services
{
  using System;
  using System.IO;
  using System.Threading.Tasks;
  using Windows.Storage;

  internal static class Storage
  {
    internal static void FlagRemoteRequestForNextLaunch()
    {
      ApplicationData.Current.LocalSettings.Values[requestPendingSettingName] = (bool)true;
    }
    internal static bool CheckAndClearRemoteRequestForNextLaunchFlag()
    {
      bool pending =
        ApplicationData.Current.LocalSettings.Values.ContainsKey(requestPendingSettingName);

      if (pending)
      {
        ApplicationData.Current.LocalSettings.Values.Remove(requestPendingSettingName);
      }
      return (pending);
    }
    internal static async Task<string> LoadAsync()
    {
      var contents = string.Empty;

      try
      {
        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(
          fileName);

        contents = await FileIO.ReadTextAsync(file);
      }
      catch (FileNotFoundException)
      {
      }
      return (contents);
    }
    internal static async Task StoreAsync(string contents)
    {
      var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
        fileName, CreationCollisionOption.ReplaceExisting);

      await FileIO.WriteTextAsync(file, contents);
    }
    const string fileName = "storage.json";
    const string requestPendingSettingName = "remoteRequestPending";
  }
}
