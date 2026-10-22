using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MALClient.Adapters.Common
{
    /// <summary>
    ///     File system backed <see cref="IDataCache" />. Uses the same on-disk format as the UWP and Android
    ///     adapters (a json serialized Tuple of save time and data), so existing caches stay readable.
    /// </summary>
    public class FileDataCache : IDataCache
    {
        private readonly string _localFolder;
        private readonly string _roamingFolder;
        private readonly IConnectionInfoProvider _connectionInfoProvider;

        public FileDataCache(string localFolder, string roamingFolder,
            IConnectionInfoProvider connectionInfoProvider)
        {
            _localFolder = localFolder ?? throw new ArgumentNullException(nameof(localFolder));
            _roamingFolder = roamingFolder ?? localFolder;
            _connectionInfoProvider = connectionInfoProvider;
        }

        public Task SaveData<T>(T data, string filename, string targetFolder)
        {
            return SaveDataToFolder(data, filename, GetFolder(_localFolder, targetFolder));
        }

        public Task<T> RetrieveData<T>(string filename, string originFolder, int expiration)
        {
            return RetrieveDataFromFolder<T>(filename, GetFolder(_localFolder, originFolder), expiration);
        }

        public Task SaveDataRoaming<T>(T data, string filename)
        {
            return SaveDataToFolder(data, filename, _roamingFolder);
        }

        public Task<T> RetrieveDataRoaming<T>(string filename, int expiration)
        {
            return RetrieveDataFromFolder<T>(filename, _roamingFolder, 0);
        }

        public Task ClearApiRelatedCache()
        {
            TryDelete(() => File.Delete(Path.Combine(_localFolder, "mal_to_hum.json")));
            TryDelete(() => File.Delete(Path.Combine(_localFolder, "volatile_data.json")));
            DeleteListData();
            TryDelete(() => Directory.Delete(Path.Combine(_localFolder, "AnimeDetails"), true));
            return Task.CompletedTask;
        }

        public Task ClearAnimeListData()
        {
            DeleteListData();
            return Task.CompletedTask;
        }

        private void DeleteListData()
        {
            if (!Directory.Exists(_localFolder))
                return;

            foreach (var file in Directory.EnumerateFiles(_localFolder)
                         .Where(file => Path.GetFileName(file).Contains("_data_")))
                TryDelete(() => File.Delete(file));
        }

        private static string GetFolder(string root, string subFolder)
        {
            return string.IsNullOrEmpty(subFolder) ? root : Path.Combine(root, subFolder);
        }

        private bool CheckForOldData(DateTime date, int days)
        {
            //offline, stale data beats no data
            if (!_connectionInfoProvider?.HasInternetConnection ?? false)
                return true;

            return DateTime.UtcNow.Subtract(date).TotalDays < days;
        }

        private static async Task SaveDataToFolder<T>(T data, string filename, string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                var json = JsonConvert.SerializeObject(new Tuple<DateTime, T>(DateTime.UtcNow, data));
                await File.WriteAllTextAsync(Path.Combine(folder, filename), json);
            }
            catch (Exception)
            {
                //caching is best effort
            }
        }

        private async Task<T> RetrieveDataFromFolder<T>(string filename, string folder, int expiration)
        {
            try
            {
                var path = Path.Combine(folder, filename);
                if (!File.Exists(path))
                    return default;

                var tuple = JsonConvert.DeserializeObject<Tuple<DateTime, T>>(await File.ReadAllTextAsync(path));
                if (tuple == null)
                    return default;

                return expiration >= 1
                    ? CheckForOldData(tuple.Item1, expiration) ? tuple.Item2 : default
                    : tuple.Item2;
            }
            catch (Exception)
            {
                //corrupted or incompatible file
                return default;
            }
        }

        private static void TryDelete(Action delete)
        {
            try
            {
                delete();
            }
            catch (Exception)
            {
                //already gone or in use
            }
        }
    }
}
