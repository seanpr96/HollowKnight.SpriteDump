using System;
using System.Collections.Generic;
using System.IO;
using Modding;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HollowKnight.SpriteDump
{
    public class SpriteDump : Mod
    {
        public override void Initialize()
        {
            On.tk2dSpriteAnimator.OnEnable += (orig, self) =>
            {
                orig(self);

                if (!Directory.Exists("Dump"))
                {
                    Directory.CreateDirectory("Dump");
                }

                foreach (tk2dSpriteAnimationClip clip in self.Library.clips)
                {
                    Log(clip.name);

                    if (!Directory.Exists("Dump/" + clip.name))
                    {
                        Directory.CreateDirectory("Dump/" + clip.name);
                    }
                    else
                    {
                        continue;
                    }

                    List<string> frameNames = new List<string>();

                    int i = 0;
                    foreach (tk2dSpriteAnimationFrame frame in clip.frames)
                    {
                        tk2dSpriteDefinition def = frame.spriteCollection.spriteDefinitions[frame.spriteId];
                        Texture2D tex = def.materialInst?.mainTexture as Texture2D;

                        if (tex == null)
                        {
                            Log("Null tex!");
                            continue;
                        }

                        if (frameNames.Contains(def.name))
                        {
                            continue;
                        }

                        frameNames.Add(def.name);

                        Log("----" + def.name + "----");

                        int x = (int)Math.Round(def.uvs[0].x * tex.width);
                        int h = (int)Math.Round((def.uvs[3].y * tex.height) - (def.uvs[0].y * tex.height));
                        int y = (int)Math.Round(tex.height - (def.uvs[0].y * tex.height) - h);
                        int w = (int)Math.Round((def.uvs[3].x * tex.width) - x);

                        Log("X: " + x);
                        Log("Y: " + y);
                        Log("W: " + w);
                        Log("H: " + h);

                        if (w <= 0 || h <= 0)
                        {
                            continue;
                        }

                        string fileName = "Dump/" + clip.name + "/" + (i++) + " - " + def.name + " (" +
                                          x + ", " + y +
                                          ", " + w + ", " +
                                          h + ")" + ".png";
                        if (File.Exists(fileName))
                        {
                            continue;
                        }

                        RenderTexture tmp = RenderTexture.GetTemporary(tex.width, tex.height, 0,
                            RenderTextureFormat.Default,
                            RenderTextureReadWrite.Linear);
                        Graphics.Blit(tex, tmp);
                        RenderTexture prev = RenderTexture.active;
                        RenderTexture.active = tmp;

                        Texture2D newTex = new Texture2D(w, h);
                        newTex.ReadPixels(new Rect(x, y, w, h), 0, 0);
                        newTex.Apply();
                        RenderTexture.active = prev;
                        RenderTexture.ReleaseTemporary(tmp);

                        File.WriteAllBytes(fileName, newTex.EncodeToPNG());
                        Object.Destroy(newTex);
                    }
                }
            };
        }
    }
}
