using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;

class Program
{
	private static readonly string computerVisionKey = "0e76869a9548428cba23bb73738c8c52";
	private static readonly string computerVisionEndpoint = "https://msvisionui.cognitiveservices.azure.com/";
	private static readonly string translationKey = "781b0ec20356402594ba03f44df18167";
	private static readonly string translationEndpoint = "https://api.cognitive.microsofttranslator.com";
	private static readonly string location = "centralindia";


	static async Task Main(string[] args)
	{
		// Path to the local image file
		string imagePath = @"C:\Users\Ankush\Downloads\Amazing-Facts-About-the-Sanskrit-Language-that-You-Didnt-Know.jpg";

		// Authenticate Computer Vision client
		var computerVisionClient = AuthenticateComputerVision(computerVisionEndpoint, computerVisionKey);

		// Analyze the image and extract text
		var extractedText = await ExtractTextFromImage(computerVisionClient, imagePath);

		// Translate the extracted text (if needed)
		var translatedText = await TranslateText(extractedText, "hi", "en");

		Console.WriteLine($"Extracted Text: {extractedText}");
		Console.WriteLine($"Translated Text: {translatedText}");
	}

	public static ComputerVisionClient AuthenticateComputerVision(string endpoint, string key)
	{
		ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
		return client;
	}

	public static async Task<string> ExtractTextFromImage(ComputerVisionClient client, string imagePath)
	{
		using (Stream imageStream = File.OpenRead(imagePath))
		{
			var textHeaders = await client.ReadInStreamAsync(imageStream, language: "hi");

			// Get the operation ID
			string operationId = textHeaders.OperationLocation.Split('/').Last();

			// Wait for the operation to complete
			while (true)
			{
				var result = await client.GetReadResultAsync(Guid.Parse(operationId));

				if (result.Status == OperationStatusCodes.Succeeded || result.Status == OperationStatusCodes.Failed)
				{
					var extractedText = new StringBuilder();
					foreach (var page in result.AnalyzeResult.ReadResults)
					{
						foreach (var line in page.Lines)
						{
							foreach (var word in line.Words)
							{
								extractedText.Append(word.Text);
								extractedText.Append(" ");
							}
						}
					}

					return extractedText.ToString().Trim();
				}

				await Task.Delay(1000);
			}
		}
	}

	public static async Task<string> TranslateText(string text, string sourceLanguage, string targetLanguage)
	{
		using (var client = new HttpClient())
		{
			string route = $"/translate?api-version=3.0&from={sourceLanguage}&to={targetLanguage}";
			string uri = translationEndpoint + route;

			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", translationKey);
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", location);

			var requestBody = new List<object>() { new { Text = text } };
			var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
			var response = await client.PostAsync(uri, content);

			response.EnsureSuccessStatusCode();

			string result = await response.Content.ReadAsStringAsync();

			var translationResponse = JsonConvert.DeserializeObject<List<TranslationResponse>>(result);
			var translations = translationResponse?[0]?.Translations;
			if (translations != null && translations.Count > 0)
			{
				var translatedText = translations[0]?.Text;
				return translatedText ?? string.Empty;
			}

			return string.Empty;
		}
	}
}

public class TranslationResponse
{
	public List<Translation> Translations { get; set; }
}

public class Translation
{
	public string Text { get; set; }
	public string To { get; set; }
}
