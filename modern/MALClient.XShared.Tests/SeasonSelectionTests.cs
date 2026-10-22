using System;
using System.Linq;
using JikanDotNet;
using MALClient.XShared.Comm.Anime;
using Xunit;

namespace MALClient.XShared.Tests
{
    public class SeasonSelectionTests
    {
        private static readonly Season[] CalendarOrder = {Season.Winter, Season.Spring, Season.Summer, Season.Fall};

        [Fact]
        public void CurrentSeasonMatchesCalendar()
        {
            var now = DateTime.UtcNow;
            var current = AnimeSeasonalQuery.GetCurrentSeason();

            Assert.Equal(now.Year, current.Year);
            Assert.Equal(CalendarOrder[(now.Month - 1) / 3], current.Season);
            Assert.Equal($"{current.Season} {current.Year}", current.Name);
        }

        [Fact]
        public void SelectionIsTwoPreviousCurrentAndNextInOrder()
        {
            var current = AnimeSeasonalQuery.GetCurrentSeason();
            var selection = AnimeSeasonalQuery.GetSeasonSelection();

            //the picker treats index 4 as the optional custom season
            Assert.Equal(4, selection.Count);
            Assert.Equal(current.Name, selection[2].Name);

            var indices = selection.Select(season => season.Year * 4 + Array.IndexOf(CalendarOrder, season.Season))
                .ToList();
            for (var i = 1; i < indices.Count; i++)
                Assert.Equal(indices[i - 1] + 1, indices[i]);

            Assert.All(selection, season => Assert.Equal($"{season.Season} {season.Year}", season.Name));
        }
    }
}
