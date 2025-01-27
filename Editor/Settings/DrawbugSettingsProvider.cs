using System.Collections.Generic;
using System.IO;
using DrawBug.Editor;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Drawbug.PhysicsExtension.Editor
{
    public class DrawbugSettingsProvider : SettingsProvider
    {
        private SerializedObject _drawbugSettings;

        private DrawbugSettingsProvider(string path, SettingsScope scopes = SettingsScope.Project, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _drawbugSettings = DrawbugSettings.GetSerializedSettings();

            var header = new VisualElement
            {
                name = "Drawbug Project Settings",
                style =
                {
                    marginBottom = 8,
                    minHeight = 20,
                    paddingLeft = 11,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                }
            };
            rootElement.Add(header);

            var headerTitle = new Label("<b>Drawbug<b>")
            {
                style =
                {
                    fontSize = 18
                }
            };
            header.Add(headerTitle);
            
            var resetButton = new Button(ResetSettings)
            {
                name = "Reset",
                text = "Reset",
                style =
                {
                    fontSize = 12,
                    marginRight = 12,
                }
            };
            header.Add(resetButton);

            var shapesContainer = new VisualElement
            {
                name = "Shapes Project Settings",
                style =
                {
                    fontSize = 12,
                    paddingBottom = 12,
                    paddingLeft = 12,
                    paddingRight = 3,
                }
            };
            rootElement.Add(shapesContainer);
            var shapesTitle = new Label("<b>Shapes<b>")
            {
                style = { fontSize = 14 }
            };
            shapesContainer.Add(shapesTitle);
            
            var occludedWireOpacityField = new PropertyField();
            shapesContainer.Add(occludedWireOpacityField);
            occludedWireOpacityField.BindProperty(_drawbugSettings.FindProperty("occludedWireOpacity"));
            var occludedSolidOpacityField = new PropertyField();
            occludedSolidOpacityField.BindProperty(_drawbugSettings.FindProperty("occludedSolidOpacity"));
            shapesContainer.Add(occludedSolidOpacityField);
            
            var physicsContainer = new VisualElement
            {
                name = "Physics Project Settings",
                style =
                {
                    fontSize = 12,
                    paddingBottom = 12,
                    paddingLeft = 12,
                    paddingRight = 3,
                }
            };
            rootElement.Add(physicsContainer);
            var physicsTitle = new Label("<b>Physics<b>")
            {
                style = { fontSize = 14 }
            };
            physicsContainer.Add(physicsTitle);
            var hitColorField = new PropertyField();
            hitColorField.BindProperty(_drawbugSettings.FindProperty(nameof(DrawbugSettings.hitColor)));
            physicsContainer.Add(hitColorField);
            var noHitColorField = new PropertyField();
            noHitColorField.BindProperty(_drawbugSettings.FindProperty(nameof(DrawbugSettings.noHitColor)));
            physicsContainer.Add(noHitColorField);
            var pointColorField = new PropertyField();
            pointColorField.BindProperty(_drawbugSettings.FindProperty(nameof(DrawbugSettings.pointColor)));
            physicsContainer.Add(pointColorField);
            var enableDrawPhysicsField = new PropertyField();
            enableDrawPhysicsField.BindProperty(_drawbugSettings.FindProperty(nameof(DrawbugSettings.enableDrawPhysics)));
            physicsContainer.Add(enableDrawPhysicsField);
            
            var bottomContainer = new VisualElement
            {
                name = "Bottom Container",
                style =
                {
                    paddingBottom = 12,
                    paddingLeft = 12,
                    paddingRight = 3,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                }
            };
            rootElement.Add(bottomContainer);
            var applyButton = new Button(ApplySettings)
            {
                name = "Apply",
                text = "Apply",
                style =
                {
                    fontSize = 12,
                    marginRight = 12,
                }
            };
            bottomContainer.Add(applyButton);
            
            base.OnActivate(searchContext, rootElement);
        }

        private void ResetSettings()
        {
            var drawbugSettings = _drawbugSettings.targetObject as DrawbugSettings;
            drawbugSettings?.RestoreDefaultSettings();
            EditorUtility.SetDirty(drawbugSettings);
            AssetDatabase.SaveAssets();
            ApplySettings();
        }

        private void ApplySettings()
        {
            var enableDrawPhysics = _drawbugSettings.FindProperty(nameof(DrawbugSettings.enableDrawPhysics)).boolValue;
            if (enableDrawPhysics)
                DefineSymbols.Remove("DONT_DRAW_PHYSICS");
            else
                DefineSymbols.Add("DONT_DRAW_PHYSICS");
            
            AssetDatabase.Refresh();
        }

        // private void ApplyDefinesToAssemblyDef()
        // {
        //     const string asmdefPath = "Packages/com.mastersix.drawbug/Runtime/Drawbug.asmdef";
        //     const string defineToAdd = "DRAWBUG_REMOVE_ALL_CALLS";
        //
        //     if (!File.Exists(asmdefPath))
        //     {
        //         Debug.LogError($"The file .asmdef is not found at: {asmdefPath}");
        //         return;
        //     }
        //
        //     var asmdefJson = File.ReadAllText(asmdefPath);
        //     var asmdefObject = JObject.Parse(asmdefJson);
        //     
        //     var defineConstraints = asmdefObject["defineConstraints"] as JArray;
        //     if (defineConstraints == null)
        //     {
        //         defineConstraints = new JArray();
        //         asmdefObject["defineConstraints"] = defineConstraints;
        //     }
        //
        //     var removeFromPlayerBuild = _drawbugSettings.FindProperty(nameof(DrawbugSettings.removeFromPlayerBuild)).boolValue;
        //     if (removeFromPlayerBuild && !defineConstraints.Contains(defineToAdd))
        //     {
        //         defineConstraints.Add('!' + defineToAdd);
        //         Debug.Log($"Definition '{defineToAdd}' added.");
        //     }
        //     else if (!removeFromPlayerBuild && defineConstraints.Contains(defineToAdd))
        //     {
        //         defineConstraints.Remove('!' + defineToAdd);
        //         Debug.Log($"Definition '{defineToAdd}' removed.");
        //     }
        //
        //     var updatedAsmdefJson = JsonConvert.SerializeObject(asmdefObject, Formatting.Indented);
        //     File.WriteAllText(asmdefPath, updatedAsmdefJson);
        // }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new DrawbugSettingsProvider("Project/Drawbug")
            {
                keywords = new[] { "draw", "bug", "drawing" }
            };
        }
    }
}