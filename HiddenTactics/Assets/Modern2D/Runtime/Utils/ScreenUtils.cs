
#if UNITY_EDITOR
using UnityEditor;
#endif


using UnityEngine;

namespace LeafUtils { 
    public static class ScreenUtils 
    {
        public static Vector2Int ScreenResolution()
        {
#if UNITY_EDITOR

            string[] res = UnityStats.screenRes.Split('x');
            Vector2Int screenResolution = new Vector2Int(int.Parse(res[0]), int.Parse(res[1]));
#endif
            //if (screenResolution.x == 0 || screenResolution.y == 0) screenResolution = new Vector2Int(Screen.width, Screen.height);
            //if (screenResolution.x == 0 || screenResolution.y == 0) screenResolution = new Vector2Int(1920, 1080);
            return new Vector2Int(1920, 1080);
        }
    }

}