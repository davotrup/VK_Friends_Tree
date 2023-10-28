using System;
using System.Net.Http;
using System.Threading.Tasks;
using static VK_parser.Authorize_VK;

//link = https://oauth.vk.com/authorize?client_id=51780544&display=page&redirect_uri=https://localhost:5001/&scope=friends&response_type=code&v=5.131

namespace VK_parser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Authorize_VK.GetAuthCode();
        }
    }
}
