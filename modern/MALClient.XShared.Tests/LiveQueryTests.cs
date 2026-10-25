using System.Linq;
using System.Threading.Tasks;
using JikanDotNet;
using MALClient.Models.Enums;
using MALClient.Models.Models.Anime;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.Comm.Details;
using MALClient.XShared.Comm.Manga;
using MALClient.XShared.Comm.Profile;
using Xunit;

namespace MALClient.XShared.Tests
{
    /// <summary>
    ///     Smoke tests against the live services (MAL, Tenrai, Jikan-Edge). Run with --filter Category=Integration.
    /// </summary>
    [Trait("Category", "Integration")]
    [Collection("Live")]
    public class LiveQueryTests
    {
        public LiveQueryTests()
        {
            TestServices.EnsureRegistered();
        }

        [Fact]
        public async Task SeasonalAnime()
        {
            var data = await new AnimeSeasonalQuery(AnimeSeasonalQuery.GetCurrentSeason()).GetSeasonalAnime(true);

            Assert.NotEmpty(data);
            Assert.Equal(data.Count, data.Select(anime => anime.Id).Distinct().Count());
            Assert.Contains(data, anime => anime.AirDay > 0);
        }

        [Fact]
        public async Task SeasonalAnimeForPastSeasonPages()
        {
            //big enough to span more than one page of the mal api
            var season = new AnimeSeason {Name = "Winter 2025", Year = 2025, Season = Season.Winter};
            var data = await new AnimeSeasonalQuery(season).GetSeasonalAnime(true);

            Assert.True(data.Count > 200, $"Expected more than 200 entries, got {data.Count}");
        }

        [Fact]
        public async Task AnimeDetails()
        {
            var details = await new AnimeGeneralDetailsQuery().GetAnimeDetails(true, "1", "", true);

            Assert.NotNull(details);
            Assert.Equal("Cowboy Bebop", details.Title);
            Assert.Equal(26, details.AllEpisodes);
            Assert.Equal("1998-04-02", details.StartDate);
        }

        [Fact]
        public async Task MangaDetails()
        {
            var details = await new AnimeGeneralDetailsQuery().GetAnimeDetails(true, "2", "", false);

            Assert.NotNull(details);
            Assert.Equal("Berserk", details.Title);
        }

        [Theory]
        [InlineData("naruto")]
        //contains upcoming entries with partial dates ("2027-10") that tenrai returns
        [InlineData("frieren")]
        [InlineData("one piece")]
        public async Task AnimeSearch(string query)
        {
            var results = await new AnimeSearchQuery(query).GetSearchResults();

            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task MangaSearch()
        {
            var results = await new MangaSearchQuery("berserk").GetSearchResults();

            Assert.Contains(results, manga => manga.Title == "Berserk");
        }

        [Fact]
        public async Task Episodes()
        {
            var episodes = await new AnimeEpisodesQuery().GetEpisodes(1, true);

            Assert.Equal(26, episodes.Count);
        }

        [Fact]
        public async Task Reviews()
        {
            var reviews = await new AnimeReviewsQuery(1).GetAnimeReviews(true);

            Assert.NotEmpty(reviews);
            Assert.All(reviews, review => Assert.False(string.IsNullOrEmpty(review.Author)));
        }

        [Fact]
        public async Task AnimeByGenre()
        {
            var results = await new AnimeGenreStudioQuery(AnimeGenreSearch.Action, 2).GetAnime();

            Assert.NotEmpty(results);
            Assert.Equal(26, results.Min(anime => anime.Index));
        }

        [Fact]
        public async Task AnimeByStudio()
        {
            var results = await new AnimeGenreStudioQuery((AnimeStudios) 14).GetAnime();

            Assert.Contains(results, anime => anime.Title == "Cowboy Bebop");
        }

        [Fact]
        public async Task Friends()
        {
            var friends = await new FriendsQuery("Xinil").GetFriends();

            Assert.NotEmpty(friends);
            Assert.All(friends, friend => Assert.StartsWith("https://myanimelist.net/profile/", friend.Id));
        }

        [Fact]
        public async Task ProfileFavourites()
        {
            var profile = await new ProfileQuery(false, "Xinil").GetProfileData(true, true);

            Assert.NotNull(profile);
            Assert.Contains(1, profile.FavouriteAnime);
            Assert.NotEmpty(profile.FavouriteCharacters);
        }

        [Fact]
        public async Task CharacterDetails()
        {
            var character = await new CharacterDetailsQuery(40).GetCharacterDetails(true);

            //nicknames go between the names, "Luffy "Mugiwara, Straw Hat" Monkey D."
            Assert.StartsWith("Luffy", character.Name);
            Assert.EndsWith("Monkey D.", character.Name);
            Assert.False(string.IsNullOrEmpty(character.ImgUrl));
            Assert.Contains(character.Animeography, anime => anime.Id == 21);
            Assert.NotEmpty(character.VoiceActors);
        }

        [Fact]
        public async Task StaffDetails()
        {
            //whoever voices Luffy lists him among their roles
            var character = await new CharacterDetailsQuery(40).GetCharacterDetails(true);
            var actor = character.VoiceActors.First();
            var person = await new StaffDetailsQuery(int.Parse(actor.Id)).GetStaffDetails(true);

            Assert.Equal(actor.Name, person.Name);
            Assert.Contains(person.ShowCharacterPairs, pair => pair.AnimeCharacter.Id == "40");
        }
    }
}
