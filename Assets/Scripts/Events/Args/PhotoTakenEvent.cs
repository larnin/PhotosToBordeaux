using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PhotoTakenEvent : EventArgs
{
    public PhotoTakenEvent(Texture2D _texture, int _photoCount, int _maxPhotoCount)
    {
        texture = _texture;
        photoCount = _photoCount;
        maxPhotoCount = _maxPhotoCount;
    }
    
    public Texture2D texture;
    public int photoCount;
    public int maxPhotoCount;
}