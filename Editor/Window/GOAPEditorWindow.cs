using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.GOAP.Editor
{
    public class GOAPEditorWindow : EditorWindow
    {
        private static readonly Dictionary<int, GOAPEditorWindow> cache = new();
        private UnityEngine.Object Key { get; set; }
        private GOAPView graphView;
        private SnapshotView snapshotView;
        private bool enableSnapshot;
        public static void ShowEditorWindow(IGOAPSet set)
        {
            var key = set.Object.GetHashCode();
            if (cache.ContainsKey(key))
            {
                cache[key].Focus();
                return;
            }
            var window = CreateInstance<GOAPEditorWindow>();
            window.titleContent = new GUIContent($"GOAP Editor ({set.Object.name})");
            window.Show();
            window.Focus();
            window.Key = set.Object;
            cache[key] = window;
            window.StructGraphView(set);
        }
        private void StructGraphView(IGOAPSet set)
        {
            rootVisualElement.Clear();
            graphView = new GOAPView(this, set);
            graphView.Restore();
            rootVisualElement.Add(CreateToolBar(graphView));
            rootVisualElement.Add(graphView);
            if (set is IPlanner planner)
            {
                planner.OnReload += OnPlannerReload;
            }
        }
        private void OnPlannerReload(IPlanner planner)
        {
            planner.OnReload -= OnPlannerReload;
            Reload();
        }
        private VisualElement CreateToolBar(GOAPView graphView)
        {
            return new IMGUIContainer(
                () =>
                {
                    GUILayout.BeginHorizontal(EditorStyles.toolbar);

                    GUI.enabled = !Application.isPlaying;
                    if (GUILayout.Button($"Save", EditorStyles.toolbarButton))
                    {
                        graphView.Save();
                        ShowNotification(new GUIContent("Update Succeed !"));
                    }
                    GUI.enabled = true;
                    GUILayout.FlexibleSpace();
                    GUI.enabled = graphView.Set is IPlanner planner;
                    bool newValue = GUILayout.Toggle(enableSnapshot, "Snapshot", EditorStyles.toolbarButton);
                    if (newValue != enableSnapshot)
                    {
                        enableSnapshot = newValue;
                        if (!enableSnapshot && snapshotView != null)
                        {
                            rootVisualElement.Remove(snapshotView);
                            snapshotView = null;
                        }
                        else if (enableSnapshot && snapshotView == null)
                            rootVisualElement.Add(snapshotView = new SnapshotView(graphView.Set as IPlanner));
                    }
                    GUI.enabled = !Application.isPlaying;
                    if (GUILayout.Button($"Save To Json", EditorStyles.toolbarButton))
                    {
                        string path = EditorUtility.SaveFilePanel("Select json file save path", Application.dataPath, graphView.Set.Object.name, "json");
                        if (!string.IsNullOrEmpty(path))
                        {
                            var template = CreateInstance<GOAPSet>();
                            template.Behaviors.AddRange(graphView.Set.Behaviors);
                            var serializedData = JsonUtility.ToJson(template);
                            FileInfo info = new(path);
                            File.WriteAllText(path, serializedData);
                            ShowNotification(new GUIContent("Save to json file succeed !"));
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        GUIUtility.ExitGUI();
                    }
                    GUI.enabled = true;
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
            if (Key != null)
            {
                if (Key is GameObject) StructGraphView((Key as GameObject).GetComponent<IGOAPSet>());
                else StructGraphView(Key as IGOAPSet);
                Repaint();
            }
        }
        private void OnDestroy()
        {
            int code = Key.GetHashCode();
            if (Key != null && cache.ContainsKey(code))
            {
                cache.Remove(code);
            }
        }
    }
}
