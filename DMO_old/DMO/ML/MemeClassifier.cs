using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 59181889-0149-4bc1-845f-c70c6b1f6abd_f8a4111c-7cd8-4129-881a-158f27e3edc1

namespace DMO.ML
{
    public sealed class MemeClassifierModelInput
    {
        public VideoFrame Data { get; set; }
    }

    public sealed class MemeClassifierModelOutput
    {
        public IList<string> ClassLabel { get; set; }
        public IDictionary<string, float> Loss { get; set; }
        public MemeClassifierModelOutput()
        {
            ClassLabel = new List<string>();
            Loss = new Dictionary<string, float>()
            {
                { "American Chopper Argument", float.NaN },
                { "Anime", float.NaN },
                { "Classical Art", float.NaN },
                { "Communism", float.NaN },
                { "Deep Fried", float.NaN },
                { "Distracted Boyfriend", float.NaN },
                { "Expanding Brain", float.NaN },
                { "Greentext", float.NaN },
                { "Gru's Plan", float.NaN },
                { "justgirlythings", float.NaN },
                { "Loss", float.NaN },
                { "Manga", float.NaN },
                { "Pepe", float.NaN },
                { "Powder That Makes You", float.NaN },
                { "Protecc Attac", float.NaN },
                { "Rage Comic", float.NaN },
                { "Roll Safe", float.NaN },
                { "Spongebob", float.NaN },
                { "Surreal", float.NaN },
                { "That's Where You're Wrong, Kiddo", float.NaN },
                { "Top Ten Anime", float.NaN },
                { "Who Would Win", float.NaN },
                { "Yu-Gi-Oh Card", float.NaN },
            };
        }
    }

    public sealed class MemeClassifierModel
    {
        private LearningModelPreview learningModel;
        public static async Task<MemeClassifierModel> CreateModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            MemeClassifierModel model = new MemeClassifierModel();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<MemeClassifierModelOutput> EvaluateAsync(MemeClassifierModelInput input) {
            MemeClassifierModelOutput output = new MemeClassifierModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.Data);
            binding.Bind("classLabel", output.ClassLabel);
            binding.Bind("loss", output.Loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
