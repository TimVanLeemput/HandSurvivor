/*
 * Copyright (c) [2024] [Tim Van Leemput]
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * - Make sure to include this script in your Unity project and use it in accordance with the MIT License terms.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// TagManager is a Unity Editor window tool that provides advanced functionality for managing tags.
/// 
/// Features:
/// - Search, filter, and apply tags to GameObjects directly within the Unity Editor.
/// - Create and add new tags seamlessly with intuitive UI controls.
/// - Access the tool via Unity Editor's Tools menu under 'Tag Manager'.
/// - Ideal for developers and designers needing efficient tag management for large-scale projects.
/// </summary>
namespace TagManagerByTVL
{
    public class TagManager : EditorWindow
    {
        private string newTagName = "";
        private Vector2 scrollPositionTagList;
        private Vector2 scrollPositionGameObjectsWithFilteredTag;

        private string searchQuery = "";
        private List<string> filteredTags = new List<string>();
        public List<GameObject> allGameObjectsWithSpecificTag = null;
        private bool shouldDisplayGameObjectsWithSpecificTagField = false;

        private float dividerPosition = 0.5f; // Initial position of the divider (0.5 = center)
        private bool isDraggingDivider = false;
        private float dividerHeight = 5f; // Height of the divider bar

        private string tagToFind = "";
        public bool shouldOpenOptionsField = false;
        public bool useColors = true;

        GameObject[] latestGameObjectSelection = null;

        public event Action<string> OnFindGameObjectInSceneButtonPressed = null;
        public event Action<GameObject[]> OnLatestGameObjectSelectionAdded = null;
        private enum TagColor
        {
            LIGHT_RED,
            LIGHT_GREEN,
            LIGHT_BLUE,
            LIGHT_YELLOW,
            LIGHT_CYAN,
            LIGHT_MAGENTA,
            LIGHT_ORANGE,
            LIGHT_LIME,
            LIGHT_PURPLE,
            LIGHT_TEAL
        }

        private Color[] tagColors = {
        new Color(0.8f, 0.6f, 0.6f),   // LIGHT_RED
        new Color(0.6f, 0.8f, 0.6f),   // LIGHT_GREEN
        new Color(0.6f, 0.6f, 0.8f),   // LIGHT_BLUE
        new Color(0.8f, 0.8f, 0.6f),   // LIGHT_YELLOW
        new Color(0.6f, 0.8f, 0.8f),   // LIGHT_CYAN
        new Color(0.8f, 0.6f, 0.8f),   // LIGHT_MAGENTA
        new Color(0.9f, 0.7f, 0.5f),   // LIGHT_ORANGE
        new Color(0.7f, 0.9f, 0.5f),   // LIGHT_LIME
        new Color(0.7f, 0.5f, 0.9f),   // LIGHT_PURPLE
        new Color(0.5f, 0.9f, 0.7f)    // LIGHT_TEAL
    };

        [MenuItem("Tools/Tag Manager Tool")]
        public static void ShowWindow()
        {
            GetWindow<TagManager>("Tag Manager");
        }

        private void OnEnable()
        {
            shouldDisplayGameObjectsWithSpecificTagField = false;
            OnFindGameObjectInSceneButtonPressed += SetTagForGameObjectToFind;
            OnLatestGameObjectSelectionAdded += SetLatestGameObjectSelection;
        }

        private void SetLatestGameObjectSelection(GameObject[] _goSelection)
        {
            latestGameObjectSelection = _goSelection;
        }

        private void SetTagForGameObjectToFind(string _tag)
        {
            tagToFind = _tag;
        }

        private void OnGUI()
        {
            SearchTagField();
            DrawResizableScrollView();

        }
        /// <summary>
        /// Split view when showing the List of GameObjects 
        /// with a matching tag
        /// </summary>
        private void DrawResizableScrollView()
        {
            EditorGUILayout.BeginVertical();

            // Top ScrollView
            EditorGUILayout.BeginVertical(GUILayout.Height(position.height * dividerPosition));
            TopScrollViewField();
            EditorGUILayout.EndVertical();

            // Divider
            DividerMouseDragField();

            // Bottom ScrollView
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            BottomScrollViewField();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        private void BottomScrollViewField()
        {
            AllGameObjectsWithTagListField();
            HideGameObjectWithFilteredTagListButtonField();
            CreateNewTagField();
        }

        private void OpenOptionsToggleField()
        {
            if (shouldOpenOptionsField = EditorGUILayout.BeginToggleGroup("Options", shouldOpenOptionsField))
            {

            }
            EditorGUILayout.EndToggleGroup();
        }
        private void UseColorsButtonField()
        {
            //GUILayout.FlexibleSpace();
            if (!shouldOpenOptionsField) return;
            if (GUILayout.Button("Colors", GUILayout.Width(50)))
            {
                useColors = !useColors;
            }
            //GUILayout.FlexibleSpace();
        }
        private void TopScrollViewField()
        {
            TagsScrollViewField();
        }

        private void DividerMouseDragField()
        {
            Rect _resizeRect = GUILayoutUtility.GetRect(position.width, dividerHeight);
            EditorGUI.DrawRect(_resizeRect, Color.black * 0.5f);

            // Handle divider dragging
            EditorGUIUtility.AddCursorRect(_resizeRect, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && _resizeRect.Contains(Event.current.mousePosition))
            {
                isDraggingDivider = true;
            }
            if (isDraggingDivider)
            {
                if (position.height <= 450)
                {
                    dividerPosition = Mathf.Clamp(Event.current.mousePosition.y / position.height - 0.15f, 0.1f, 0.9f); // 0.15f => Dynamic Offset for window resizing
                }
                else
                {
                    dividerPosition = Mathf.Clamp(Event.current.mousePosition.y / position.height - 0.05f, 0.1f, 0.9f); // 0.05f => Dynamic Offset for window resizing
                }
                Repaint();
            }
            if (Event.current.type == EventType.MouseUp)
            {
                isDraggingDivider = false;
            }
        }

        private void UntaggedButtonField()
        {
            if (GUILayout.Button("Untagged", GUILayout.Width(80)))
            {
                ChangeTagOfSelectedObject("Untagged");
            }
        }

        private void SearchTagField()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search tag:", EditorStyles.boldLabel, GUILayout.Width(80));
            searchQuery = EditorGUILayout.TextField(searchQuery);
            GUILayout.FlexibleSpace();

            UntaggedButtonField();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void TagsScrollViewField()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            scrollPositionTagList = EditorGUILayout.BeginScrollView(scrollPositionTagList);

            string[] _tags = UnityEditorInternal.InternalEditorUtility.tags;

            // Filter tags based on search query
            filteredTags = FilterTags(_tags, searchQuery);
            filteredTags.Sort();

            foreach (string _tag in filteredTags)
            {
                DisplayTagEntry(_tag);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private List<string> FilterTags(string[] _tags, string _searchQuery)
        {
            List<string> _filtered = new List<string>();

            foreach (string _tag in _tags)
            {
                if (string.IsNullOrEmpty(_searchQuery) || _tag.ToLower().Contains(_searchQuery.ToLower()))
                {
                    _filtered.Add(_tag);
                }
            }
            return _filtered;
        }
        /// <summary>
        /// This method is called to display one tag
        /// as a Button field
        /// </summary>
        /// <param name="_tag"></param>
        private void DisplayTagEntry(string _tag)
        {
            EditorGUILayout.BeginHorizontal();

            // Display tag name with custom color as button
            GUIStyle _buttonStyle = new GUIStyle(GUI.skin.button);
            //int colorIndex = System.Array.IndexOf(UnityEditorInternal.InternalEditorUtility.tags, tag) % tagColors.Length;  // If we want to filter colors by actual TagManager indexation
            if (useColors)
            {
                int colorIndex = filteredTags.IndexOf(_tag) % tagColors.Length; // Filtering colors per filteredTag list
                _buttonStyle.normal.textColor = tagColors[colorIndex];
                _buttonStyle.hover.textColor = tagColors[colorIndex]; // Change text color on hover (optional)
                _buttonStyle.active.textColor = tagColors[colorIndex]; // Change text color on click (optional)
            }

            // Turn into if statement to add a functionality on click
            GUILayout.Button(_tag, _buttonStyle, GUILayout.ExpandWidth(true));

            //Find in scene button
            if (GUILayout.Button("Find", GUILayout.Width(60)))
            {
                OnFindGameObjectInSceneButtonPressed?.Invoke(_tag);
                allGameObjectsWithSpecificTag = GetListOfGameObjectsWithSpecificTag(tagToFind);
                if (allGameObjectsWithSpecificTag.Count > 0) shouldDisplayGameObjectsWithSpecificTagField = true;
                else SetShouldDisplayGameObjectsWithSpecificTagField(false);
            }
            // Apply button
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                ChangeTagOfSelectedObject(_tag);
            }

            // Copy button
            if (GUILayout.Button("Copy", GUILayout.Width(50)))
            {
                CopyToClipboard(_tag);
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("Confirm tag delete", $"Are you sure you want to delete the '{_tag}' tag?", "Yes", "No"))
                    RemoveTagFromTagList(_tag);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RemoveTagFromTagList(string _tagToRemove)
        {
            SerializedObject _tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty _tagsProp = _tagManager.FindProperty("tags");

            if (TagExists(_tagToRemove))
            {
                for (int i = 0; i < _tagsProp.arraySize; i++)
                {
                    SerializedProperty tag = _tagsProp.GetArrayElementAtIndex(i);
                    if (tag.stringValue == _tagToRemove)
                    {
                        _tagsProp.DeleteArrayElementAtIndex(i);
                        _tagManager.ApplyModifiedProperties();
                        Debug.Log("Tag '" + _tagToRemove + "' deleted.");
                        foreach (var _go in Selection.gameObjects)
                        {
                            EditorUtility.SetDirty(_go);
                        }
                        return; // Early return if tag found
                    }
                }
            }
            else
            {
                Debug.LogWarning("Tag '" + _tagToRemove + "' does not exist.");
            }
        }

        private void AllGameObjectsWithTagListField()
        {
            if (!shouldDisplayGameObjectsWithSpecificTagField) return;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            scrollPositionGameObjectsWithFilteredTag = EditorGUILayout.BeginScrollView(scrollPositionGameObjectsWithFilteredTag);

            if (string.IsNullOrEmpty(tagToFind) || !TagExists(tagToFind))
            {
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }

            allGameObjectsWithSpecificTag = GetListOfGameObjectsWithSpecificTag(tagToFind);

            if (allGameObjectsWithSpecificTag.Count > 0)
            {
                foreach (GameObject _go in allGameObjectsWithSpecificTag)
                {
                    EditorGUILayout.ObjectField(_go.name, _go, typeof(GameObject), true);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private static List<GameObject> GetListOfGameObjectsWithSpecificTag(string _tag)
        {
            
            var _prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (_prefabStage != null)
            {
                GameObject _prefabRoot = _prefabStage.prefabContentsRoot;

                return _prefabRoot.GetComponentsInChildren<Transform>(true)
                                 .Where(t => t.gameObject.CompareTag(_tag))
                                 .Select(t => t.gameObject)
                                 .ToList();
            }
            else
            {
                List<GameObject> _allGOWithSpecificTag = GameObject.FindGameObjectsWithTag(_tag).ToList();
                return _allGOWithSpecificTag;
            }
        }

        private void HideGameObjectWithFilteredTagListButtonField()
        {
            if (shouldDisplayGameObjectsWithSpecificTagField)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Hide List", GUILayout.Width(65)))
                {
                    SetShouldDisplayGameObjectsWithSpecificTagField(false);
                }

                SelectAllGameObjectsInFilteredTagListButtonField();
                UnSelectAllGameObjectsInFilteredTagListButtonField();
                GUILayout.EndHorizontal();
            }
        }
        /// <summary>
        /// This button will select all GameObjects found
        /// after clicking [Find] Button.
        /// CTRL + ButtonClick will add to
        /// the current selection
        /// </summary>
        private void SelectAllGameObjectsInFilteredTagListButtonField()
        {
            bool _selectAllButton = GUILayout.Button("Select All", GUILayout.Width(65));
            bool _isCtrlPressed = Event.current.control;
            if (_selectAllButton)
            {
                if (_isCtrlPressed)
                {
                    GameObject[] _currentlySelectedGOs = allGameObjectsWithSpecificTag.ToArray();
                    GameObject[] _newSelection = Selection.gameObjects;
                    OnLatestGameObjectSelectionAdded?.Invoke(_newSelection);

                    _newSelection = _newSelection.Concat(_currentlySelectedGOs).ToArray();

                    Selection.objects = _newSelection;
                }
                else
                {
                    Selection.objects = allGameObjectsWithSpecificTag == null ? Selection.objects = null : allGameObjectsWithSpecificTag.ToArray();
                }
            }
        }
        /// <summary>
        /// This button will unselect all currently selected GameObjects
        /// On CTRL + ButtonClick - Only remove latest additional selection
        /// </summary>
        private void UnSelectAllGameObjectsInFilteredTagListButtonField()
        {
            bool _isCtrlPressed = Event.current.control;
            bool _unselectAllButton = GUILayout.Button("Unselect All", GUILayout.Width(80));
            if (_unselectAllButton)
            {
                if (_isCtrlPressed)
                {
                    GameObject[] _currentlySelectedGOs = allGameObjectsWithSpecificTag.ToArray();
                    Selection.objects = latestGameObjectSelection == null ? Selection.objects = null : latestGameObjectSelection.ToArray();
                }
                else
                {
                    Selection.objects = null;
                    latestGameObjectSelection = null;
                }
            }
        }

        private void SetShouldDisplayGameObjectsWithSpecificTagField(bool _value)
        {
            shouldDisplayGameObjectsWithSpecificTagField = _value;
        }

        private void CreateNewTagField()
        {
            GUILayout.Space(10);
            GUILayout.Label("Create New Tag", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            newTagName = EditorGUILayout.TextField(newTagName);

            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && !string.IsNullOrEmpty(newTagName))
            {
                AddTag(newTagName);
                newTagName = "";
                Repaint();
            }

            GUILayout.BeginHorizontal();
            OpenOptionsToggleField();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Tag") && !string.IsNullOrEmpty(newTagName))
            {
                AddTag(newTagName);
                newTagName = "";
            }

            GUILayout.EndHorizontal();
            OptionListField();
        }

        private void OptionListField()
        {
            UseColorsButtonField();
        }

        /// <summary>
        /// This method creates a new tag and
        /// adds it to the AssetDataBase
        /// </summary>
        /// <param name="_tag"></param>
        private void AddTag(string _tag)
        {
            if (!TagExists(_tag))
            {
                SerializedObject _tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty _tagsProp = _tagManager.FindProperty("tags");

                int _arraySize = _tagsProp.arraySize;
                _tagsProp.InsertArrayElementAtIndex(_arraySize);
                SerializedProperty _newTag = _tagsProp.GetArrayElementAtIndex(_arraySize);
                _newTag.stringValue = _tag;
                _tagManager.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("Tag already exists.");
            }
        }
        /// <summary>
        /// This method will copy the string of
        /// the tag reference you are currently clicking
        /// in the EditorWindow
        /// </summary>
        /// <param name="_text"></param>
        private void CopyToClipboard(string _text)
        {
            TextEditor _te = new TextEditor { text = _text };
            _te.SelectAll();
            _te.Copy();
        }
        /// <summary>
        /// This method sets the tag of the 
        /// currently selected GameObject
        /// </summary>
        /// <param name="_tag"> Name of the new tag</param>
        private void ChangeTagOfSelectedObject(string _tag)
        {
            if (Selection.activeGameObject != null)
            {
                foreach (GameObject _go in Selection.objects)
                {
                    Undo.RecordObject(_go, "Change Tag");
                    _go.tag = _tag;
                    EditorUtility.SetDirty(_go);
                }
            }
            else
            {
                Debug.LogWarning("No GameObject selected.");
            }
        }

        /// <summary>
        /// This method checks if a tag already exists in this Unity Project
        /// </summary>
        /// <param name="_tag"></param>
        /// <returns></returns>
        private bool TagExists(string _tag)
        {
            return UnityEditorInternal.InternalEditorUtility.tags.Contains(_tag);
        }
    }//
}
