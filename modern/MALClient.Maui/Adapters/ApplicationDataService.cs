using Android.Content;
using MALClient.Adapters;
using Application = Android.App.Application;

namespace MALClient.Maui.Adapters
{
    /// <summary>
    ///     Settings storage on Android shared preferences, ported from the Xamarin adapter. MAUI's Preferences
    ///     api needs to know the stored type up front, while settings are read back as plain objects.
    /// </summary>
    public class ApplicationDataService : IApplicationDataService
    {
        private static readonly ISharedPreferences Preferences =
            Application.Context.GetSharedPreferences($"{Application.Context.PackageName}_preferences",
                FileCreationMode.Private);

        public object this[string key]
        {
            get => Preferences.Contains(key) ? Preferences.All[key] : null;
            set
            {
                var editor = Preferences.Edit();
                if (value != null)
                    switch (Type.GetTypeCode(value.GetType()))
                    {
                        case TypeCode.Boolean:
                            editor.PutBoolean(key, (bool) value);
                            break;
                        case TypeCode.String:
                            editor.PutString(key, (string) value);
                            break;
                        case TypeCode.Int32:
                            editor.PutInt(key, (int) value);
                            break;
                        case TypeCode.Int64:
                            editor.PutLong(key, (long) value);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(value), value.GetType(), null);
                    }
                else
                    editor.Remove(key);
                editor.Apply();
            }
        }

        public object this[RoamingDataTypes key]
        {
            get => this[key.ToString()];
            set => this[key.ToString()] = value;
        }
    }
}
