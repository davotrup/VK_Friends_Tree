using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using VK_Friends_Tree;
using static VK_Friends_Tree.Controllers.GraphController;

namespace VK_parser
{

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
        public FriendsEnum friends { get; set; }
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
        public Friend parent { get; set; }
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
        private int nodeCount { get; set; }
        //Словарь - матрица крутой
        
        private Dictionary<int,Vector<KeyValuePair<int, int>>> adjMatrix { get; set; }
        public FriendTree ParseFriendTree()
        {
            var friendTree = new FriendTree();

            // Загрузка ваших друзей из файла my_friends.json
            var myFriendsJson = File.ReadAllText("my_friends.json");
            var myFriendsResponse = JsonConvert.DeserializeObject<FriendsResponse>(myFriendsJson);
            var myFriends = myFriendsResponse.friends.items;
            adjMatrix = new Dictionary<int, Vector<KeyValuePair<int, int>>>();
            friendTree.Root.Friends.AddRange(myFriends);
            adjMatrix.Add(friendTree.Root.Id, (new Vector<KeyValuePair<int, int>>()));
            adjMatrix[friendTree.Root.Id].Add(friendTree.Root.Id, 0);
            // Загрузка друзей друзей из соответствующих файлов
            foreach (var friend in myFriends)
            {

                adjMatrix.Add(friend.Id, (new Vector<KeyValuePair<int, int>>()));
                //adjMatrix[friend.Id].Add(friend.Id, 0);
                //adjMatrix[friend.Id].Add(friend.parent.Id, 1);
                friendTree.Root.Friends[friendTree.Root.Friends.IndexOf(friend)].parent = friendTree.Root;
                if (!adjMatrix.ContainsKey(friend.Id))
                {
                    adjMatrix.Add(friend.Id, (new Dictionary<int, int>()));
                    adjMatrix[friend.Id].Add(friend.Id, 0);
                }
                adjMatrix[friend.Id].Add(friend.parent.Id, 1);

                var fileName = $"Friends\\{friend.Id}_{friend.LastName}_{friend.FirstName}_friends.json";
                if (File.Exists(fileName))
                {
                    var friendJson = File.ReadAllText(fileName);
                    var friendFriendsResponse = JsonConvert.DeserializeObject<FriendsResponse>(friendJson);
                    if (friendFriendsResponse.friends != null)
                    {       
                        var friendFriends = friendFriendsResponse.friends.items;
                        friend.Friends.AddRange(friendFriends);
                        foreach(var frienMyFriend in friend.Friends)
                        {
                            friend.Friends[friend.Friends.IndexOf(frienMyFriend)].parent = friend;

                            if (!adjMatrix.ContainsKey(frienMyFriend.Id))
                            {
                                adjMatrix.Add(frienMyFriend.Id, (new Dictionary<int, int>()));
                                adjMatrix[frienMyFriend.Id].Add(frienMyFriend.Id, 0);
                            }
                            adjMatrix[frienMyFriend.Id].Add(frienMyFriend.parent.Id, 1);
                        }
                    }
                }
            }

            return friendTree;
        }
        public FriendTreeParser()
        {

        }
        public void addEdge(Friend node1, Friend node2 )
        {

        }
        //public List<int> getFriendsId()
        //{

        //}
    }
}
