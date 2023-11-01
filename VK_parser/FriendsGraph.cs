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
        public Friend()
        {
            Friends = new List<Friend>();
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        public List<Friend> Friends { get; set; }
        public Friend       parent  { get; set; }
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

    using FRespDeserializer = JsonConvert.DeserializeObject<FriendsResponse>;
    //                        Vector<{id, matrix_val}>
    using ListII            = List<KeyValuePair<int, int>>;
    using ListListII        = List<KeyValuePair<int, ListII>>;
    using Matrix            = Dictionary<int, Dictionary<int, int>>;

    public class FriendTreeParser
    {
        public FriendTreeParser()
        {

        }

        public FriendTree ParseFriendTree()
        {
            var friendTree = new FriendTree();

            var myFriendsJson = File.ReadAllText("my_friends.json");
            var myFriendsResponse = FRespDeserializer(myFriendsJson);
            var myFriends = myFriendsResponse.friends.items;

            var root = friendTree.Root;
            root.Friends.AddRange(myFriends);

            // Загрузка друзей друзей из соответствующих файлов
            foreach (var friend in myFriends)
            {
                var friends = friend.Friends;
                root.Friends[root.Friends.IndexOf(friend)].parent = root;
                var fileName = 
                    $"Friends\\{friend.Id}_{friend.LastName}_\
                    {friend.FirstName}_friends.json";

                if (File.Exists(fileName))
                {
                    var friendsResponse = 
                        FRespDeserializer(File.ReadAllText(fileName));
                    if (friendsResponse.friends != null)
                    {       
                        friends.AddRange(friendsResponse.friends.items);
                        foreach(var frienMyFriend in friends)
                            friends[friends.IndexOf(frienMyFriend)].parent = friend;
                    }
                }
            }
            return friendTree;
        }

        public Matrix adjMatrix(FriendTree tree)
        {
            vat root = tree.Root;
            ListListII friendsMyFriends = new ListListII();
            ListII myFriends = fillInRelations(root.Friends);
            friendsMyFriends.Add({root.Id, myFriends})
            foreach (var friend in root.Friends)
            {
                var fRelations = {frirend.Id, fillInRelations(friend.Friends)};
                friendsMyFriends.Add(fRelations);
            }
            var setFriends = getSetFriends(friendsMyFriends);
            int dim = setFriends.Count;

            Matrix res = createMatrix(setFriends, fRels);
            return res;
        }

        private Matrix createMatrix(HashSet<int> friends, ListListII fRels)
        {
            Matrix res = new Matrix();
            foreach (var fIdV in friends)
            {
                var row = new Dictionary<int, int>();
                foreach (var fIdH in friends)
                    row.Add({fIdH, 0});
                res.Add({fIdV, row});
            }
            res = fillInMatrix(res, fRels);
            return res;
        }

        private Matrix fillInMatrix(Matrix init, ListListII data)
        {
            var res = init
            foreach (var row in data)
                foreach (var fRel in row)
                {
                    res[row.Key][fRel.Key] = fRel.Value;
                    res[fRel.Key][row.Key] = fRel.Value;
                }
            return res;
        }

        private ListII fillInRelations(List<Friend> friends)
        {
            var res = new ListII();
            foreach (var friend in friends)
                res.Add({friend.Id, 1});
            return res;
        }

        private HashSet<int> getSetFriends(Matrix tmpMatrix)
        {
            var res = new HashSet<int>();
            foreach (var it in tmpMatrix)
                foreach (var fRel in it)
                    res.Add(fRel.Key);
            return res;
        }

        private int nodeCount { get; set; }
        private Dictionary<int, Vector<KeyValuePair<int, int>>> 
        adjMatrix { get; set; }
    }
}
