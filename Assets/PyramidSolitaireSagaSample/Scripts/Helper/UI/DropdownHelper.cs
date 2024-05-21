using System;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper.UI
{
    public static class DropdownHelper
    {
        public static void FillDropdownOptionValues<EnumType>(this TMP_Dropdown dropdown, float itemHeight, float paddingHeight)
        {
            dropdown.options.Clear();
            foreach (EnumType item in Enum.GetValues(typeof(EnumType)))
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(item.ToString()));
            }
            dropdown.RefreshShownValue();

            // Resize Template
            RectTransform templateTransform = dropdown.transform.Find("Template").GetComponent<RectTransform>();
            Vector2 originSize = templateTransform.sizeDelta;
            templateTransform.sizeDelta = new Vector2(originSize.x, dropdown.options.Count * itemHeight + paddingHeight);
        }
    }
}
