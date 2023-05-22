using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly string key = "e1000ad9e30a471ea46305cf5d712117";
    private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com";

    // location, also known as region.
    // required if you're using a multi-service or regional (not global) resource. It can be found in the Azure portal on the Keys and Endpoint page.
    private static readonly string location = "centralindia";

    static async Task Main(string[] args)
    {
        // Input and output languages are defined as parameters.
        string route = "/translate?api-version=3.0&from=ar&to=en";
        string textToTranslate = "دعت حكومة ولاية ماهاراشترا شركة تيسلا الأمريكية لتصنيع السيارات الكهربائية التي يقودها إيلون موسك إلى الولاية. في وقت سابق حتى حكومة ولاية كارناتاكا عرضت عليهم إنشاء مصنع في ولاية الهند المذكورة أعلاه.\r\n\r\nعلى تويتر ، ألمح إيلون ماسك إلى أن الشركة يمكنها الدخول في مشروع إلى الهند في عام 2021.";
        object[] body = new object[] { new { Text = textToTranslate } };
        var requestBody = JsonConvert.SerializeObject(body);

        using (var client = new HttpClient())
        {
            // Build the request.
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", key);
            // location required if you're using a multi-service or regional (not global) resource.
            request.Headers.Add("Ocp-Apim-Subscription-Region", location);

            // Send the request and get the response.
            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            // Read the response as a string.
            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);

            // Deserialize the response to get the translated text.
            var translationResponse = JsonConvert.DeserializeObject<TranslationResponse[]>(result);
            var translatedText = translationResponse[0]?.Translations[0]?.Text;
            Console.WriteLine($"Translated Text: {translatedText}");
        }
    }
}

public class TranslationResponse
{
    public Translation[]? Translations { get; set; }
}

public class Translation
{
    public string? Text { get; set; }
    public string? To { get; set; }
}
