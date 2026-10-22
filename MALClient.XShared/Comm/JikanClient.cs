using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JikanDotNet;
using JikanDotNet.Config;

namespace MALClient.XShared.Comm;
public static class JikanClient
{
    //public jikan api (api.jikan.moe) was discontinued on 1st Oct 2026, tenrai is a drop-in replacement for jikan v4
    //kept separate from Query's client so we don't send mal credentials to third parties
    public static readonly HttpClient HttpClient = new HttpClient(new TenraiCompatHandler(new HttpClientHandler()))
    {
        BaseAddress = new Uri("https://api.tenrai.org/v1/")
    };

    //tenrai doesn't serve /users endpoints, jikan-edge does but with its own (camelCase) schema
    public static readonly HttpClient UsersHttpClient = new HttpClient
    {
        BaseAddress = new Uri("https://jikan.lucashdo.com/v1/")
    };

    public static readonly Jikan Jikan = new Jikan(new JikanClientConfiguration
    {
        SuppressException = true,
        LimiterConfigurations = new List<TaskLimiterConfiguration>
        {
            new TaskLimiterConfiguration(3, TimeSpan.FromSeconds(1)),
            new TaskLimiterConfiguration(60, TimeSpan.FromSeconds(60))
        }
    }, HttpClient);

    /// <summary>
    ///     Tenrai returns partial dates for upcoming shows ("2027" or "2027-10") where jikan returned full
    ///     timestamps, which JikanDotNet fails to deserialize, taking whole search pages down with them.
    /// </summary>
    private class TenraiCompatHandler : DelegatingHandler
    {
        private static readonly Regex PartialDateRegex =
            new Regex("\"(from|to|aired|date)\"\\s*:\\s*\"(\\d{4})(?:-(\\d{2}))?\"", RegexOptions.Compiled);

        public TenraiCompatHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.Content == null)
                return response;

            var json = await response.Content.ReadAsStringAsync();
            var fixedJson = PartialDateRegex.Replace(json, match =>
                $"\"{match.Groups[1].Value}\":\"{match.Groups[2].Value}-" +
                $"{(match.Groups[3].Success ? match.Groups[3].Value : "01")}-01T00:00:00+00:00\"");
            var mediaType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
            response.Content = new StringContent(fixedJson, Encoding.UTF8, mediaType);
            return response;
        }
    }
}
