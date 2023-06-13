using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace PDD_Translate_Automatic
{
    internal static class GoogleTranslator
    {
        // Times to try failed web request
        private const int RETRY_COUNT = 3;

        // Delay between tries
        private const int RETRY_WAIT = 100;

        // Max length of path + query, including leading /
        private const int GOOGLE_LIMIT = 16384;

        private static Uri BuildUri(string sourceLanguage, string targetLanguage, string text)
        {
            var ub = new UriBuilder("http://translate.google.com/translate_a/t")
            {
                Query = $"client=gtx&ie=UTF-8&oe=UTF-8&sl={sourceLanguage}&tl={targetLanguage}&q={text}"
            };
            return ub.Uri;
        }

        public static bool TooLong(string text)
        {
            string source = PDDLanguage.Russian.GoogleCode;
            string target = PDDLanguage.Current.GoogleCode;
            Uri uri = BuildUri(source, target, text);
            return uri.PathAndQuery.Length > GOOGLE_LIMIT;
        }

        public static string Translate(string input)
        {
            if (TooLong(input))
            {
                return null;
            }

            string source = PDDLanguage.Russian.GoogleCode;
            string target = PDDLanguage.Current.GoogleCode;
            Uri uri = BuildUri(source, target, input);
            var webClient = new WebClient() { Encoding = Encoding.UTF8 };

            for (int tries = 0; tries < RETRY_COUNT; tries++)
            {
                try
                {
                    string json = webClient.DownloadString(uri);
                    string[] response = JsonConvert.DeserializeObject<string[]>(json);
                    if (response.Length != 1)
                    {
                        ProgressForm.Bug($"Unexpected response from Google Translate:\n\t{json}");
                        return null;
                    }

                    return string.Concat(response);
                }
                catch (WebException ex)
                {
                    ProgressForm.Bug($"Google request failed, retrying: {ex}");
                    Thread.Sleep(RETRY_WAIT);
                }
            }

            return null;
        }
    }
}
