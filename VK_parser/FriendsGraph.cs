using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using VK_Friends_Tree;
using static VK_Friends_Tree.Controllers.GraphController;

namespace VK_parser
{
    public class FriendsGraph
    {
        public int friendsCount { get; set; }
        public string[,] adjMatrix { get; set; }
        /*public void GetFriends()
        {
            FriendsResponse friends = JsonConvert.DeserializeObject<FriendsResponse>(System.IO.File.ReadAllText("my_friends.json"));
            List<FriendsResponse> friendsMyFriends = new List<FriendsResponse>();

        }*///VK_Friends_Tree.Controllers.GraphController.FriendsResponse
        
    }
    public class FriendsEnum
    {
        [JsonProperty("count")]
        public int count { get; set; }
        [JsonProperty("items")]
        public List<Friend> items { get; set; }
    }
    public class FriendsResponse
    {
        [JsonProperty("response")]
        public FriendTree friends { get; set; }
    }
    public class Friend
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        public List<Friend> Friends { get; set; }

        public Friend()
        {
            Friends = new List<Friend>();
        }
    }

    public class FriendTree
    {
        public Friend Root { get; set; }

        public FriendTree()
        {
            Root = new Friend
            {
                Id = 00000, // Ваш ID
                FirstName = "Даниил",
                LastName = "Пуртов"
            };
        }
    }
    public class FriendTreeParser
    {
        public FriendTree ParseFriendTree()
        {
            var friendTree = new FriendTree();

            // Загрузка ваших друзей из файла my_friends.json
            var myFriendsJson = File.ReadAllText("C:\\Users\\danpu\\source\\repos\\VK_Friends_Tree\\VK_Friends_Tree\\my_friends.json");
            var myFriends = JsonConvert.DeserializeObject<FriendsResponse>(myFriendsJson);

            friendTree.Root.Friends.AddRange(myFriends);

            // Загрузка друзей друзей из соответствующих файлов
            foreach (var friend in myFriends)
            {
                var fileName = $"C:\\Users\\danpu\\source\\repos\\VK_Friends_Tree\\VK_Friends_Tree\\Friends\\{friend.Id}_{friend.LastName}_{friend.FirstName}_friends.json";
                if (File.Exists(fileName))
                {
                    var friendJson = File.ReadAllText(fileName);
                    var friendFriends = JsonConvert.DeserializeObject<List<Friend>>(friendJson);

                    friend.Friends.AddRange(friendFriends);
                }
            }

            return friendTree;
        }
    }
}
