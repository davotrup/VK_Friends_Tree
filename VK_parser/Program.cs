using System;
using System.Net.Http;
using System.Threading.Tasks;
using VK_Friends_Tree;

//link = https://oauth.vk.com/authorize?client_id=51780544&display=page&\
//redirect_uri=https://localhost:5001/&scope=friends&response_type=code&v=5.131

namespace VK_parser
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            FriendTreeParser friendTreeParser = new FriendTreeParser();
            FriendTree friendTree = friendTreeParser.ParseFriendTree();
            Console.WriteLine("-------");
            //await Authorize_VK.GetAuthCode();
        }
    }
}
