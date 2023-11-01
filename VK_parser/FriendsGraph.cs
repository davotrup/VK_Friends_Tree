using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Matrix = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, int>>;
using ListII = System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, int>>;
using System.Linq;

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
        private int fCount { get; set; } = 10;
        private int num_nodes { get; set; }

        public FriendTreeParser()
        {
            this.num_nodes = 0;
        }

        public FriendTree ParseFriendTree()
        {
            var friendTree = new FriendTree();

            var myFriendsJson = File.ReadAllText("C:\\Users\\ВТБ\\source\\repos\\VK_Friends_Tree\\VK_parser\\my_friends.json");
            var myFriendsResponse = JsonConvert.DeserializeObject<FriendsResponse>(myFriendsJson);
            var myFriends = myFriendsResponse.friends.items.Take(fCount);

            var root = friendTree.Root;
            root.Friends.AddRange(myFriends.Take(fCount));
            int counter = 0;
            foreach (var friend in myFriends)
            {
                /*if (counter++ == fCount)
                    break;*/
                var friends = friend.Friends;
                root.Friends[root.Friends.IndexOf(friend)].parent = root;
                var fileName = 
                    $"C:\\Users\\ВТБ\\source\\repos\\VK_Friends_Tree\\VK_parser\\Friends\\{friend.Id}_{friend.LastName}_{friend.FirstName}_friends.json";

                if (File.Exists(fileName))
                {
                    var friendsResponse =
                        JsonConvert.DeserializeObject<FriendsResponse>(File.ReadAllText(fileName));
                    if (friendsResponse.friends != null)
                    {
                        int counter2 = 0;
                        friends.AddRange(friendsResponse.friends.items.Take(fCount));
                        foreach (var frienMyFriend in friends)
                        {
                            /*if (counter2++ == fCount)
                                break;*/
                            friends[friends.IndexOf(frienMyFriend)].parent = friend;
                        }
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
            num_nodes = setFriends.Count;
            //int dim = setFriends.Count;

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
            res.Add(0, new Dictionary<int, int>());
            res[0].Add(0, 0);
            foreach (var f in friends)
                res[0].Add(f, 0);
            fillInMatrix(ref res, ref fRels);
            return res;
        }

        private void fillInMatrix(ref Matrix init, ref ListListII data)
        {
            foreach (var row in data)
            {
                /*if (row.Value.Key == 0)
                    continue;*/
                foreach (var fRel in row.Value)
                {
                    init[row.Key][fRel.Key] = fRel.Value;
                    init[fRel.Key][row.Key] = fRel.Value;
                }

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
        
        public Dictionary<int, List<List<int>>> GetShortestPaths(int friendId, ref Matrix adjMatrix)
        {
            var queue = new List<(int, List<int>)> { (friendId, new List<int>()) };
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
                foreach (var it in adjMatrix[current_node])
                    if (it.Value == 1)
                        neighbors.Add(it.Key);

                foreach (int neighbor in neighbors)
                    if (!visited.Contains(neighbor))
                        queue.Add((neighbor, new List<int>(current_path) { neighbor }));
            }

            return paths;
        }

        public Dictionary<int, double> BetweennessCentrality(ref Matrix adjMatrix)
        {
            var betweenness = new Dictionary<int, double>();
            foreach (var node in adjMatrix)
            {
                Dictionary<int, List<List<int>>> paths = GetShortestPaths(node.Key, ref adjMatrix);
                int total_paths = paths.Count;

                foreach (int source in paths.Keys)
                    foreach (int target in paths.Keys)
                        if (source != target)
                            foreach (List<int> path in paths[source])
                                if (path.Contains(target))
                                {
                                    if (!betweenness.ContainsKey(node.Key))
                                        betweenness[node.Key] = 0;
                                    betweenness[node.Key] += 1.0 / total_paths;
                                }
            }

            return betweenness;
        }

        public Dictionary<int, double> ClosenessCentrality(ref Matrix adjMatrix)
        {
            var closeness = new Dictionary<int, double>();

            foreach (var node in adjMatrix)
            {
                int total_distance = 0;
                Dictionary<int, List<List<int>>> paths = GetShortestPaths(node.Key, ref adjMatrix);

                foreach (int target in paths.Keys)
                    if (target != node.Key)
                        total_distance += paths[target][0].Count - 1;

                closeness[node.Key] = 
                    total_distance > 0 ? (double)(paths.Count - 1) / total_distance : 0;
            }

            return closeness;
        }

        /*public double[] EigenvectorCentrality(double[][] adjMatrix)
        {
            int numNodes = adjMatrix.Length;
            (double eigenvalue, double[] eigenvector) = PowerIteration(adjMatrix);
            double[] dominantEigenvector = eigenvector;
            double sum = dominantEigenvector.Sum();
            double[] centrality = dominantEigenvector.Select(v => v / sum).ToArray();
            return centrality;
        }*/

        public (double, Dictionary<int, double>) PowerIteration(ref Matrix matrix, int numIterations = 100)
        {
            int numNodes = matrix.Count;
            var vector = new Dictionary<int, double>();
            foreach (var key in matrix.Keys)
                vector.Add(key, 1.0);

            for (int iteration = 0; iteration < numIterations; iteration++)
            {
                var nextVector = new Dictionary<int, double>();
                foreach (var key in matrix.Keys)
                    nextVector.Add(key, 0);

                foreach(var it in matrix)
                {
                    foreach(var node in it.Value)
                    {
                        nextVector[it.Key] += node.Key * vector[node.Key];
                    }
                }

                double norm = nextVector.Values.Max();
                foreach (var it in vector)
                    vector[it.Key] = it.Value/norm;
            }

            double eigenvalue = 0.0;

            foreach (var it in matrix)
            {
                foreach (var node in it.Value)
                {
                    eigenvalue += node.Key * vector[it.Key] * vector[node.Key];
                }
            }
            return (eigenvalue, vector);
        }
    }
}
