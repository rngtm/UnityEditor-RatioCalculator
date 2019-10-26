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

    public class RatioTreeView : TreeView
    {
        private List<RatioTreeElement> baseElements = null; // 要素

        private const float nameFieldWidth = 80f;
        private const float value1FieldWidth = 80f;
        private const float value2FieldWidth = 80f;
        private const float rateFieldWidth = 60f;
        private const float ratePopupWidth = 75f;
        private const float buttonWidth = 20f;

        private static string[] templatePopupDisplayNames = new string[]
        {
            "カスタム",// 自分で入力
            "黄金比", // 黄金比 1.618
            "黄金比-逆数", // 黄金比 逆数  0.618
            "白銀比", // 白銀比  1.414 
            "白銀比-逆数", // 白銀比の逆数 0.707
            "青銅比", // 青銅比 3.302
            "青銅比-逆数", // 青銅比の逆数 0.303
        };

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
        public RatioTreeView(TreeViewState state)
            : base(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column() {
                    headerContent = new GUIContent("備考"), 
                    width = nameFieldWidth,
                    autoResize = false,
                },
                new MultiColumnHeaderState.Column() {
                    headerContent = new GUIContent("数値"),
                    width = value1FieldWidth,
                    autoResize = false,
                },
                new MultiColumnHeaderState.Column() {
                    headerContent = new GUIContent("係数"),
                    width = rateFieldWidth + ratePopupWidth,
                    autoResize = false,
                },
                new MultiColumnHeaderState.Column() {
                    headerContent = new GUIContent("結果"),
                    width = value2FieldWidth,
                    autoResize = false,
                },
                new MultiColumnHeaderState.Column() {
                    headerContent = new GUIContent(""),
                    width = buttonWidth * 2f,
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

                bool changeTemplate = false;
                bool changeValue1 = false;
                bool changeRate = false;
                switch (columnIndex)
                {
                    case 0: // 備考
                        rect.x += 2f;
                        rect.width -= 4f;
                        element.Name = EditorGUI.TextField(rect, element.Name);
                        break;
                    case 1: // 数値
                        EditorGUI.BeginChangeCheck();
                        element.Value1 = EditorGUI.FloatField(rect, element.Value1);
                        changeValue1 = EditorGUI.EndChangeCheck();
                        break;

                    case 2: // 係数
                        var rectTemplate = rect;
                        rectTemplate.width = ratePopupWidth;
                        rect.width -= ratePopupWidth;
                        rectTemplate.x = rect.x + rect.width + 2f;
                        rectTemplate.width -= 1f;

                        EditorGUI.BeginChangeCheck();
                        element.Rate = EditorGUI.FloatField(rect, element.Rate);
                        changeRate = EditorGUI.EndChangeCheck(); // 倍率を変更した

                        EditorGUI.BeginChangeCheck();
                        element.TemplateIndex = EditorGUI.Popup(rectTemplate, element.TemplateIndex, templatePopupDisplayNames);
                        changeTemplate = EditorGUI.EndChangeCheck(); // テンプレートを変更した
                        break;
                    case 3: // 結果
                        EditorGUI.FloatField(rect, element.Value2);
                        break;
                    case 4: // ボタン
                        rect.width /= 2;
                        if (GUI.Button(rect, "+"))
                        {
                            baseElements.Insert(element.Id, new RatioTreeElement
                            {
                                Name = element.Name,
                                TemplateIndex = element.TemplateIndex,
                                Value1 = element.Value1,
                                Rate = element.Rate,
                                Value2 = element.Value2,
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

                if (changeTemplate)
                {
                    element.Rate = GetRateFromTemplate((ETemplateType)element.TemplateIndex, element.Rate);
                    changeRate = true;
                }
                else
                if (changeRate)
                {
                    element.TemplateIndex = (int)ETemplateType.Custom;
                }

                if (changeValue1 || changeRate)
                {
                    element.Value2 = element.Value1 * element.Rate;
                }
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
        private void AddChildrenRecursive(RatioTreeElement model, TreeViewItem item)
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
        private TreeViewItem CreateTreeViewItem(RatioTreeElement model)
        {
            return new TreeViewItem { id = model.Id, displayName = "Label" };
        }

        /// <summary>
        /// TreeView初期化
        /// </summary>
        public void Setup(List<RatioTreeElement> list)
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
                baseElements[i].Update();
            }
        }

        /// <summary>
        /// 倍率の取得
        /// </summary>
        private float GetRateFromTemplate(ETemplateType templateIndex, float defaultRate)
        {
            float rate = 0f;
            switch (templateIndex)
            {
                case ETemplateType.Custom:
                    rate = defaultRate;
                    break;
                case ETemplateType.Gold:
                    rate = 1.618f;
                    //rate = (1f + Mathf.Sqrt(5f)) / 2f;
                    break;
                case ETemplateType.GoldReciprocal:
                    rate = 0.618f;
                    //rate = 2f / (1f + Mathf.Sqrt(5f));
                    break;
                case ETemplateType.Silver:
                    rate = 1.414f;
                    //rate = Mathf.Sqrt(2f);
                    break;
                case ETemplateType.SilverReciprocal:
                    rate = 0.707f;
                    //rate = 1f / Mathf.Sqrt(2f);
                    break;
                case ETemplateType.Bronze:
                    rate = 3.302f;
                    //rate = (3f + Mathf.Sqrt(13f)) / 2f;
                    break;
                case ETemplateType.BronzeReciprocal:
                    rate = 0.303f;
                    //rate = 2f / (3f + Mathf.Sqrt(13f));
                    break;
            }
            return rate;
        }
    }
}