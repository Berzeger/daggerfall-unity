﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2018 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using UnityEngine.UI;

namespace DaggerfallWorkshop.Game.UserInterface
{
    /// <summary>
    /// Manages the render target texture for UI systems and provides helpers for drawing UI components.
    /// </summary>
    public class UserInterfaceRenderTarget : MonoBehaviour
    {
        #region Fields

        int createCount = 0;
        RenderTexture targetTexture = null;
        Rect targetRect = new Rect();

        #endregion

        #region Properties

        /// <summary>
        /// Gets current render target texture.
        /// </summary>
        public RenderTexture TargetTexture
        {
            get { return targetTexture; }
        }

        /// <summary>
        /// Gets rectangle of target texture.
        /// </summary>
        public Rect TargetRect
        {
            get { return targetRect; }
        }

        #endregion

        #region Unity

        private void Awake()
        {
            CheckTargetTexture();
            UpdateNonDiegeticOutput();
        }

        private void Update()
        {
            CheckTargetTexture();
        }

        private void OnGUI()
        {
            // Clear behind everything else
            GUI.depth = 10;

            if (Event.current.type != EventType.Repaint)
                return;

            if (IsReady())
                Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets diegetic canvas output raw image (if enabled)
        /// </summary>
        /// <returns>RawImage or null.</returns>
        public RawImage GetDiegeticCanvasRawImage()
        {
            // Must be able to find output canvas object
            GameObject nonDiegeticUIOutput = DaggerfallUI.Instance.NonDiegeticUIOutput;
            if (!nonDiegeticUIOutput)
                return null;

            // Output canvas object must be active
            if (!nonDiegeticUIOutput.activeInHierarchy)
                return null;

            // Get raw image component
            RawImage rawImage = nonDiegeticUIOutput.GetComponent<RawImage>();
            if (!rawImage)
                return null;

            return rawImage;
        }

        #endregion

        #region Drawing Methods

        public void Clear()
        {
            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = targetTexture;

            GL.Clear(true, true, Color.clear);

            RenderTexture.active = oldRt;
        }

        public void DrawTexture(Rect position, Texture2D image)
        {
            if (!IsReady())
                return;

            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = targetTexture;

            GUI.DrawTexture(position, image);

            RenderTexture.active = oldRt;
        }

        public void DrawTexture(Rect position, Texture2D image, ScaleMode scaleMode, bool alphaBlend = true, float imageAspect = 0)
        {
            if (!IsReady())
                return;

            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = targetTexture;

            GUI.DrawTexture(position, image, scaleMode, alphaBlend);

            RenderTexture.active = oldRt;
        }

        public void DrawTextureWithTexCoords(Rect position, Texture image, Rect texCoords, bool alphaBlend = true)
        {
            if (!IsReady())
                return;

            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = targetTexture;

            GUI.DrawTextureWithTexCoords(position, image, texCoords, alphaBlend);

            RenderTexture.active = oldRt;
        }

        #endregion

        #region Render Texture Management

        // Check render texture is non-null and created
        bool IsReady()
        {
            return (targetTexture != null && targetTexture.IsCreated());
        }

        // Check render texture and recreate if not valid
        void CheckTargetTexture()
        {
            // Just using screen dimensions for now while solving problems of redirecting rendering from UI components
            // Aiming for a baseline of 1:1 functionality with current setup before changing anything further
            int width = Screen.width;
            int height = Screen.height;

            // Just return same texture if still valid
            if (!IsReady() || targetTexture.width != width || targetTexture.height != height)
            {
                // Create target texture matching screen dimensions
                targetRect = new Rect(0, 0, width, height);
                targetTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                targetTexture.name = string.Format("DaggerfallUI RenderTexture {0}", createCount++);
                targetTexture.Create();
                UpdateNonDiegeticOutput();
                RaiseOnCreateTargetTexture();
                Debug.LogFormat("Created UI RenderTexture with dimensions {0}, {1}", width, height);
            }
        }

        void UpdateNonDiegeticOutput()
        {
            // Get raw image component
            RawImage rawImage = GetDiegeticCanvasRawImage();
            if (!rawImage)
                return;

            // Set target render texture to raw image output
            rawImage.texture = targetTexture;
            rawImage.SetNativeSize();
        }

        #endregion

        #region Events

        // OnCreateTargetTexture
        public delegate void OnCreateTargetTextureEventHandler();
        public event OnCreateTargetTextureEventHandler OnCreateTargetTexture;
        protected virtual void RaiseOnCreateTargetTexture()
        {
            if (OnCreateTargetTexture != null)
                OnCreateTargetTexture();
        }

        #endregion
    }
}