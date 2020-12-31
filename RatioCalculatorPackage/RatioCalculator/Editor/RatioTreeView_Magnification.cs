///------------------------
/// Ratio Calculator
/// @ 2019 RNGTM
///------------------------
namespace RatioCalculator
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    /// <summary>
    /// 二つの数値を指定して倍率を求めるTreeView
    /// </summary>
    public class RatioTreeView_Magnification : TreeView, IRatioTreeView
    {
        private List<TreeElement> baseElements = null;

        private const float nameFieldWidth = MyStyle.NameFieldWidth;

        // value field
        private const float inputValueFieldWidth = 145f; // 入力 比率
        private const float outputValueFieldWidth = 148f; // 結果 比率
        private const float valueFieldSpace = 5f;
        private const float valueFieldMarginLeft = 2f;
        private const float valueFieldMarginRight = 1f;

        private const float rateFieldWidth = 60f;
        private const float ratePopupWidth = 75f;
        private const float buttonWidth = MyStyle.ButtonWidth;

        public enum ETemplateType
        {
            Custom, // 自分で入力
            Gold, // 黄金比
            GoldReciprocal, // 黄金比(逆数)
            Silver, // 白銀比
            SilverReciprocal, // 白銀比の逆数
            Bronze, // 青銅比
            BronzeReciprocal, // 青銅比(逆数)
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RatioTreeView_Magnification(TreeViewState state)
            : base(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column() {
                    headerContent = new GUIContent("備考"),
                    width = nameFieldWidth,
                    autoResize = false,
                },
                new MultiColumnHeaderState.Column() {
                    headerContent = new GUIContent("入力(A:B)"),
                    width = inputValueFieldWidth,
                    autoResize = false,
                },
                new MultiColumnHeaderState.Column() {
                    headerContent = new GUIContent("結果(C:D)"),
                    width = outputValueFieldWidth,
                    autoResize = false,
                },
                new MultiColumnHeaderState.Column() {
                    headerContent = new GUIContent(""),
                    width = buttonWidth * 2f,
                    minWidth = buttonWidth * 2f,
                    maxWidth = buttonWidth * 2f,
                    autoResize = false,
                },
            })))
        {
            showAlternatingRowBackgrounds = true; // 背景のシマシマを表示
            showBorder = true; // 境界線を表示　
        }

        /// <summary>
        /// TreeViewの列の描画
        /// </summary>
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as TreeViewItem;
            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var rect = args.GetCellRect(visibleColumnIndex);
                rect.y += 1f;
                rect.height -= 2f;

                var columnIndex = args.GetColumn(visibleColumnIndex);
                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleCenter;

                var element = baseElements[args.item.id];

                bool changeValue1 = false;
                bool changeValue2 = false;
                bool changeValue3 = false;
                bool changeValue4 = false;
                switch (columnIndex)
                {
                    case 0: // 備考
                        rect.x += 2f;
                        rect.width -= 4f;
                        element.Name = EditorGUI.TextField(rect, element.Name);
                        break;
                    case 1: // 数値1:数値2
                        rect.width -= valueFieldSpace + valueFieldMarginLeft + valueFieldMarginRight;
                        rect.width /= 2f;

                        EditorGUI.BeginChangeCheck();
                        rect.x += valueFieldMarginLeft;
                        element.Value1 = EditorGUI.FloatField(rect, element.Value1);
                        changeValue1 = EditorGUI.EndChangeCheck();

                        rect.x += rect.width + valueFieldSpace;

                        EditorGUI.BeginChangeCheck();
                        element.Value2 = EditorGUI.FloatField(rect, element.Value2);
                        changeValue2 = EditorGUI.EndChangeCheck();
                        break;
                    case 2: // 数値3:数値4
                        rect.width -= valueFieldSpace + valueFieldMarginLeft + valueFieldMarginRight;
                        rect.width /= 2f;
                        rect.x += valueFieldMarginLeft;
                        
                        EditorGUI.BeginChangeCheck();
                        element.Value3 = EditorGUI.FloatField(rect, element.Value3);
                        changeValue3 = EditorGUI.EndChangeCheck();

                        rect.x += rect.width + valueFieldSpace;

                        EditorGUI.BeginChangeCheck();
                        element.Value4 = EditorGUI.FloatField(rect, element.Value4);
                        changeValue4 = EditorGUI.EndChangeCheck();
                        break;
                    case 3: // ボタン表示
                        rect.width /= 2;
                        if (GUI.Button(rect, "+"))
                        {
                            // 要素を複製して追加
                            baseElements.Insert(element.Id, new TreeElement
                            {
                                Name = element.Name,
                                TemplateIndex = element.TemplateIndex,
                                Value1 = element.Value1,
                                Value2 = element.Value2,
                                Value3 = element.Value3,
                                Value4 = element.Value4,
                            });
                            Update();
                            Reload();
                        };
                        rect.x += rect.width;

                        EditorGUI.BeginDisabledGroup(this.baseElements.Count <= 1);
                        if (GUI.Button(rect, "-"))
                        {
                            baseElements.RemoveAt(element.Id);
                            Update();
                            Reload();
                        };
                        EditorGUI.EndDisabledGroup();
                        break;
                }

                if (changeValue1) { element.UpdateValue4(); }
                if (changeValue2) { element.UpdateValue3(); }
                if (changeValue3) { element.UpdateValue4(); }
                if (changeValue4) { element.UpdateValue3(); }
            }
        }


        /// <summary>
        /// ルートとなるTreeViewItemを作成
        /// </summary>
        protected override TreeViewItem BuildRoot()
        {
            // ルートはdepth=-1として設定する必要がある
            var root = new TreeViewItem { depth = -1, id = -1, displayName = "Root", children = new List<TreeViewItem>() };

            if (baseElements.Count > 0)
            {
                //　モデルからTreeViewItemの親子関係を構築
                var elements = new List<TreeViewItem>();
                foreach (var baseElement in baseElements)
                {
                    var baseItem = CreateTreeViewItem(baseElement);
                    root.AddChild(baseItem);
                    AddChildrenRecursive(baseElement, baseItem);
                }

                // 親子関係に基づいてDepthを自動設定するメソッド
                SetupDepthsFromParentsAndChildren(root);
            }

            return root;
        }

        /// <summary>
        /// モデルとItemから再帰的に子Itemを作成・追加する
        /// </summary>
        private void AddChildrenRecursive(TreeElement model, TreeViewItem item)
        {
            foreach (var childModel in model.Children)
            {
                var childItem = CreateTreeViewItem(childModel);
                item.AddChild(childItem);
                AddChildrenRecursive(childModel, childItem);
            }
        }

        /// <summary>
        /// 要素を作成
        /// </summary>
        private TreeViewItem CreateTreeViewItem(TreeElement model)
        {
            return new TreeViewItem { id = model.Id, displayName = "Label" };
        }

        /// <summary>
        /// TreeView初期化
        /// </summary>
        public void Setup(List<TreeElement> list)
        {
            baseElements = list;
            Update();
            Reload();
        }

        /// <summary>
        /// ID設定
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < baseElements.Count; i++)
            {
                baseElements[i].Id = i;
                baseElements[i].UpdateValue4();
            }
        }

        public void AddItem()
        {
            baseElements.Add(new TreeElement());
            Update();
            Reload();
        }

        [System.Serializable]
        public class TreeElement
        {
            [SerializeField] public int TemplateIndex = 0;
            [SerializeField] public string Name = "---"; // 名前
            [SerializeField] public float Value1 = 1f; // 数値
            [SerializeField] public float Value2 = 1f; // 結果
            [SerializeField] public float Value3 = 1f; // 結果
            [SerializeField] public float Value4 = 1f; // 結果

            public int Id { get; set; } = 0; // 要素のId
            public TreeElement Parent { get; private set; } = null; // 親の要素
            public List<TreeElement> Children { get; } = new List<TreeElement>(); // 子の要素

            public void UpdateValue3()
            {
                Value3 = Value4 * Value1 / Value2;
            }
            public void UpdateValue4()
            {
                Value4 = Value3 * Value2 / Value1;
            }

            /// <summary>
            /// 子を追加
            /// </summary>
            internal void AddChild(TreeElement child)
            {
                // 既に親がいたら削除
                if (child.Parent != null)
                {
                    child.Parent.RemoveChild(child);
                }

                // 親子関係を設定
                Children.Add(child);
                child.Parent = this;
            }

            /// <summary>
            /// 子を削除
            /// </summary>
            public void RemoveChild(TreeElement child)
            {
                if (Children.Contains(child))
                {
                    Children.Remove(child);
                    child.Parent = null;
                }
            }
        }
    }
}