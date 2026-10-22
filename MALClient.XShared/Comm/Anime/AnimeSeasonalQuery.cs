using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Runtime;
using JikanDotNet;
using MALClient.Models.Models.Anime;
using MALClient.Models.Models.AnimeScrapped;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;
using Newtonsoft.Json;

namespace MALClient.XShared.Comm.Anime
{
    public class AnimeSeasonalQuery : Query
    {
        //public jikan api was discontinued on 1st Oct 2026, official mal api only needs client id for this endpoint
        private const string MalClientId = "183063f74126e7551b00c3b4de66986c";

        private readonly AnimeSeason _season;

        public AnimeSeasonalQuery(AnimeSeason season)
        {
            _season = season;
        }

        //jikan's Season enum isn't in calendar order (Spring = 0 ... Winter = 3)
        private static readonly Season[] CalendarSeasons = {Season.Winter, Season.Spring, Season.Summer, Season.Fall};

        public static AnimeSeason GetCurrentSeason()
        {
            var now = DateTime.UtcNow;
            return CreateSeason(now.Year, CalendarSeasons[(now.Month - 1) / 3]);
        }

        /// <summary>
        ///     Two previous seasons, current one and next one.
        /// </summary>
        public static List<AnimeSeason> GetSeasonSelection()
        {
            var current = GetCurrentSeason();
            var currentIndex = current.Year * 4 + Array.IndexOf(CalendarSeasons, current.Season);
            return Enumerable.Range(currentIndex - 2, 4)
                .Select(index => CreateSeason(index / 4, CalendarSeasons[index % 4]))
                .ToList();
        }

        private static AnimeSeason CreateSeason(int year, Season season)
        {
            return new AnimeSeason {Name = $"{season} {year}", Year = year, Season = season};
        }

        public async Task<List<SeasonalAnimeData>> GetSeasonalAnime(bool force = false)
        {
            var output = force /*|| DataCache.SeasonalUrls?.Count == 0*/ //either force or urls are empty after update
                ? new List<SeasonalAnimeData>()
                : await DataCache.RetrieveSeasonalData(_season.Name) ?? new List<SeasonalAnimeData>();
            //current season without suffix
            if (output.Count != 0) return output;

            var requestedSeason = _season.Year != 0 ? _season : GetCurrentSeason();

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-MAL-CLIENT-ID", MalClientId);
            try
            {
                var url = $"https://api.myanimelist.net/v2/anime/season/{requestedSeason.Year}/" +
                          $"{requestedSeason.Season.ToString().ToLowerInvariant()}" +
                          "?fields=id,title,main_picture,num_episodes,mean,genres,start_date,broadcast,status" +
                          "&sort=anime_num_list_users&limit=500";

                while (url != null)
                {
                    var season = JsonConvert.DeserializeObject<Root>(await client.GetStringAsync(url));

                    foreach (var entry in season.data.Select(datum => datum.node))
                    {
                        output.Add(new SeasonalAnimeData
                        {
                            Title = entry.title,
                            Id = entry.id,
                            ImgUrl = entry.main_picture?.large ?? entry.main_picture?.medium,
                            Episodes = entry.num_episodes.ToString(),
                            Score = entry.mean ?? 0,
                            Genres = entry.genres?.Select(item => item.name).ToList() ?? new List<string>(),
                            Index = output.Count + 1,
                            AirDay = entry.status != "finished_airing" &&
                                     Enum.TryParse(entry.broadcast?.day_of_the_week, true, out DayOfWeek day)
                                ? (int) day + 1
                                : -1,
                            AirStartDate = entry.start_date?.Length == 10
                                ? entry.start_date
                                : AnimeItemViewModel.InvalidStartEndDate
                        });
                    }

                    url = season.paging?.next;
                }

                DataCache.SaveSeasonalData(output, _season.Name);

                //We are done.
                return output;
            }
            catch (Exception)
            {
                return output;
            }
        }

        [Preserve(AllMembers = true)]
        public class MainPicture
        {
            public string medium { get; set; }
            public string large { get; set; }
        }

        [Preserve(AllMembers = true)]
        public class Genre
        {
            public string name { get; set; }
        }

        [Preserve(AllMembers = true)]
        public class Broadcast
        {
            public string day_of_the_week { get; set; }
        }

        [Preserve(AllMembers = true)]
        public class Node
        {
            public int id { get; set; }
            public string title { get; set; }
            public MainPicture main_picture { get; set; }
            public int num_episodes { get; set; }
            public float? mean { get; set; }
            public List<Genre> genres { get; set; }
            public string start_date { get; set; }
            public Broadcast broadcast { get; set; }
            public string status { get; set; }
        }

        [Preserve(AllMembers = true)]
        public class Datum
        {
            public Node node { get; set; }
        }

        [Preserve(AllMembers = true)]
        public class Paging
        {
            public string next { get; set; }
        }

        [Preserve(AllMembers = true)]
        public class Root
        {
            public List<Datum> data { get; set; }
            public Paging paging { get; set; }
        }
    }
}
