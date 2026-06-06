using UnityEngine;

namespace Assets.PixelFantasy.PixelBuildings.Scripts
{
    [ExecuteInEditMode]
    public class PixelGridSnap : MonoBehaviour
    {
        #if UNITY_EDITOR

        public void Update()
        {
            const float step = 1 / 16f;

            foreach (Transform child in transform)
            {
                var p = child.position;

                p.x = Mathf.RoundToInt(p.x / step) * step;
                p.y = Mathf.RoundToInt(p.y / step) * step;

                child.position = p;
            }
        }

        #endif
    }
}