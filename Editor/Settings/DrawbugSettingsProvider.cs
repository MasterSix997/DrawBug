using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
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
            hitColorField.BindProperty(_drawbugSettings.FindProperty("hitColor"));
            physicsContainer.Add(hitColorField);
            var noHitColorField = new PropertyField();
            noHitColorField.BindProperty(_drawbugSettings.FindProperty("noHitColor"));
            physicsContainer.Add(noHitColorField);
            var pointColorField = new PropertyField();
            pointColorField.BindProperty(_drawbugSettings.FindProperty("pointColor"));
            physicsContainer.Add(pointColorField);

            // rootElement.Add(shapesTitleElement);
            // rootElement.Add(occludedWireOpacityField);
            // rootElement.Add(occludedSolidOpacityField);
            // rootElement.Add(physicsTitleElement);
            // rootElement.Add(hitColorField);
            // rootElement.Add(noHitColorField);
            // rootElement.Add(pointColorField);
            
            base.OnActivate(searchContext, rootElement);
        }

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