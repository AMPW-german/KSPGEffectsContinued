using KSPGEffectsContinued.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSPGEffectsContinued.Visuals
{
    internal class KSPGEffectsShader
    {
        private AssetBundle assetBundle;
        private Shader shader;
        private Material material;
        private RenderTexture renderTexture;
        private RenderTexture uiRenderTexture;
        private Camera uiCamera;
        Vector2Int screenSize;

        internal void Update(float GreyScaleLevel, float TunnelVisionLevel)
        {
            material.SetFloat("_GrayScaleLevel", GreyScaleLevel);
            material.SetFloat("_TunnelVisionLevel", TunnelVisionLevel);
            // Ensure the UI camera renders into the uiRenderTexture immediately
            // so we can use it as the source for the post-process material.
            uiCamera.Render();

            // Blit from the UI render target through the effect material into the final renderTexture.
            // Graphics.Blit(source, dest, material) automatically sets _MainTex = source for the shader.
            Graphics.Blit(uiRenderTexture, renderTexture, material);
        }

        internal void Draw()
        {
            GUI.DrawTexture(new Rect(0, 0, screenSize.x, screenSize.y), renderTexture, ScaleMode.ScaleAndCrop, true);
        }

        internal KSPGEffectsShader()
        {
            string filePath = $"{KSPUtil.ApplicationRootPath}GameData/KSPGEffectsContinued/Visuals/Shader/GEffectsShader";
            if (Application.platform == RuntimePlatform.LinuxPlayer || (Application.platform == RuntimePlatform.WindowsPlayer && SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL")))
            {
                filePath += "-linux.unity3d";
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                filePath += "-windows.unity3d";
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                filePath += "-macosx.unity3d";
            }

            assetBundle = AssetBundle.LoadFromFile(filePath);

            screenSize = new Vector2Int(GameSettings.SCREEN_RESOLUTION_WIDTH, GameSettings.SCREEN_RESOLUTION_HEIGHT);

            renderTexture = new RenderTexture(screenSize.x, screenSize.y, 32);
            uiRenderTexture = new RenderTexture(screenSize.x, screenSize.y, 32);

            Shader[] shaders = assetBundle.LoadAllAssets<Shader>();

            if (shaders.Length == 1)
            {
                shader = shaders.First();
            }
            else if (shaders.Length == 0)
            {
                GEffectsModLogging.LogStr("no shader in AssetBundle!", GEffectsLogic.Logging.Logger.LogLevel.Error);
                throw new ArgumentException("no shader in AssetBundle!");
            }
            else
            {
                GEffectsModLogging.LogStr("too many shaders in AssetBundle!", GEffectsLogic.Logging.Logger.LogLevel.Error);
                throw new ArgumentException("too many shaders in AssetBundle!");
            }

            material = new Material(shader);
            material.SetFloat("_GrayScaleLevel", 0.0f);
            material.SetFloat("_TunnelVisionLevel", 0.0f);
            material.SetFloat("_ScreenSizeAdjustment", KSPGEffectsContinued.vignetteShape * screenSize.y / screenSize.x);

            GameObject camObj = new GameObject("GEffectsVisuals");
            uiCamera = camObj.AddComponent<Camera>();
            uiCamera.orthographic = true;
            uiCamera.clearFlags = CameraClearFlags.Nothing;
            uiCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            uiCamera.cullingMask = LayerMask.GetMask("UI");
            // Render UI into a separate render target, then post-process into `renderTexture`.
            uiCamera.targetTexture = uiRenderTexture;

            // Create Canvas
            GameObject canvasObj = new GameObject("GEffectsCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera; // Assign the camera to the canvas
            canvasObj.layer = LayerMask.NameToLayer("UI"); // Use the "UI" layer

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(screenSize.x, screenSize.y); // Match render texture size
        }
    }
}
