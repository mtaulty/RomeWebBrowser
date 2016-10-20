namespace App56
{
  using System;
  using Windows.UI.Xaml.Data;
  using Windows.UI.Xaml.Media.Imaging;

  class ConnectionTypeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      return ((bool)value ? "(proximity)" : "(cloud)");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
