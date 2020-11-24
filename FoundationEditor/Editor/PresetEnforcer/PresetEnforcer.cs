using TMPro;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FoundationEditor.Enforcers.Editor.PresetEnforcer
{
    public class PresetEnforcer
    {

        private const string PresetsFolder = "Presets/";
        private const string ButtonImagePresetFileName = "ButtonImagePreset";
        
        #region MenuItemOverrides

        [MenuItem("GameObject/UI/Button")]
        public static void CreateButton()
        {
            DefaultControls.Resources uiResources = new DefaultControls.Resources {standard = null};
            GameObject uiButton = DefaultControls.CreateButton(uiResources);
            Button button = uiButton.GetComponent<Button>();

            ApplyDefaultPreset(button);
            EditorApplication.delayCall += () =>
            {
                //If GetComponent<Image>() is done without delay it returns null 
                button.image = uiButton.GetComponent<Image>();
                Preset buttonImage = GetButtonImagePreset();
                buttonImage.ApplyTo(button.image);
                button.image.color = button.colors.normalColor;

                Text buttonText = uiButton.GetComponentInChildren<Text>();
                ApplyDefaultPreset(buttonText);
            };
            
            ParentObjectUnderCanvas(uiButton);
        }

        [MenuItem("GameObject/UI/Text - TextMeshPro")]
        public static void CreateTextMeshProText()
        {
            GameObject uiTextObject = new GameObject("TextMeshPro UGui");
            TextMeshProUGUI uiText = uiTextObject.AddComponent<TextMeshProUGUI>();

            ApplyDefaultPreset(uiText);
            ParentObjectUnderCanvas(uiTextObject);
        }

        [MenuItem("GameObject/UI/Text")]
        public static void CreateText()
        {
            GameObject uiTextObject = new GameObject("Text");
            Text uiText = uiTextObject.AddComponent<Text>();

            ApplyDefaultPreset(uiText);
            ParentObjectUnderCanvas(uiTextObject);
        }
        
        #endregion

        #region Utility methods

        private static Preset GetButtonImagePreset()
        {
            return Resources.Load<Preset>(PresetsFolder + ButtonImagePresetFileName);
        }
        
        private static void ParentObjectUnderCanvas(GameObject createdObject)
        {
            Transform current;
            if (Selection.activeGameObject)
            {
                current = Selection.activeGameObject.transform;

                while (current)
                {
                    if (current.GetComponent<Canvas>())
                    {
                        createdObject.transform.SetParent(current);
                        return;
                    }
                    current = current.parent;
                }
            }

            current = GameObject.FindObjectOfType<Canvas>()?.transform;
            if (current) { createdObject.transform.SetParent(current); }
            else { createdObject.transform.SetParent(CreateCanvas().gameObject.transform); }
        }

        private static Canvas CreateCanvas()
        {
            GameObject newCanvas = new GameObject("Canvas");
            Canvas canvas = newCanvas.AddComponent<Canvas>();
            ApplyDefaultPreset(canvas);
            ApplyDefaultPreset(newCanvas.AddComponent<CanvasScaler>());
            ApplyDefaultPreset(newCanvas.AddComponent<GraphicRaycaster>());

            if (!GameObject.FindObjectOfType<EventSystem>())
            {
                GameObject eventSystem = new GameObject("EventSystem");
                ApplyDefaultPreset(eventSystem.AddComponent<EventSystem>());
                ApplyDefaultPreset(eventSystem.AddComponent<StandaloneInputModule>());
            }

            return canvas;
        }

        private static void ApplyDefaultPreset(Object target)
        {
            Preset[] presets = Preset.GetDefaultPresetsForObject(target);
            if (presets.Length > 0) { presets[0].ApplyTo(target); }
        }

        #endregion
    }
}
