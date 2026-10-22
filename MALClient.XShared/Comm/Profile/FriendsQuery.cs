using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.Runtime;
using HtmlAgilityPack;
using MALClient.Models.Models;
using MALClient.Models.Models.MalSpecific;
using MALClient.XShared.Utils;
using Newtonsoft.Json;

namespace MALClient.XShared.Comm.Profile
{
    public class FriendsQuery : Query
    {
        private readonly string _userName;

        public FriendsQuery(string userName)
        {
            _userName = userName;
        }

        public async Task<List<MalFriend>> GetFriends()
        {
            var output = new List<MalFriend>();

            try
            {
                var json = await JikanClient.UsersHttpClient.GetStringAsync(
                    $"users/{Uri.EscapeDataString(_userName)}/friends");
                var result = JsonConvert.DeserializeObject<FriendsRoot>(json);

                foreach (var friend in result.Data)
                {
                    output.Add(new MalFriend
                    {
                        Id = $"https://myanimelist.net/profile/{friend.Username}",
                        User = new MalUser
                        {
                            ImgUrl = friend.ImageUrl,
                            Name = friend.Username,
                        },
                        FriendsSince = friend.FriendsSince ?? "N/A",
                        LastOnline = friend.LastOnline ?? "N/A",
                    });
                }
            }
            catch (Exception e)
            {
                //HTML as always
            }


            return output;
        }

        [Preserve(AllMembers = true)]
        public class Friend
        {
            public string Username { get; set; }
            public string ImageUrl { get; set; }
            public string LastOnline { get; set; }
            public string FriendsSince { get; set; }
        }

        [Preserve(AllMembers = true)]
        public class FriendsRoot
        {
            public List<Friend> Data { get; set; }
        }
    }
}
