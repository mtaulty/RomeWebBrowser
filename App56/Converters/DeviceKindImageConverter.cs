namespace App56
{
  using System;
  using Windows.UI.Xaml.Data;
  using Windows.UI.Xaml.Media.Imaging;

  class DeviceKindImageConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      var type = ((string)value).ToLower();

      BitmapImage image = new BitmapImage(new Uri($"ms-appx:///Assets/{type}.png"));

      return (image);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
