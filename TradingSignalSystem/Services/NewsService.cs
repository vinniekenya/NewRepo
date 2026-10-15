using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TradingSignalSystem.Services  // ✅ Add namespace
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private string apiKey = "c3db8625213f4f2d85b035e1b7c5f1bf";

        public NewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<dynamic?> GetForexNews()  // ✅ nullable dynamic
        {
            try
            {
                string url = $"https://newsapi.org/v2/everything?q=forex OR \"interest rate\" OR inflation OR cpi OR fomc OR \"central bank\" OR usd OR eur OR gbp&language=en&sortBy=publishedAt&pageSize=20&apiKey={apiKey}";

                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return null;

                var data = JsonConvert.DeserializeObject<dynamic>(json);

                if (data?.status != "ok")
                    return null;

                return data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public bool HasHighImpactNews(dynamic? news)  // ✅ nullable
        {
            if (news == null || news.articles == null)
                return false;

            string[] keywords = {
                "interest rate", "inflation", "cpi",
                "nfp", "non-farm", "fomc", "fed", "ecb"
            };

            foreach (var article in news.articles)
            {
                string title = article.title?.ToString().ToLower() ?? "";
                foreach (var keyword in keywords)
                    if (title.Contains(keyword)) return true;
            }

            return false;
        }

        public int GetNewsScore(dynamic? news)  // ✅ nullable
        {
            if (news == null || news.articles == null) return 0;

            int score = 0;
            foreach (var article in news.articles)
            {
                string title = article.title?.ToString().ToLower() ?? "";
                if (!IsRelevantForexNews(title)) continue;

                if (title.Contains("non-farm") || title.Contains("nfp")) score += 10;
                else if (title.Contains("interest rate") || title.Contains("fomc")) score += 9;
                else if (title.Contains("inflation") || title.Contains("cpi")) score += 8;
                else if (title.Contains("gdp")) score += 7;
                else if (title.Contains("employment")) score += 6;
                else if (title.Contains("fed") || title.Contains("ecb") || title.Contains("boe")) score += 5;
                else if (title.Contains("market") || title.Contains("forecast")) score += 2;
            }
            return score;
        }

        public int GetPairSpecificScore(dynamic? news, string pair)  // ✅ nullable
        {
            if (news == null || news.articles == null) return 0;

            int score = 0;
            string baseCurrency = pair.Substring(0, 3).ToLower();
            string quoteCurrency = pair.Substring(3, 3).ToLower();

            var currencyMap = new Dictionary<string, string[]>
            {
                { "eur", new[] { "euro", "ecb" } },
                { "usd", new[] { "dollar", "fed", "usd" } },
                { "gbp", new[] { "pound", "boe" } },
                { "jpy", new[] { "yen", "boj" } }
            };

            foreach (var article in news.articles)
            {
                string title = article.title?.ToString().ToLower() ?? "";
                if (!IsRelevantForexNews(title)) continue;

                bool relevantToPair = false;

                if (currencyMap.ContainsKey(baseCurrency))
                    foreach (var word in currencyMap[baseCurrency])
                        if (title.Contains(word)) relevantToPair = true;

                if (currencyMap.ContainsKey(quoteCurrency))
                    foreach (var word in currencyMap[quoteCurrency])
                        if (title.Contains(word)) relevantToPair = true;

                if (!relevantToPair) continue;

                if (title.Contains("non-farm") || title.Contains("nfp")) score += 10;
                else if (title.Contains("interest rate") || title.Contains("fomc")) score += 9;
                else if (title.Contains("inflation") || title.Contains("cpi")) score += 8;
                else if (title.Contains("gdp")) score += 7;
                else if (title.Contains("employment")) score += 6;
                else if (title.Contains("fed") || title.Contains("ecb") || title.Contains("boe")) score += 5;
                else if (title.Contains("market") || title.Contains("forecast")) score += 2;
            }
            return score;
        }

        private bool IsRelevantForexNews(string title)
        {
            return title.Contains("usd") || title.Contains("dollar") ||
                   title.Contains("eur") || title.Contains("euro") ||
                   title.Contains("gbp") || title.Contains("pound") ||
                   title.Contains("jpy") || title.Contains("yen") ||
                   title.Contains("inflation") || title.Contains("interest rate") ||
                   title.Contains("fed") || title.Contains("ecb") ||
                   title.Contains("boe") || title.Contains("central bank");
        }
    }
}