/* 
*   DBFace
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.Visualizers {

    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// </summary>
    [RequireComponent(typeof(RawImage), typeof(AspectRatioFitter)), DisallowMultipleComponent]
    public sealed class DBFaceVisualizer : MonoBehaviour {

        #region --Inspector--
        /// <summary>
        /// Face rectangle prefab.
        /// </summary>
        [Tooltip(@"Face rectangle prefab.")]
        public Image faceRect;
        #endregion


        #region --Client API--
        /// <summary>
        /// Detection source image.
        /// </summary>
        public Texture2D image {
            get => rawImage.texture as Texture2D;
            set {
                rawImage.texture = value;
                aspectFitter.aspectRatio = (float)value.width / value.height;
            }
        }

        /// <summary>
        /// Render a set of detected faces.
        /// </summary>
        /// <param name="faces">Faces to render.</param>
        public void Render (params Rect[] faces) {
            // Delete current
            foreach (var rect in currentRects)
                GameObject.Destroy(rect.gameObject);
            currentRects.Clear();
            // Display image
            var rawImage = GetComponent<RawImage>();
            var aspectFitter = GetComponent<AspectRatioFitter>();
            rawImage.texture = image;
            aspectFitter.aspectRatio = (float)image.width / image.height;            
            // Render rects
            foreach (var face in faces) {
                var prefab = Instantiate(faceRect, transform);
                prefab.gameObject.SetActive(true);
                Render(prefab, face);
                currentRects.Add(prefab);
            }
        }
        #endregion


        #region --Operations--
        private RawImage rawImage;
        private AspectRatioFitter aspectFitter;
        private readonly List<Image> currentRects = new List<Image>();

        void Awake () {
            rawImage = GetComponent<RawImage>();
            aspectFitter = GetComponent<AspectRatioFitter>();
        }

        void Render (Image prefab, Rect faceRect) {
            // Position
            var transform = prefab.transform as RectTransform;
            var imageTransform = rawImage.transform as RectTransform;
            transform.anchorMin = 0.5f * Vector2.one;
            transform.anchorMax = 0.5f * Vector2.one;
            transform.pivot = Vector2.zero;
            transform.sizeDelta = Vector2.Scale(imageTransform.rect.size, faceRect.size);
            transform.anchoredPosition = Rect.NormalizedToPoint(imageTransform.rect, faceRect.position);
        }
        #endregion
    }
}