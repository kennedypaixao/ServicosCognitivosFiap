using System;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Microsoft.CognitiveServices.Speech;
using System.Text;

namespace ServicosCognitivosFiap
{
	class Program
	{
		// Add your Computer Vision subscription key and endpoint
		static string vision_subscriptionKey = "vision_subscriptionKey";
		static string vision_endpoint = "vision_endpoint";

		static string speach_subscriptionKey = "speach_subscriptionKey";
		static string speach_serviceRegion = "speach_serviceRegion";

		//private const string READ_TEXT_URL_IMAGE = "https://1.bp.blogspot.com/-ZrH2Td5gObU/Xw_J8Vv03GI/AAAAAAAAE-I/qtHRKjKJtqIVOpqw2h8tXm9rprlf-M3IACLcBGAsYHQ/s1280/imagens-para-produ%25C3%25A7%25C3%25A3o-de-texto.jpg";
		private const string READ_TEXT_URL_IMAGE = "https://www.baixarvideosgratis.com.br/imagens/videos-de-bom-dia.png";

		private static StringBuilder TextToSpeach = new StringBuilder();

		/*
         * AUTHENTICATE
         * Creates a Computer Vision client used by each example.
         */
		public static ComputerVisionClient Authenticate(string endpoint, string key)
		{
			ComputerVisionClient client =
			  new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
			  { Endpoint = endpoint };
			return client;
		}

		/*
         * READ FILE - URL 
         * Extracts text. 
         */
		public static async Task ReadFileUrl(ComputerVisionClient client, string urlFile)
		{
			Console.WriteLine("----------------------------------------------------------");
			Console.WriteLine("READ FILE FROM URL");
			Console.WriteLine();

			// Read text from URL
			var textHeaders = await client.ReadAsync(urlFile);
			// After the request, get the operation location (operation ID)
			string operationLocation = textHeaders.OperationLocation;
			Thread.Sleep(2000);

			// Retrieve the URI where the extracted text will be stored from the Operation-Location header.
			// We only need the ID and not the full URL
			const int numberOfCharsInOperationId = 36;
			string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

			// Extract the text
			ReadOperationResult results;
			Console.WriteLine($"Extracting text from URL file {Path.GetFileName(urlFile)}...");
			Console.WriteLine();
			do
			{
				results = await client.GetReadResultAsync(Guid.Parse(operationId));
			}
			while ((results.Status == OperationStatusCodes.Running ||
				results.Status == OperationStatusCodes.NotStarted));

			// Display the found text.
			Console.WriteLine();
			var textUrlFileResults = results.AnalyzeResult.ReadResults;
			
			TextToSpeach.Clear();

			foreach (ReadResult page in textUrlFileResults)
			{
				foreach (Line line in page.Lines)
				{
					TextToSpeach.Append(line.Text + " ");
					Console.WriteLine(line.Text);
				}
			}
			Console.WriteLine();
		}

		/*
         * READ FILE - LOCAL
         */

		public static async Task ReadFileLocal(ComputerVisionClient client, string localFile)
		{
			Console.WriteLine("----------------------------------------------------------");
			Console.WriteLine("READ FILE FROM LOCAL");
			Console.WriteLine();

			// Read text from URL
			var textHeaders = await client.ReadInStreamAsync(File.OpenRead(localFile));
			// After the request, get the operation location (operation ID)
			string operationLocation = textHeaders.OperationLocation;
			Thread.Sleep(2000);

			// Retrieve the URI where the recognized text will be stored from the Operation-Location header.
			// We only need the ID and not the full URL
			const int numberOfCharsInOperationId = 36;
			string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

			// Extract the text
			ReadOperationResult results;
			Console.WriteLine($"Reading text from local file {Path.GetFileName(localFile)}...");
			Console.WriteLine();
			do
			{
				results = await client.GetReadResultAsync(Guid.Parse(operationId));
			}
			while ((results.Status == OperationStatusCodes.Running ||
				results.Status == OperationStatusCodes.NotStarted));

			// Display the found text.
			Console.WriteLine();
			var textUrlFileResults = results.AnalyzeResult.ReadResults;
			foreach (ReadResult page in textUrlFileResults)
			{
				foreach (Line line in page.Lines)
				{
					Console.WriteLine(line.Text);
				}
			}
			Console.WriteLine();
		}

		public static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
		{
			switch (speechSynthesisResult.Reason)
			{
				case ResultReason.SynthesizingAudioCompleted:
					Console.WriteLine($"Speech synthesized for text: [{text}]");
					break;
				case ResultReason.Canceled:
					var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
					Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

					if (cancellation.Reason == CancellationReason.Error)
					{
						Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
						Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
						Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
					}
					break;
				default:
					break;
			}
		}

		public async static Task Speach()
		{
			var speechConfig = SpeechConfig.FromSubscription(speach_subscriptionKey, speach_serviceRegion);

			// The language of the voice that speaks.
			speechConfig.SpeechSynthesisVoiceName = "pt-BR-AntonioNeural";

			using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
			{
				string text = TextToSpeach.ToString();
				var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
				OutputSpeechSynthesisResult(speechSynthesisResult, text);
			}
		}

		static void Main(string[] args)
		{
			Console.WriteLine("Azure Cognitive Services Computer Vision - .NET quickstart example");
			Console.WriteLine();

			ComputerVisionClient client = Authenticate(vision_endpoint, vision_subscriptionKey);

			ReadFileUrl(client, READ_TEXT_URL_IMAGE).Wait();

			Speach().Wait();

			Console.WriteLine("----------------------------------------------------------");
			Console.WriteLine();
			Console.WriteLine("Computer Vision quickstart is complete.");
			Console.WriteLine();
			Console.WriteLine("Press enter to exit...");
			Console.ReadLine();
		}

	}
}
