using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace VK_Friends_Tree.Controllers
{
    public class AuthController : Controller
    {
        public static string clientId = "51780544";
        public static string clientSecret = "4eaoUlvEf2a1oy9T3j26";
        public static string redirectUri = "https://localhost:5001/auth/callback";
        public static string scope = "friends";

        // Код авторизации, который вы получите после успешной авторизации пользователя
        public static string authorizationCode { get; set; }

        // URL для получения Auth Code
        public static string authUrl = $"https://oauth.vk.com/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&response_type=code&v=5.131";
        // Создайте URL для обмена кода авторизации на access_token
        public static string tokenExchangeUrl = $"https://oauth.vk.com/access_token?client_id={clientId}&client_secret={clientSecret}&redirect_uri={redirectUri}&code={authorizationCode}";
        

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetCodePost()
        {
            // Здесь можно создать URL для авторизации и перенаправить пользователя
            string authUrl = $"https://oauth.vk.com/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&response_type=code&v=5.131";

            // Выполните перенаправление пользователя на authUrl
            return Redirect(authUrl);
        }
        [Route("/auth/callback")] // Указываете путь, который соответствует вашему redirect_uri
        public IActionResult Callback()
        {
            // Извлекаем параметр "code" из URL
            string _authorizationCode = HttpContext.Request.Query["code"];

            // Теперь у вас есть код авторизации, который можно использовать для обмена на access_token

            if (!string.IsNullOrEmpty(_authorizationCode))
            {
                // Далее можно выполнить запрос к VK API для обмена кода авторизации на access_token
                authorizationCode = _authorizationCode;
                string successMessage = _authorizationCode;
                return View("Success", successMessage);
            }
            else
            {
                // Обработка ошибки: отсутствует параметр "code" в URL
                string errorMessage = "Произошла ошибка при авторизации";
                return View("Error", errorMessage);
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetAccessTokenPost()
        {
            // Получение authorizationCode из формы (предполагается, что у вас есть форма для ввода кода)
            string _authorizationCode = authorizationCode;

            if (string.IsNullOrEmpty(_authorizationCode))
            {
                // Обработка ошибки: отсутствие кода авторизации
                return View("Error", "Код авторизации отсутствует.");
            }

            // Создайте URL для обмена кода авторизации на access_token
            string tokenExchangeUrl = $"https://oauth.vk.com/access_token?client_id={clientId}&client_secret={clientSecret}&redirect_uri={redirectUri}&code={authorizationCode}";

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // Выполните POST-запрос для обмена кода авторизации на access_token
                    HttpResponseMessage response = await httpClient.PostAsync(tokenExchangeUrl, null);
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Обработка JSON-ответа, который содержит access_token
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessTokenResponse>(responseContent);

                    if (string.IsNullOrEmpty(result.access_token))
                    {
                        // Обработка ошибки получения access_token
                        return View("Error", "Ошибка при получении access_token.");
                    }

                    // Теперь у вас есть access_token, который вы можете использовать
                    string accessToken = result.access_token;

                    // Вернуть страницу или выполнить дальнейшие действия
                    return View("Success", accessToken);
                }
                catch (Exception ex)
                {
                    // Обработка других ошибок
                    return View("Error", "Произошла ошибка: " + ex.Message);
                }
            }
        }
        // Модель представления для JSON-ответа об access_token
        public class AccessTokenResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public int user_id { get; set; }
        }
    }
}
