///------------------------
/// Ratio Calculator
/// @ 2019 RNGTM
///------------------------
namespace RatioCalculator
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    /// <summary>
    /// 比率表示ウィンドウ
    /// </summary>
    public class RatioCalculatorWindow : EditorWindow
    {
        private RatioTreeView treeView = null; // TreeView
        private TreeViewState treeViewState = null; // TreeViewの状態
        private List<RatioTreeElement> dataList = new List<RatioTreeElement>  {
            new RatioTreeElement()
        };

        /// <summary>
        /// ウィンドウを開く
        /// </summary>
        [MenuItem("Tools/Ratio Calculator")]
        private static void OpenWindow()
        {
            var window = GetWindow<RatioCalculatorWindow>();
            window.titleContent = new GUIContent("Ratio Calculator");
            window.UpdateTreeView();

            var posisiton = window.position;
            posisiton.width = 500f;
            posisiton.position = new Vector2(80f, 120f);
            window.position = posisiton;
        }

        /// <summary>
        /// ウィンドウの描画
        /// </summary>
        private void OnGUI()
        {
            if (treeView == null)
            {
                UpdateTreeView();
            }

            treeView?.OnGUI(EditorGUILayout.GetControlRect(false, GUILayout.ExpandHeight(true)));
        }

        /// <summary>
        /// ウィンドウにフォーカスを乗せたときに呼ばれる
        /// </summary>
        private void OnFocus()
        {
            UpdateTreeView();
        }

        /// <summary>
        /// TreeViewの更新
        /// </summary>
        private void UpdateTreeView()
        {
            treeViewState = treeViewState ?? new TreeViewState();
            treeView = treeView ?? new RatioTreeView(treeViewState);
            treeView.Setup(dataList);
        }
    }
}