using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_parser
{
    public class FriendsView
    {
        private FriendTreeParser _parser;
        private FriendTree friendTree { get; set; }
        public FriendsView()
        {
            _parser= new FriendTreeParser();
            friendTree = _parser.ParseFriendTree();
        }
        
    }
}
