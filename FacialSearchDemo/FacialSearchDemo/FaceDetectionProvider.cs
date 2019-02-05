using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialSearchDemo
{
    public class FaceDetectionProvider : BaseFaceApi
    {


        public FaceDetectionProvider()
        {

        }

        // Uploads the image file and calls DetectWithStreamAsync.
        public async Task<IList<DetectedFace>> UploadAndDetectFaces(string imageFilePath)
        {
            InitializeClient();

            // The list of Face attributes to return.
            IList<FaceAttributeType> faceAttributes =
                new FaceAttributeType[]
                {
            FaceAttributeType.Gender, FaceAttributeType.Age,
            FaceAttributeType.Smile, FaceAttributeType.Emotion,
            FaceAttributeType.Glasses, FaceAttributeType.Hair
                };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    // The second argument specifies to return the faceId, while
                    // the third argument specifies not to return face landmarks.
                    IList<DetectedFace> faceList =
                        await _client.Face.DetectWithStreamAsync(
                            imageFileStream, true, false, faceAttributes);
                    return faceList;
                }
            }
            // Catch and display Face API errors.
            catch (APIErrorException f)
            {
                throw new Exception(f.Message);
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
