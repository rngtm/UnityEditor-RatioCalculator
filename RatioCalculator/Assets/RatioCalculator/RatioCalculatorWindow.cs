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
        static readonly GUILayoutOption[] treeViewLayoutOptions = new GUILayoutOption[]
        {
            GUILayout.MinHeight(120f),
            GUILayout.ExpandHeight(true), // 広げる
        };

        private bool isOpen_Multiplication = true;
        private bool isOpen_Magnification = true;

        private RatioTreeView_Multiplication treeView_Multiplication = null; // TreeView(乗算)
        private TreeViewState treeViewState_Multiplication = null; // TreeViewの状態

        private RatioTreeView_Magnification treeView_Magnification = null; // TreeView(乗算)
        private TreeViewState treeViewState_Magnification = null; // TreeViewの状態

        [SerializeField]
        private List<RatioTreeView_Multiplication.TreeElement> multiplyDataList = new List<RatioTreeView_Multiplication.TreeElement>  {
            new RatioTreeView_Multiplication.TreeElement { Value1 = 100f , Value2 = 0.3f },
        };

        [SerializeField]
        private List<RatioTreeView_Magnification.TreeElement> magnificationDataList = new List<RatioTreeView_Magnification.TreeElement>  {
            new RatioTreeView_Magnification.TreeElement { Value1 = 100f , Value2 = 160f , Value3 = 1.0f },
            new RatioTreeView_Magnification.TreeElement { Value1 = 100f , Value2 = 160f , Value3 = 1.5f },
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
            posisiton.height = 309f;
            posisiton.position = new Vector2(80f, 120f);
            window.position = posisiton;
        }

        /// <summary>
        /// ウィンドウの描画
        /// </summary>
        private void OnGUI()
        {
            if (treeView_Multiplication == null)
            {
                UpdateTreeView();
            }

            var defaultColor = GUI.backgroundColor;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    //isOpen_Multiplication = GUILayout.Label("乗算");
                    isOpen_Multiplication = EditorGUILayout.Foldout(isOpen_Multiplication, "乗算");

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(32f)))
                    {
                        treeView_Multiplication.AddItem();
                    }
                }

                if (isOpen_Multiplication)
                {
                    treeView_Multiplication?.OnGUI(EditorGUILayout.GetControlRect(false, treeViewLayoutOptions));
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    //GUILayout.Label("比率計算");
                    isOpen_Magnification = EditorGUILayout.Foldout(isOpen_Magnification, "比率");

                    if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(32f)))
                    {
                        treeView_Magnification?.AddItem();
                    }
                }

                if (isOpen_Magnification)
                {
                    treeView_Magnification?.OnGUI(EditorGUILayout.GetControlRect(false, treeViewLayoutOptions));
                }
            }
            EditorGUILayout.EndVertical();

            //EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = defaultColor;
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
            treeViewState_Multiplication = treeViewState_Multiplication ?? new TreeViewState();
            treeView_Multiplication = treeView_Multiplication ?? new RatioTreeView_Multiplication(treeViewState_Multiplication);
            treeView_Multiplication.Setup(multiplyDataList);

            treeViewState_Magnification = treeViewState_Magnification ?? new TreeViewState();
            treeView_Magnification = treeView_Magnification ?? new RatioTreeView_Magnification(treeViewState_Magnification);
            treeView_Magnification.Setup(magnificationDataList);
        }
    }
}