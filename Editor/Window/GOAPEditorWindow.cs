using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class GOAPEditorWindow : EditorWindow
    {
        private static readonly Dictionary<int,GOAPEditorWindow> cache = new Dictionary<int, GOAPEditorWindow>();
        private UnityEngine.Object key { get; set; }
        private GOAPView graphView;
        public static void ShowEditorWindow(IGOAPSet set)
        {
            var key = set._Object.GetHashCode();
            if (cache.ContainsKey(key))
            {
                cache[key].Focus();
                return;
            }
            var window = ScriptableObject.CreateInstance<GOAPEditorWindow>();
            window.titleContent = new GUIContent($"GOAP Editor ({set._Object.name})");
            window.Show();
            window.Focus();
            window.key = set._Object;
            cache[key] = window;
            window.StructGraphView(set);
        }
        private void StructGraphView(IGOAPSet set)
        {
            rootVisualElement.Clear();
            graphView=new GOAPView(this,set);
            graphView.Restore();
            rootVisualElement.Add(CreateToolBar(graphView));
            rootVisualElement.Add(graphView); 
        }
        private VisualElement CreateToolBar(GOAPView graphView)
        {
            return new IMGUIContainer(
                () =>
                {
                    GUILayout.BeginHorizontal(EditorStyles.toolbar);

                    GUI.enabled=!Application.isPlaying;
                    if (GUILayout.Button($"Save", EditorStyles.toolbarButton))
                    {
                        var guiContent = new GUIContent();
                        graphView.Save();
                        guiContent.text = $"Update Succeed !";
                        this.ShowNotification(guiContent);
                    }
                    GUI.enabled=true;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            );
        }
        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    Reload();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    Reload();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playModeStateChange), playModeStateChange, null);
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Reload();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        private void Reload()
        {
            if (key != null)
            {
                if(key is GameObject)StructGraphView((key as GameObject).GetComponent<IGOAPSet>());
                else StructGraphView((key as IGOAPSet));
                Repaint();
            }
        }
        private void OnDestroy()
        {
            int code=key.GetHashCode();
            if (key != null && cache.ContainsKey(code))
            {
                cache.Remove(code);
            }
        }
    }
}
