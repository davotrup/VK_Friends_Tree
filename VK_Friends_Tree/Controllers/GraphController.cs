using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using static VK_Friends_Tree.Controllers.AuthController;
using System.Collections.Generic;
using Newtonsoft.Json;
using static VK_Friends_Tree.Controllers.GraphController;

namespace VK_Friends_Tree.Controllers
{
    public class GraphController : Controller
    {
        public int fCount { get; set; } = 100;
        public static string token { get; set; }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetFriendsPost(string accessToken)
        {
            token = accessToken;
            // URL метода VK API для получения информации о текущем пользователе
            string apiUrl = "https://api.vk.com/method/friends.get?access_token=" + accessToken + "&order=hints&fields=nickname, sex, bdate&v=5.131";

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        // Преобразование JSON-ответа в строку
                        string responseContent = await response.Content.ReadAsStringAsync();
                        // Обработка JSON-ответа
                        FriendsResponse result = JsonConvert.DeserializeObject<FriendsResponse>(responseContent);
                        string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                        System.IO.File.WriteAllText("my_friends.json", json);
                        // В этом примере вы можете просто вернуть его в представление
                        return View("Index");
                    }
                    else
                    {
                        // Обработка ошибки HTTP-запроса
                        return View("Error", "Ошибка при запросе к VK API: " + response.ReasonPhrase);
                    }
                }
                catch (Exception ex)
                {
                    // Обработка других ошибок
                    return View("Error", "Произошла ошибка: " + ex.Message);
                }
            }
            //return View("Success", accessToken);
        }
        public async Task<IActionResult> GetFriendsMyFriends()
        {
            FriendsResponse result = JsonConvert.DeserializeObject<FriendsResponse>(System.IO.File.ReadAllText("my_friends.json"));
            int counter = 0;
            foreach (var friend in result.friends.items)
            {
                if (counter++ == fCount)
                    break;
                string friendId = friend.id; // Идентификатор друга
                string friendFriendsUrl = $"https://api.vk.com/method/friends.get?access_token={token}&user_id={friendId}&order=hints&fields=nickname, sex, bdate&v=5.131";
                using (HttpClient httpClient = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(friendFriendsUrl);
                        if (response.IsSuccessStatusCode)
                        {//944722
                            // Преобразование JSON-ответа в строку
                            string responseContent = await response.Content.ReadAsStringAsync();
                            // Обработка JSON-ответа
                            FriendsResponse friends = JsonConvert.DeserializeObject<FriendsResponse>(responseContent);
                            string json = JsonConvert.SerializeObject(friends, Formatting.Indented);
                            System.IO.File.WriteAllText($"Friends/{friend.id}_{friend.last_name}_{friend.first_name}_friends.json", json);
                            // В этом примере вы можете просто вернуть его в представление
                            //return View("Index");
                        }
                        else
                        {
                            // Обработка ошибки HTTP-запроса
                            return View("Error", "Ошибка при запросе к VK API: " + response.ReasonPhrase);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Обработка других ошибок
                        return View("Error", "Произошла ошибка: " + ex.Message);
                    }
                    // Выполните запрос к друзьям друга и десериализуйте их список друзей
                }
            }
            return View("Success");
        }
        public class Friend
        {
            [JsonProperty("id")]
            public string id { get; set; }
            [JsonProperty("nickname")]
            public string nickname { get; set; }
            [JsonProperty("first_name")]
            public string first_name { get; set; }
            [JsonProperty("last_name")]
            public string last_name { get; set; }
            [JsonProperty("sex")]
            public string sex { get; set; }
            //public List<FriendsResponse> items { get; set; }
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
            public FriendsEnum friends { get; set; }
        }
    }
}
