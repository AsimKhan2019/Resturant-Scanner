using System;
using System.Collections.Generic;

namespace LogoScanner
{
    public class PredictionResult
    {
        public string Id { get; set; }

        public string Project { get; set; }

        public string Iteration { get; set; }

        public DateTime Created { get; set; }

        public List<Prediction> Predictions { get; set; }

        public override string ToString()
        {
            var probability = double.MinValue;
            var name = "";
            foreach (Prediction type in Predictions)
            {
                if (type.Probability > probability && type.Probability >= 0.3)
                {
                    probability = type.Probability;
                    name = type.TagName;

                }
            }
            return name;
        }
    }
}