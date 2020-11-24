using System;
using Foundation.ConfigurationResolver;
using TMPro;
using UnityEngine;

namespace Foundation.CurrencySystem
{
    [Serializable]
    public enum Style
    {
        Front = 0, Big = 1, Triple = 2,
        NewStyle = 3
    }
    [Serializable]
    public class CurrencyStyle
    {
        public Style style;
        public Sprite icon;
        public int defaultFontSize;
        public TMP_FontAsset font;
    }
    
    [CreateAssetMenu(fileName = "CurrencyProviderConfig", menuName = "Foundation/Config/Create Currency Provider Config")]
    public class CurrencyProviderConfig : BaseConfig
    {
        [SerializeField] protected int typeId;
        public CurrencyStyle[] styles;
    }
}
