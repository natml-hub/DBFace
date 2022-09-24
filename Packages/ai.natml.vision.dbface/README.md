# DBFace
[DBFace](https://github.com/dlunion/DBFace) face detection.

## Detecting Faces in an Image
First, create the DBFace predictor:
```csharp
// Fetch the model data from NatML
var modelData = await MLModelData.FromHub("@natsuite/dbface");
// Deserialize the model
var model = modelData.Deserialize();
// Create the DBFace predictor
var predictor = new DBFacePredictor(model);
```

Then detect faces in the image:
```csharp
// Create image feature
Texture2D image = ...;
var imageFeature = new MLImageFeature(image); // This also accepts a `Color32[]` or `byte[]`
(imageFeature.mean, imageFeature.std) = modelData.normalization;
imageFeature.aspectMode = modelData.aspectMode;
// Detect faces
Rect[] faces = predictor.Predict(imageFeature);
```

___

## Requirements
- Unity 2021.2+

## Quick Tips
- Join the [NatML community on Discord](https://hub.natml.ai/community).
- Discover more ML models on [NatML Hub](https://hub.natml.ai).
- See the [NatML documentation](https://docs.natml.ai/unity).
- Discuss [NatML on Unity Forums](https://forum.unity.com/threads/open-beta-natml-machine-learning-runtime.1109339/).
- Contact us at [hi@natml.ai](mailto:hi@natml.ai).

Thank you very much!