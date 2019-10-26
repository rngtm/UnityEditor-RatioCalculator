///------------------------
/// Ratio Calculator
/// @ 2019 RNGTM
///------------------------
namespace RatioCalculator
{
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class RatioTreeElement
    {
        public int Id { get; set; } = 0; // 要素のId
        public RatioTreeElement Parent { get; private set; } = null; // 親の要素
        public List<RatioTreeElement> Children { get; } = new List<RatioTreeElement>(); // 子の要素

        [SerializeField] public int TemplateIndex = 0;
        [SerializeField] public string Name = "---"; // 名前
        [SerializeField] public float Value1 = 100f; // 数値
        [SerializeField] public float Rate = 0.3f; // 割合
        [SerializeField] public float Value2 = 0f; // 結果

        public void Update()
        {
            Value2 = Value1 * Rate;
        }

        /// <summary>
        /// 子を追加
        /// </summary>
        internal void AddChild(RatioTreeElement child)
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
        public void RemoveChild(RatioTreeElement child)
        {
            if (Children.Contains(child))
            {
                Children.Remove(child);
                child.Parent = null;
            }
        }
    }
}