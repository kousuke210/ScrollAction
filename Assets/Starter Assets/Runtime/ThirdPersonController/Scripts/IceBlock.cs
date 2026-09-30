using UnityEngine;

namespace StarterAssets
{
    public class IceBlock : MonoBehaviour
    {
        [Header("滑りやすさ（加減速率）")]
        [Tooltip("低いほどツルツル滑ります。（通常の床は10.0。氷は1.5などが目安です）")]
        public float slipRate = 1.5f;
    }
}
