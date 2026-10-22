using MALClient.Adapters.Credentials;
using MALClient.Models.AdapterModels;

namespace MALClient.Maui.Adapters
{
    /// <summary>
    ///     The "password" holds the MAL session cookies, so it goes to the keystore backed SecureStorage instead of
    ///     plain shared preferences like the Xamarin app did.
    /// </summary>
    public class PasswordVault : IPasswordVault
    {
        private const string UserNameKey = "Username";
        private const string PasswordKey = "Passwd";

        public void Add(VaultCredential credential)
        {
            Run(() => SecureStorage.Default.SetAsync(UserNameKey, credential.UserName ?? string.Empty));
            Run(() => SecureStorage.Default.SetAsync(PasswordKey, credential.Password ?? string.Empty));
        }

        public VaultCredential Get(string domain)
        {
            var userName = Run(() => SecureStorage.Default.GetAsync(UserNameKey));
            var password = Run(() => SecureStorage.Default.GetAsync(PasswordKey));
            return string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)
                ? null
                : new VaultCredential("MALClient", userName, password);
        }

        public void Reset()
        {
            SecureStorage.Default.Remove(UserNameKey);
            SecureStorage.Default.Remove(PasswordKey);
        }

        //the vault interface is synchronous, run off the ui thread so blocking can't deadlock
        private static void Run(Func<Task> action)
        {
            Task.Run(action).GetAwaiter().GetResult();
        }

        private static T Run<T>(Func<Task<T>> action)
        {
            try
            {
                return Task.Run(action).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                //keystore got reset (e.g. after restoring a backup), treat as signed out
                return default;
            }
        }
    }
}
