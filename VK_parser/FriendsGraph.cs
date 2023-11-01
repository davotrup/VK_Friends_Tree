using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Matrix = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, int>>;
using ListII = System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, int>>;

namespace VK_parser
{
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
                Id = 00000,
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

            Matrix res = createMatrix(ref setFriends, ref friendsMyFriends);
            return res;
        }

        private Matrix createMatrix(ref HashSet<int> friends, ref ListListII fRels)
        {
            Matrix res = new Matrix();
            foreach (var fIdV in friends)
            {
                var row = new Dictionary<int, int>();
                foreach (var fIdH in friends)
                    row.Add(fIdH, 0);
                res.Add(fIdV, row);
            }
            fillInMatrix(ref res, ref fRels);
            return res;
        }

        private void fillInMatrix(ref Matrix init, ref ListListII data)
        {
            foreach (var row in data)
                foreach (var fRel in row.Value)
                {
                    init[row.Key][fRel.Key] = fRel.Value;
                    init[fRel.Key][row.Key] = fRel.Value;
                }
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
        
        public Dictionary<int, List<List<int>>> GetShortestPaths(int node)
        {
            var queue = new List<(int, List<int>)> { (node, new List<int>()) };
            HashSet<int> visited = new HashSet<int>();
            var  paths = new Dictionary<int, List<List<int>>>();

            while (queue.Count > 0)
            {
                (int current_node, List<int> current_path) = queue[0];
                queue.RemoveAt(0);
                visited.Add(current_node);

                if (!paths.ContainsKey(current_node))
                    paths[current_node] = new List<List<int>>();

                if (!paths[current_node].Contains(current_path))
                    paths[current_node].Add(current_path);

                List<int> neighbors = new List<int>();
                for (int i = 0; i < num_nodes; i++)
                    if (adj_matrix[current_node, i] == 1)
                        neighbors.Add(i);

                foreach (int neighbor in neighbors)
                    if (!visited.Contains(neighbor))
                        queue.Add((neighbor, new List<int>(current_path) { neighbor }));
            }

            return paths;
        }

        public Dictionary<int, double> BetweennessCentrality()
        {
            var betweenness = new Dictionary<int, double>();
            for (int node = 0; node < num_nodes; node++)
            {
                Dictionary<int, List<List<int>>> paths = GetShortestPaths(node);
                int total_paths = paths.Count;

                foreach (int source in paths.Keys)
                    foreach (int target in paths.Keys)
                        if (source != target)
                            foreach (List<int> path in paths[source])
                                if (path.Contains(target))
                                {
                                    if (!betweenness.ContainsKey(node))
                                        betweenness[node] = 0;
                                    betweenness[node] += 1.0 / total_paths;
                                }
            }

            return betweenness;
        }

        public Dictionary<int, double> ClosenessCentrality()
        {
            var closeness = new Dictionary<int, double>();

            for (int node = 0; node < num_nodes; node++)
            {
                int total_distance = 0;
                Dictionary<int, List<List<int>>> paths = GetShortestPaths(node);

                foreach (int target in paths.Keys)
                    if (target != node)
                        total_distance += paths[target][0].Count - 1;

                closeness[node] = 
                    total_distance > 0 ? (double)(paths.Count - 1) / total_distance : 0;
            }

            return closeness;
        }

        public double[] EigenvectorCentrality()
        {
            double[] eigenvalues;
            double[,] eigenvectors;
            double[] dominant_eigenvector = 
                eigenvectors.GetColumn(np.argmax(eigenvalues));
            double sum = dominant_eigenvector.Sum();
            double[] centrality = new double[dominant_eigenvector.Length];

            for (int i = 0; i < dominant_eigenvector.Length; i++)
                centrality[i] = dominant_eigenvector[i] / sum;

            return centrality;
        }
    }
}
