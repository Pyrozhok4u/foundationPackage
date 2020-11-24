using UnityEngine;

namespace Foundation.Sound
{
    [CreateAssetMenu(fileName = "SoundSettings", menuName = "Foundation/SoundService/SoundSettings")]
    public class SoundSettings : ScriptableObject
    {
        [Range(0, 256)] public int Priority = 128;

        [Range(0f, 1f)] public float Volume = 1f;

        [Range(-3f, 3f)] public float Pitch = 1f;
    }
}
