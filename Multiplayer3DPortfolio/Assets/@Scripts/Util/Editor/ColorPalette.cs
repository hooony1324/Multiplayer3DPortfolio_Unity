using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "ColorPalette", menuName = "UI/Color Palette")]
public class ColorPalette : ScriptableObject
{
    [System.Serializable]
    public class ColorItem
    {
        public string colorName;
        public Color color;
    }

    public List<ColorItem> colors = new List<ColorItem>();
}