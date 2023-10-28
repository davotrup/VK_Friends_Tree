using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VK_parser
{
    public class Authorize_VK
    {
        public async Task GetAccessToken()
        {
            // Замените следующие значения на данные вашего приложения VK
            string clientId = "51780544";
            string clientSecret = "4eaoUlvEf2a1oy9T3j26";
            string redirectUri = "https://localhost:5001/";

            // Код авторизации, который вы получите после успешной авторизации пользователя
            string authorizationCode = "YOUR_AUTHORIZATION_CODE";

            // Создайте URL для обмена кода авторизации на access_token
            string tokenExchangeUrl = $"https://oauth.vk.com/access_token?client_id={clientId}&client_secret={clientSecret}&redirect_uri={redirectUri}&code={authorizationCode}";

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // Выполните POST-запрос для обмена кода авторизации на access_token
                    HttpResponseMessage response = await httpClient.PostAsync(tokenExchangeUrl, null);
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Обработайте ответ (responseContent) для извлечения access_token
                    Console.WriteLine(responseContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении access_token: {ex.Message}");
                }
            }
        }

        public static async Task GetAuthCode()
        {
            // Замените следующие значения на данные вашего приложения VK
            string clientId = "51780544";
            string redirectUri = "https://localhost:5001/auth/callback";
            string scope = "friends"; // Укажите необходимые разрешения VK

            // Создайте URL для авторизации пользователя
            string authUrl = $"https://oauth.vk.com/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&response_type=code&v=5.131";

            // Выведите URL для авторизации и попросите пользователя перейти по нему
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // Выполните POST-запрос для обмена кода авторизации на access_token
                    HttpResponseMessage response = await httpClient.PostAsync(authUrl, null);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                    // Обработайте ответ (responseContent) для извлечения access_token
                    //Console.WriteLine(responseContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении access_token: {ex.Message}");
                }
            }
        }
    }
}
