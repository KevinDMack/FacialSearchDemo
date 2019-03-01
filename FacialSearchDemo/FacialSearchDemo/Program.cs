using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialSearchDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var groupID = ConfigurationManager.AppSettings["GroupID"];
            var groupName = ConfigurationManager.AppSettings["GroupName"];
            var filePath = ConfigurationManager.AppSettings["FilePath"];
            var fileName = ConfigurationManager.AppSettings["TestImage"];

            Program.PopulatePersonGroups(groupID, groupName);

            Program.RunDescription(groupID, filePath + fileName);

            Program.RunSearch(groupID, filePath + fileName);

            Console.ReadLine();
        }

        private static void PopulatePersonGroups(string groupID, string groupName)
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("<*>---------------------------------------------------------<*>");
            Console.WriteLine("Populate Person Groups");
            Console.WriteLine("<*>---------------------------------------------------------<*>");

            string basePath = ConfigurationManager.AppSettings["FilePath"];
            var peopleNames = ConfigurationManager.AppSettings["PeopleNames"];
            List<string> people = new List<string>();
            people = peopleNames.Split(',').ToList();

            var personGroupProvider = new PersonGroupProvider();

            var groupTask = personGroupProvider.CreatePersonGroup(groupID, groupName);
            groupTask.Wait();

            foreach (var name in people)
            {
                var personTask = personGroupProvider.CreatePerson(groupID, name);
                personTask.Wait();
                var personId = personTask.Result;

                var registerTask = personGroupProvider.RegisterImages(groupID, personId, basePath + name);
                registerTask.Wait();

                var trainTask = personGroupProvider.TrainPersonGroup(groupID);
                trainTask.Wait();
            }
        }

        private static void RunSearch(string groupID, string filePath)
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("<*>---------------------------------------------------------<*>");
            Console.WriteLine("Run Facial Search");
            Console.WriteLine("<*>---------------------------------------------------------<*>");

            var personGroupProvider = new PersonGroupProvider();

            var searchTask = personGroupProvider.IdentifyFaces(filePath, groupID);

            searchTask.Wait();

            if (searchTask.Result.ToList().Count > 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Faces Found");
                foreach (var faceName in searchTask.Result)
                {
                    Console.WriteLine(string.Format("Person ID : {0} - Name : {1}", faceName.Key.ToString(), faceName.Value.Name));
                    Console.WriteLine("Persisted IDs:");
                    foreach (var id in faceName.Value.FaceIds)
                    {
                        Console.WriteLine("-" + id.ToString());
                    }
                    Console.WriteLine("");
                    Console.WriteLine(string.Format("Age: {0}", faceName.Value.Age));

                    Console.WriteLine(string.Format("Emotion - Anger: {0}", faceName.Value.EmotionAnger));
                    Console.WriteLine(string.Format("Emotion - Contempt: {0}", faceName.Value.EmotionContempt));
                    Console.WriteLine(string.Format("Emotion - Disgust: {0}", faceName.Value.EmotionDisgust));
                    Console.WriteLine(string.Format("Emotion - Fear: {0}", faceName.Value.EmotionFear));
                    Console.WriteLine(string.Format("Emotion - Happiness: {0}", faceName.Value.EmotionHappiness));
                    Console.WriteLine(string.Format("Emotion - Neutral: {0}", faceName.Value.EmotionNeutral));
                    Console.WriteLine(string.Format("Emotion - Sadness: {0}", faceName.Value.EmotionSadness));
                    Console.WriteLine(string.Format("Emotion - Surprise: {0}", faceName.Value.EmotionSurprise));

                    Console.WriteLine(string.Format("Gender: {0}", faceName.Value.Gender));
                }
            }
        }

        private static void RunDescription(string groupID, string filePath)
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("<*>---------------------------------------------------------<*>");
            Console.WriteLine("Run Computer Vision Description");
            Console.WriteLine("<*>---------------------------------------------------------<*>");

            var faceDetectionProvider = new FaceDetectionProvider();

            var task = faceDetectionProvider.UploadAndDetectFaces(filePath);

            task.Wait();

            foreach (var face in task.Result)
            {
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine(string.Format("Face ID: {0}", face.FaceId.ToString()));

                var landmarks = face.FaceLandmarks;
                var rectangle = face.FaceRectangle;
                Console.WriteLine(string.Format("Age: {0}", face.FaceAttributes.Age));

                Console.WriteLine(string.Format("Emotion - Anger: {0}", face.FaceAttributes.Emotion.Anger));
                Console.WriteLine(string.Format("Emotion - Contempt: {0}", face.FaceAttributes.Emotion.Contempt));
                Console.WriteLine(string.Format("Emotion - Disgust: {0}", face.FaceAttributes.Emotion.Disgust));
                Console.WriteLine(string.Format("Emotion - Fear: {0}", face.FaceAttributes.Emotion.Fear));
                Console.WriteLine(string.Format("Emotion - Happiness: {0}", face.FaceAttributes.Emotion.Happiness));
                Console.WriteLine(string.Format("Emotion - Neutral: {0}", face.FaceAttributes.Emotion.Neutral));
                Console.WriteLine(string.Format("Emotion - Sadness: {0}", face.FaceAttributes.Emotion.Sadness));
                Console.WriteLine(string.Format("Emotion - Surprise: {0}", face.FaceAttributes.Emotion.Surprise));

                Console.WriteLine("Accessories:");
                if (face.FaceAttributes.Accessories != null)
                {
                    foreach (var accessory in face.FaceAttributes.Accessories)
                    {
                        Console.WriteLine(string.Format("-{0} ({1})", accessory.Type, accessory.Confidence.ToString()));
                    }
                }
                else
                {
                    Console.WriteLine("-No Accessories Detected");
                }
                Console.WriteLine(string.Format("Exposure: {0}", face.FaceAttributes.Exposure));
                Console.WriteLine(string.Format("Facial Hair: {0}", face.FaceAttributes.FacialHair));
                Console.WriteLine(string.Format("Gender: {0}", face.FaceAttributes.Gender));
                Console.WriteLine(string.Format("Makeup: {0}", face.FaceAttributes.Makeup));
                Console.WriteLine(string.Format("Noise: {0}", face.FaceAttributes.Noise));
                Console.WriteLine(string.Format("Glasses: {0}", face.FaceAttributes.Glasses));
                Console.WriteLine(string.Format("Bald: {0}", face.FaceAttributes.Hair.Bald));

                if (face.FaceAttributes.Hair.HairColor != null)
                {
                    Console.WriteLine("Hair Colors:");
                    foreach (var haircolor in face.FaceAttributes.Hair.HairColor)
                    {
                        Console.WriteLine(string.Format("-{0} ({1})", haircolor.Color, haircolor.Confidence));
                    }
                }
            }
        }
    }
}
