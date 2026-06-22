using KSPGEffectsContinued.Logging;
using Smooth.Compare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static KSP.UI.Screens.RDArchivesController;

namespace KSPGEffectsContinued.Visuals
{
    internal class KSPGEffectsVisual
    {
        static Texture2D blackoutTexture = null;
        static Texture2D intensifier = new Texture2D(1, 1);
        static Color visualsColor = new Color();
        static Color intensifierColor = new Color();

        internal static void drawGEffects(float TunnelVisionLevel)
        {
            if (TunnelVisionLevel > 0)
            {
                visualsColor = Color.black;
                visualsColor.a = TunnelVisionLevel;

                intensifierColor.r = visualsColor.r;
                intensifierColor.g = visualsColor.g;
                intensifierColor.b = visualsColor.b;
                intensifierColor.a = (float)Math.Pow(TunnelVisionLevel, 4); //this will intensify blackout effect at the very end

                intensifier.SetPixel(0, 0, intensifierColor);
                intensifier.Apply();

                GUI.color = visualsColor;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackoutTexture);
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), intensifier);
            }
        }

        public static void initializeCameraFilter()
        {
            if (blackoutTexture == null)
            {
                blackoutTexture = GameDatabase.Instance.GetTexture("KSPGEffectsContinued/visuals/blackout", false);
            }
        }
    }
}
