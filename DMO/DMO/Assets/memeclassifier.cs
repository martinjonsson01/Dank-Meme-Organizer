using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 59181889-0149-4bc1-845f-c70c6b1f6abd_61d2ce08-75cf-4c1e-af54-7d52814bfaa2

namespace DMO
{
    public sealed class _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2ModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2ModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2ModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new Dictionary<string, float>()
            {
                { "american chopper argument", float.NaN },
                { "anime", float.NaN },
                { "classical art", float.NaN },
                { "communism", float.NaN },
                { "deep fried", float.NaN },
                { "distracted boyfriend", float.NaN },
                { "expanding brain", float.NaN },
                { "greentext", float.NaN },
                { "gru's plan", float.NaN },
                { "justgirlythings", float.NaN },
                { "loss", float.NaN },
                { "manga", float.NaN },
                { "pepe", float.NaN },
                { "powder that makes you", float.NaN },
                { "protecc attac", float.NaN },
                { "rage comic", float.NaN },
                { "roll safe", float.NaN },
                { "spongebob", float.NaN },
                { "surreal", float.NaN },
                { "that's where you're wrong kiddo", float.NaN },
                { "top ten anime", float.NaN },
                { "who would win", float.NaN },
                { "yu-gi-oh card", float.NaN },
            };
        }
    }

    public sealed class _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2Model
    {
        private LearningModelPreview learningModel;
        public static async Task<_x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2Model> Create_x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2Model(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2Model model = new _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2Model();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<_x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2ModelOutput> EvaluateAsync(_x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2ModelInput input) {
            _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2ModelOutput output = new _x0035_9181889_x002D_0149_x002D_4bc1_x002D_845f_x002D_c70c6b1f6abd_61d2ce08_x002D_75cf_x002D_4c1e_x002D_af54_x002D_7d52814bfaa2ModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
