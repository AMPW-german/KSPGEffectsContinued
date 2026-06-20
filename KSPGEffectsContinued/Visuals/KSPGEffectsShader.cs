using KSPGEffectsContinued.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static KSP.UI.Screens.RDArchivesController;

namespace KSPGEffectsContinued.Visuals
{
    internal class KSPGEffectsShader : MonoBehaviour
    {
        private static AssetBundle assetBundle;
        private static Shader shader;
        private static Material material;
        private static Vector2Int screenSize;

        internal void UpdateLevels(float GreyScaleLevel, float TunnelVisionLevel)
        {
            material.SetFloat("_GrayScaleLevel", GreyScaleLevel);
            material.SetFloat("_TunnelVisionLevel", TunnelVisionLevel);
            //// Ensure the UI camera renders into the uiRenderTexture immediately
            //// so we can use it as the source for the post-process material.
            //uiCamera.Render();

            //// Blit from the UI render target through the effect material into the final renderTexture.
            //// Graphics.Blit(source, dest, material) automatically sets _MainTex = source for the shader.
            //Graphics.Blit(uiRenderTexture, renderTexture, material);
        }

        //internal void Draw()
        //{
        //    GUI.DrawTexture(new Rect(0, 0, screenSize.x, screenSize.y), renderTexture, ScaleMode.ScaleAndCrop, true);
        //}

        void OnRenderImage(RenderTexture source, RenderTexture dest)
        {
            if ((material != null))
            {
                //material.SetFloat("_Magnitude", magnitude);
                Graphics.Blit(source, dest, material);
            }
        }

        private static void LoadMaterial()
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
        }

        public static KSPGEffectsShader initializeCameraFilter(Camera camera)
        {
            if (material == null) LoadMaterial();

            KSPGEffectsShader filter;

            Camera[] flightCameras = FlightCamera.fetch.cameras;

            // Attach the post-process component to all cameras so the effect can run
            // for both world and UI cameras. Previously we excluded flight cameras
            // which prevented the post-process from being applied consistently over
            // the final composited frame.
            foreach (Camera item in Camera.allCameras)
            {
                filter = item.gameObject.GetComponent<KSPGEffectsShader>();
                if (filter == null)
                {
                    filter = item.gameObject.AddComponent<KSPGEffectsShader>();
                }
            }

            filter = camera.gameObject.GetComponent<KSPGEffectsShader>();
            if (filter == null)
            {
                filter = camera.gameObject.AddComponent<KSPGEffectsShader>();
            }
            return filter;
        }
    }
}
