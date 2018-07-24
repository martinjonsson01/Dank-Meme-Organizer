using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 59181889-0149-4bc1-845f-c70c6b1f6abd_f8a4111c-7cd8-4129-881a-158f27e3edc1

namespace DMO
{
    public sealed class _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1ModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1ModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new Dictionary<string, float>()
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

    public sealed class _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1Model
    {
        private LearningModelPreview learningModel;
        public static async Task<_x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1Model> Create_x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1Model(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1Model model = new _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1Model();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<_x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1ModelOutput> EvaluateAsync(_x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1ModelInput input) {
            _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1ModelOutput output = new _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_f8a4111c_x002D_7cd8_x002D_4129_x002D_881a_x002D_158f27e3edc1ModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
