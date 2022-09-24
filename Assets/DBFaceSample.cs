/* 
*   DBFace
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.Examples {

    using System.Threading.Tasks;
    using UnityEngine;
    using NatML.Devices;
    using NatML.Devices.Outputs;
    using NatML.Features;
    using NatML.Vision;
    using NatML.Visualizers;

    public sealed class DBFaceSample : MonoBehaviour {

        [Header(@"UI")]
        public DBFaceVisualizer visualizer;

        private CameraDevice cameraDevice;
        private TextureOutput cameraTextureOutput;

        private MLModelData modelData;
        private MLModel model;
        private DBFacePredictor predictor;

        async void Start () {
            // Request camera permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<CameraDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogError(@"User did not grant camera permissions");
                return;
            }
            // Get the default camera device
            var query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);
            cameraDevice = query.current as CameraDevice;
            // Start the camera preview
            cameraDevice.previewResolution = (640, 480);
            cameraTextureOutput = new TextureOutput();
            cameraDevice.StartRunning(cameraTextureOutput);
            // Display the camera preview
            var cameraTexture = await cameraTextureOutput;
            visualizer.image = cameraTexture;
            // Create the DBFace predictor
            Debug.Log("Fetching model data from NatML...");
            modelData = await MLModelData.FromHub("@natsuite/dbface");
            model = modelData.Deserialize();
            predictor = new DBFacePredictor(model);
        }

        void Update () {
            // Check that predictor has downloaded
            if (predictor == null)
                return;
            // Create input feature
            var inputFeature = new MLImageFeature(cameraTextureOutput.texture);
            (inputFeature.mean, inputFeature.std) = modelData.normalization;
            inputFeature.aspectMode = modelData.aspectMode;
            // Predict
            var faces = predictor.Predict(inputFeature);
            // Visualize
            visualizer.Render(faces);
        }

        void OnDisable () {
            // Dispose the model
            model?.Dispose();
        }
    }
}