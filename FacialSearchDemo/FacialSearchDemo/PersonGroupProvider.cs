using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialSearchDemo
{
    public class PersonGroupProvider : BaseFaceApi
    {
        public async Task<bool> CreatePersonGroup(string groupID, string groupName)
        {
            try
            {
                InitializeClient();

                await _client.PersonGroup.DeleteWithHttpMessagesAsync(groupID);

                var result = await _client.PersonGroup.CreateWithHttpMessagesAsync(groupID, groupName);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Guid> CreatePerson(string groupID, string name)
        {
            try
            {
                InitializeClient();

                var result = await _client.PersonGroupPerson.CreateWithHttpMessagesAsync(groupID, name);

                return result.Body.PersonId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> RegisterImages(string groupid, Guid personid, string folderPath)
        {
            try
            {
                InitializeClient();

                foreach (string imagePath in Directory.GetFiles(folderPath, "*.jpg"))
                {
                    using (Stream s = File.OpenRead(imagePath))
                    {
                        // Detect faces in the image and add to Anna
                        await _client.PersonGroupPerson.AddFaceFromStreamWithHttpMessagesAsync(
                            groupid, personid, s);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public async Task<bool> TrainPersonGroup(string groupid)
        {
            try
            {
                InitializeClient();

                var result = await _client.PersonGroup.TrainWithHttpMessagesAsync(groupid);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Dictionary<Guid,FacePerson>> IdentifyFaces(string filePath,string groupID)
        {
            InitializeClient();

            Dictionary<Guid,FacePerson> ret = new Dictionary<Guid, FacePerson>();

            using (Stream s = File.OpenRead(filePath))
            {
                // The list of Face attributes to return.
                IList<FaceAttributeType> faceAttributes =
                    new FaceAttributeType[]
                    {
            FaceAttributeType.Gender, FaceAttributeType.Age,
            FaceAttributeType.Smile, FaceAttributeType.Emotion,
            FaceAttributeType.Glasses, FaceAttributeType.Hair
                    };

                var facesTask = await _client.Face.DetectWithStreamWithHttpMessagesAsync(s,true,true,faceAttributes);
                var faceIds = facesTask.Body.Select(face => face.FaceId.Value).ToList();

                var identifyTask = await _client.Face.IdentifyWithHttpMessagesAsync(faceIds,groupID);
                foreach (var identifyResult in identifyTask.Body)
                {
                    Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Count > 0)
                    { 
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await _client.PersonGroupPerson.GetWithHttpMessagesAsync(groupID, candidateId);

                        var fp = new FacePerson();
                        fp.PersonID = person.Body.PersonId;
                        fp.Name = person.Body.Name;
                        fp.FaceIds = person.Body.PersistedFaceIds.ToList();

                        var faceInstance = facesTask.Body.Where(f => f.FaceId.Value == identifyResult.FaceId).SingleOrDefault();
                        fp.Age = faceInstance.FaceAttributes.Age.ToString();
                        fp.EmotionAnger = faceInstance.FaceAttributes.Emotion.Anger.ToString();
                        fp.EmotionContempt = faceInstance.FaceAttributes.Emotion.Contempt.ToString();
                        fp.EmotionDisgust = faceInstance.FaceAttributes.Emotion.Disgust.ToString();
                        fp.EmotionFear = faceInstance.FaceAttributes.Emotion.Fear.ToString();
                        fp.EmotionHappiness = faceInstance.FaceAttributes.Emotion.Happiness.ToString();
                        fp.EmotionNeutral = faceInstance.FaceAttributes.Emotion.Neutral.ToString();
                        fp.EmotionSadness = faceInstance.FaceAttributes.Emotion.Sadness.ToString();
                        fp.EmotionSurprise = faceInstance.FaceAttributes.Emotion.Surprise.ToString();
                        fp.Gender = faceInstance.FaceAttributes.Gender.ToString();

                        ret.Add(person.Body.PersonId, fp);
                    }
                }
            }

            return ret;
        }
    }

    public class FacePerson
    {
        public Guid PersonID { get; set; }
        public string Name { get; set; }
        public List<Guid> FaceIds { get; set; }

        public Guid FaceID { get; set; }
        public string Age { get; set; }
        public string EmotionAnger { get; set; }
        public string EmotionContempt { get; set; }
        public string EmotionDisgust { get; set; }
        public string EmotionFear { get; set; }
        public string EmotionHappiness { get; set; }
        public string EmotionNeutral { get; set; }
        public string EmotionSadness { get; set; }
        public string EmotionSurprise { get; set; }
        public string Gender { get; set; }

        public FacePerson()
        {
            FaceIds = new List<Guid>();
        }
    }
}
