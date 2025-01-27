using UnityEngine;

namespace Drawbug
{
    public class DrawbugSettings : ScriptableObject
    {
        internal const string DrawbugSettingsPath = "Packages/com.mastersix.drawbug/Resources/DrawbugSettings.asset";
        internal const string DrawbugSettingsName = "DrawbugSettings";
        
        [Range(0, 1)] public float occludedWireOpacity = 0.05f;
        [Range(0, 1)] public float occludedSolidOpacity = 0.0f;
        
        public Color hitColor = Color.green;
        public Color noHitColor = Color.red;
        public Color pointColor = Color.red;
        public bool enableDrawPhysics = true;

        internal static DrawbugSettings CreateDefaultSettings()
        {
            Debug.LogWarning("Drawbug Settings not find, creating a new");
            var settings = CreateInstance<DrawbugSettings>();
            settings.RestoreDefaultSettings();
            return settings;
        }

        internal static DrawbugSettings LoadSettings()
        {
            var settings = Resources.Load<DrawbugSettings>(DrawbugSettingsName);
            return settings ?? CreateDefaultSettings();
        }

        internal void RestoreDefaultSettings()
        {
            occludedWireOpacity = 0.05f;
            occludedSolidOpacity = 0.0f;
            hitColor = Color.green;
            noHitColor = Color.red;
            pointColor = Color.red;
            enableDrawPhysics = true;
        }

#if UNITY_EDITOR
        internal static DrawbugSettings GetOrCreateSettings()
        {
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<DrawbugSettings>(DrawbugSettingsPath);
            if (!settings)
            {
                settings = CreateInstance<DrawbugSettings>();
                UnityEditor.AssetDatabase.CreateAsset(settings, DrawbugSettingsPath);
                UnityEditor.AssetDatabase.SaveAssets();
            }
            return settings;
        }
        
        internal static UnityEditor.SerializedObject GetSerializedSettings()
        {
            return new UnityEditor.SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}