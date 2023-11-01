using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Matrix = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, int>>;
using ListII = System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, int>>;
namespace VK_parser
{
     // Объявляем алиас ListII
    using ListListII = List<KeyValuePair<int, ListII>>;
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

    public class FriendTreeParser
    {
        public FriendTreeParser()
        {

        }

        public FriendTree ParseFriendTree()
        {
            var friendTree = new FriendTree();

            var myFriendsJson = File.ReadAllText("my_friends.json");
            var myFriendsResponse = JsonConvert.DeserializeObject<FriendsResponse>(myFriendsJson);
            var myFriends = myFriendsResponse.friends.items;

            var root = friendTree.Root;
            root.Friends.AddRange(myFriends);

            // Загрузка друзей друзей из соответствующих файлов
            foreach (var friend in myFriends)
            {
                var friends = friend.Friends;
                root.Friends[root.Friends.IndexOf(friend)].parent = root;
                var fileName = 
                    $"Friends\\{friend.Id}_{friend.LastName}_{friend.FirstName}_friends.json";

                if (File.Exists(fileName))
                {
                    var friendsResponse =
                        JsonConvert.DeserializeObject<VK_parser.FriendsResponse>(File.ReadAllText(fileName));
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
            var root = tree.Root;
            ListListII friendsMyFriends = new ListListII();
            ListII myFriends = fillInRelations(root.Friends);
            friendsMyFriends.Add(new KeyValuePair<int, ListII>(root.Id, myFriends));
            foreach (var friend in root.Friends)
            {
                var fRelations = new KeyValuePair<int, ListII>(friend.Id, fillInRelations(friend.Friends));
                friendsMyFriends.Add(fRelations);
            }
            var setFriends = getSetFriends(friendsMyFriends);
            int dim = setFriends.Count;

            Matrix res = createMatrix(setFriends, friendsMyFriends);
            return res;
        }

        private Matrix createMatrix(HashSet<int> friends, ListListII fRels)
        {
            // Создайте матрицу с нулевыми значениями
            Matrix res = new Matrix();
            foreach (var fIdV in friends)
            {
                var row = new Dictionary<int, int>();
                foreach (var fIdH in friends)
                    row.Add(fIdH, 0); // Уберите фигурные скобки
                res.Add(fIdV, row);
            }
            res = fillInMatrix(res, fRels);
            return res;
        }

        private Matrix fillInMatrix(Matrix init, ListListII data)
        {
            var res = init;
            foreach (var row in data)
                foreach (var fRel in row.Value)
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
                res.Add(new KeyValuePair<int, int>(friend.Id, 1));
            return res;
        }

        private HashSet<int> getSetFriends(ListListII tmpMatrix)
        {
            var res = new HashSet<int>();
            foreach (var it in tmpMatrix)
                foreach (var fRel in it.Value)
                    res.Add(fRel.Key);
            return res;
        }

        //private int nodeCount { get; set; }
        //private Dictionary<int, Vector<KeyValuePair<int, int>>> adjMatrix { get; set; }
    }
}
