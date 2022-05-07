using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GhostloreAP
{
    public static class SpriteFactory
    {
        public static Sprite LoadSprite(string fileName)
        {
            string path = GLAPModLoader.modInfo.Folder + "/" + fileName; 
            byte[] imgByteArr = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2,2);
            tex.filterMode = FilterMode.Point;
            ImageConversion.LoadImage(tex,imgByteArr);
            
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
    }
}
