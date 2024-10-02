using UnityEngine;

namespace Drawbug
{
    public class DrawbugSettings : ScriptableObject
    {
#if DRAWBUG
        internal const string DrawbugSettingsPath = "Packages/com.mastersix.drawbug/DrawbugSettings.asset";
#else
        internal const string DrawbugSettingsPath = "Assets/com.mastersix.drawbug/DrawbugSettings.asset";
#endif
        
        [Range(0, 1)] public float occludedWireOpacity = 0.05f;
        [Range(0, 1)] public float occludedSolidOpacity = 0.0f;
        
        public bool drawPhysicsEnabled = true;
        public Color hitColor = Color.green;
        public Color noHitColor = Color.red;
        public Color pointColor = Color.red;

        internal static DrawbugSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<DrawbugSettings>(DrawbugSettingsPath);
            if (!settings)
            {
                settings = CreateInstance<DrawbugSettings>();
                settings.occludedWireOpacity = 0.05f;
                settings.occludedSolidOpacity = 0.0f;
                settings.drawPhysicsEnabled = true;
                settings.hitColor = Color.green;
                settings.noHitColor = Color.red;
                settings.pointColor = Color.red;
                AssetDatabase.CreateAsset(settings, DrawbugSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
        
#if UNITY_EDITOR
        internal static UnityEditor.SerializedObject GetSerializedSettings()
        {
            return new UnityEditor.SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}