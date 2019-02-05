using Microsoft.Azure.CognitiveServices.Vision.Face;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialSearchDemo
{
    public class BaseFaceApi
    {
        protected string _apiKey = ConfigurationManager.AppSettings["FaceApiKey"];
        protected string _apiUrl = ConfigurationManager.AppSettings["FaceApiUrl"];
        protected string _apiEndpoint = ConfigurationManager.AppSettings["FaceApiEndpoint"];
        protected FaceClient _client;

        protected void InitializeClient()
        {
            _client = new FaceClient(new ApiKeyServiceClientCredentials(_apiKey));
            _client.Endpoint = _apiEndpoint;
        }
    }
}
