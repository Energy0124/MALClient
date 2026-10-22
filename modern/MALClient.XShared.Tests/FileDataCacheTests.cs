using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MALClient.Adapters;
using MALClient.Adapters.Common;
using Newtonsoft.Json;
using Xunit;

namespace MALClient.XShared.Tests
{
    public class FileDataCacheTests : IDisposable
    {
        private class Connection : IConnectionInfoProvider
        {
            public void Init()
            {
            }

            public bool HasInternetConnection { get; set; } = true;
        }

        private readonly string _root = Path.Combine(Path.GetTempPath(), "malclient-cache-" + Guid.NewGuid());
        private readonly Connection _connection = new Connection();
        private readonly FileDataCache _cache;

        public FileDataCacheTests()
        {
            _cache = new FileDataCache(Path.Combine(_root, "local"), Path.Combine(_root, "roaming"), _connection);
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);
        }

        private void WriteLegacyFile<T>(string relativePath, DateTime savedAt, T data)
        {
            var path = Path.Combine(_root, "local", relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonConvert.SerializeObject(new Tuple<DateTime, T>(savedAt, data)));
        }

        [Fact]
        public async Task RoundTripsDataInSubFolder()
        {
            await _cache.SaveData(new List<int> {1, 2, 3}, "list.json", "AnimeDetails");

            Assert.Equal(new[] {1, 2, 3}, await _cache.RetrieveData<List<int>>("list.json", "AnimeDetails", 7));
        }

        [Fact]
        public async Task RoundTripsRoamingData()
        {
            await _cache.SaveDataRoaming("value", "roaming.json");

            Assert.Equal("value", await _cache.RetrieveDataRoaming<string>("roaming.json", 0));
        }

        [Fact]
        public async Task ReadsFilesWrittenByOldAdapters()
        {
            WriteLegacyFile("seasonal_data.json", DateTime.UtcNow, new List<string> {"a"});

            Assert.Equal(new[] {"a"}, await _cache.RetrieveData<List<string>>("seasonal_data.json", null, 7));
        }

        [Fact]
        public async Task ExpiredDataIsIgnoredOnlyWhenOnline()
        {
            WriteLegacyFile("old.json", DateTime.UtcNow.AddDays(-10), "stale");

            Assert.Null(await _cache.RetrieveData<string>("old.json", null, 7));
            Assert.Equal("stale", await _cache.RetrieveData<string>("old.json", null, 0));

            _connection.HasInternetConnection = false;
            Assert.Equal("stale", await _cache.RetrieveData<string>("old.json", null, 7));
        }

        [Fact]
        public async Task MissingOrCorruptedFilesReturnDefault()
        {
            Assert.Null(await _cache.RetrieveData<string>("missing.json", "nowhere", 7));

            Directory.CreateDirectory(Path.Combine(_root, "local"));
            File.WriteAllText(Path.Combine(_root, "local", "broken.json"), "{not json");
            Assert.Null(await _cache.RetrieveData<string>("broken.json", null, 7));
        }

        [Fact]
        public async Task ClearAnimeListDataOnlyRemovesListFiles()
        {
            await _cache.SaveData("list", "anime_data_user.json", null);
            await _cache.SaveData("other", "settings.json", null);

            await _cache.ClearAnimeListData();

            Assert.Null(await _cache.RetrieveData<string>("anime_data_user.json", null, 0));
            Assert.Equal("other", await _cache.RetrieveData<string>("settings.json", null, 0));
        }
    }
}
