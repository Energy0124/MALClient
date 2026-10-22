using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MALClient.Adapters;
using MALClient.Adapters.Credentials;
using MALClient.Models.AdapterModels;
using MALClient.XShared.Comm.MagicalRawQueries;
using MALClient.XShared.Interfaces;
using MALClient.XShared.Ioc;

namespace MALClient.XShared.Tests
{
    /// <summary>
    ///     Registers the minimal set of platform adapters the queries need, in-memory and with no persistence.
    /// </summary>
    public static class TestServices
    {
        private static readonly object SyncLock = new object();
        private static bool _registered;

        public static void EnsureRegistered()
        {
            lock (SyncLock)
            {
                if (_registered)
                    return;

                SimpleIoc.Default.Register<IApplicationDataService, InMemoryApplicationData>();
                SimpleIoc.Default.Register<IDataCache, NullDataCache>();
                SimpleIoc.Default.Register<IConnectionInfoProvider, AlwaysOnlineConnection>();
                SimpleIoc.Default.Register<IMalHttpContextProvider, PlainHttpContextProvider>();
                SimpleIoc.Default.Register<IPasswordVault, NullPasswordVault>();
                _registered = true;
            }
        }

        private class InMemoryApplicationData : IApplicationDataService
        {
            private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

            public object this[string key]
            {
                get => _data.TryGetValue(key, out var value) ? value : null;
                set => _data[key] = value;
            }

            public object this[RoamingDataTypes key]
            {
                get => this[key.ToString()];
                set => this[key.ToString()] = value;
            }
        }

        private class NullDataCache : IDataCache
        {
            public Task SaveData<T>(T data, string filename, string targetFolder) => Task.CompletedTask;
            public Task SaveDataRoaming<T>(T data, string filename) => Task.CompletedTask;
            public Task<T> RetrieveData<T>(string filename, string originFolder, int expiration) => Task.FromResult(default(T));
            public Task<T> RetrieveDataRoaming<T>(string filename, int expiration) => Task.FromResult(default(T));
            public Task ClearApiRelatedCache() => Task.CompletedTask;
            public Task ClearAnimeListData() => Task.CompletedTask;
        }

        private class AlwaysOnlineConnection : IConnectionInfoProvider
        {
            public void Init()
            {
            }

            public bool HasInternetConnection { get; set; } = true;
        }

        private class PlainHttpContextProvider : IMalHttpContextProvider
        {
            public void ErrorMessage(string what)
            {
            }

            public Task<CsrfHttpClient> GetHttpContextAsync(bool skipAuthCheck = false) => throw new NotSupportedException();

            public void Invalidate()
            {
            }

            public HttpClientHandler GetHandler() => new HttpClientHandler();

            public void SetCookies(string cookies)
            {
            }

            public Task<HttpClient> GetApiHttpContextAsync() => throw new NotSupportedException();
        }

        private class NullPasswordVault : IPasswordVault
        {
            public void Add(VaultCredential credential)
            {
            }

            public VaultCredential Get(string domain) => null;

            public void Reset()
            {
            }
        }
    }
}
