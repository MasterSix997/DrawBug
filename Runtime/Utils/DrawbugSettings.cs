using UnityEditor;
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
        
        [Range(0, 1)] public float occludedWireOpacity;
        [Range(0, 1)] public float occludedSolidOpacity;
        
        public bool drawPhysicsEnabled;
        public Color hitColor;
        public Color noHitColor;
        public Color pointColor;

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
 
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}