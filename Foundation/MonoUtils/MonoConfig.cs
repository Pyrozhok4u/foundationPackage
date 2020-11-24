using Foundation.ConfigurationResolver;
using UnityEngine;

namespace Foundation.MonoUtils
{
    public class MonoConfig : BaseConfig
    {

        public int SlowUpdateRate = 10;
        public bool LateUpdateEnabled;
        public bool FixedUpdateEnabled;

    }
}
