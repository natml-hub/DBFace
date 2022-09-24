/* 
*   DBFace
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.Vision {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using NatML.Features;
    using NatML.Internal;
    using NatML.Types;

    /// <summary>
    /// DBFace face predictor.
    /// This predictor accepts an image feature and produces a list of face rectangles.
    /// Face rectangles are always specified in normalized coordinates.
    /// </summary>
    public sealed class DBFacePredictor : IMLPredictor<Rect[]> {

        #region --Client API--
        /// <summary>
        /// Create the DBFace predictor.
        /// </summary>
        /// <param name="model">DBFace ML model.</param>
        /// <param name="minScore">Minimum candidate score.</param>
        /// <param name="maxIoU">Maximum intersection-over-union score for overlap removal.</param>
        public DBFacePredictor (MLModel model, float minScore = 0.5f, float maxIoU = 0.5f) {
            this.model = model as MLEdgeModel;
            this.minScore = minScore;
            this.maxIoU = maxIoU;
            this.candidateBoxes = new List<Rect>(128);
            this.candidateScores = new List<float>(128);
        }

        /// <summary>
        /// Detect faces in an image.
        /// </summary>
        /// <param name="inputs">Input image.</param>
        /// <returns>Detected faces.</returns>
        public Rect[] Predict (params MLFeature[] inputs) {
            // Check
            if (inputs.Length != 1)
                throw new ArgumentException(@"DBFace predictor expects a single feature", nameof(inputs));
            // Check type
            var input = inputs[0];
            var imageType = MLImageType.FromType(input.type);
            var imageFeature = input as MLImageFeature;
            if (!imageType)
                throw new ArgumentException(@"DBFace predictor expects an an array or image feature", nameof(inputs));
            // Predict
            var inputType = model.inputs[0] as MLImageType;
            using var inputFeature = (input as IMLEdgeFeature).Create(inputType);
            using var outputFeatures = model.Predict(inputFeature);
            // Marshal
            var heatmap = new MLArrayFeature<float>(outputFeatures[2]); // (1,H,W,1)
            var boxes = new MLArrayFeature<float>(outputFeatures[1]);   // (1,H,W,4)
            var (heatmapWidth, heatmapHeight) = (heatmap.shape[2], heatmap.shape[1]);
            var (heatmapWidthInv, heatmapHeightInv) = (1f / heatmapWidth, 1f / heatmapHeight);
            candidateBoxes.Clear();
            candidateScores.Clear();
            for (var j = 0; j < heatmapHeight; ++j)
                for (var i = 0; i < heatmapWidth; ++i) {
                    var score = heatmap[0,j,i,0];
                    if (score < minScore)
                        continue;
                    var l = boxes[0,j,i,0];
                    var t = boxes[0,j,i,1];
                    var r = boxes[0,j,i,2];
                    var b = boxes[0,j,i,3];
                    var rawBox = Rect.MinMaxRect(
                        (i - l) * heatmapWidthInv,
                        1f - (j + b) * heatmapHeightInv,
                        (i + r) * heatmapWidthInv,
                        1f - (j - t) * heatmapHeightInv
                    );
                    var box = imageFeature?.TransformRect(rawBox, inputType) ?? rawBox;
                    candidateBoxes.Add(box);
                    candidateScores.Add(score);
                }
            var boxIndices = MLImageFeature.NonMaxSuppression(candidateBoxes, candidateScores, maxIoU);
            var result = boxIndices.Select(i => candidateBoxes[i]).ToArray();
            // Release
            return result;
        }
        #endregion


        #region --Operations--
        private readonly MLEdgeModel model;
        private readonly float minScore;
        private readonly float maxIoU;
        private readonly List<Rect> candidateBoxes;
        private readonly List<float> candidateScores;

        void IDisposable.Dispose () { } // Not used
        #endregion
    }
}