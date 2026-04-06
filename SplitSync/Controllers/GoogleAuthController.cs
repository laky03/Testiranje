using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Data;
using System.Security.Claims;
using System.Text.Json;

namespace SplitSync.Controllers
{
    [AllowAnonymous]
    public class GoogleAuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;

        public GoogleAuthController(AppDbContext context, IHttpClientFactory http, IConfiguration configuration)
        { 
            _context = context; 
            _http = http; 
            _configuration = configuration; 
        }

        [HttpGet]
        public IActionResult Start()
        {
            var clientId = _configuration["GoogleAuth:ClientId"]!;
            var redirectUri = _configuration["GoogleAuth:RedirectUri"]!;

            var url =
                $"https://accounts.google.com/o/oauth2/v2/auth" +
                $"?client_id={Uri.EscapeDataString(clientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&response_type=code" +
                $"&scope={Uri.EscapeDataString("openid email profile")}";

            return Redirect(url);
        }

        [HttpGet]
        public async Task<IActionResult> Callback(string? code, string? error = null)
        {
            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
                return RedirectToAction("Login", "Account");

            var clientId = _configuration["GoogleAuth:ClientId"]!;
            var clientSecret = _configuration["GoogleAuth:ClientSecret"]!;
            var redirectUri = _configuration["GoogleAuth:RedirectUri"]!;
            var http = _http.CreateClient();

            var tokenRequestBody = new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["code"] = code!,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = redirectUri
            };

            // Razmena koda za token 
            var tokenResponse = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequestBody));
            if (!tokenResponse.IsSuccessStatusCode) 
                return RedirectToAction("Login", "Account");

            var token = JsonSerializer.Deserialize<TokenResponse>(
                await tokenResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (string.IsNullOrEmpty(token?.access_token)) 
                return RedirectToAction("Login", "Account");

            // Razmena tokena za informacije o korisniku
            var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://openidconnect.googleapis.com/v1/userinfo");
            userInfoRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.access_token);
            var userInfoResponse = await http.SendAsync(userInfoRequest);
            if (!userInfoResponse.IsSuccessStatusCode) 
                return RedirectToAction("Login", "Account");

            var userInfo = JsonSerializer.Deserialize<UserInfo>(
                await userInfoResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (string.IsNullOrEmpty(userInfo?.sub)) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleSubject == userInfo.sub)
                    ?? (string.IsNullOrEmpty(userInfo.email) ? null
                        : await _context.Users.FirstOrDefaultAsync(u => u.Email == userInfo.email && u.IsEmailVerified));

            if (user != null)
            {
                if (string.IsNullOrEmpty(user.GoogleSubject))
                {
                    user.GoogleSubject = userInfo.sub;
                    await _context.SaveChangesAsync();
                }
                await SignIn(user);
                return RedirectToAction("Index", "Home");
            }

            HttpContext.Session.SetString("google_sub", userInfo.sub!);
            if (!string.IsNullOrEmpty(userInfo.email)) 
                HttpContext.Session.SetString("google_email", userInfo.email!);
            HttpContext.Session.SetString("google_given", userInfo.given_name ?? "");
            HttpContext.Session.SetString("google_family", userInfo.family_name ?? "");

            return RedirectToAction("GoogleUsername", "Account");
        }

        private async Task SignIn(Entities.User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email)
            };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });
        }

        private sealed class TokenResponse 
        {
            public string? access_token { get; set; } 
        }

        private sealed class UserInfo
        {
            public string? sub { get; set; }
            public string? email { get; set; }
            public string? given_name { get; set; }
            public string? family_name { get; set; }
        }
    }
}
