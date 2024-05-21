using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    //Original version of the ConditionalEnumHideAttribute created by Brecht Lecluyse (www.brechtos.com)
    //Modified by: -
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionalEnumHideAttribute : PropertyAttribute
    {
        //The name of the bool field that will be in control
        public string ConditionalSourceField = "";

        public int EnumValue1 = 0;
        public int EnumValue2 = 0;

        public bool HideInInspector = false;
        public bool Inverse = false;

        public ConditionalEnumHideAttribute(string conditionalSourceField, int enumValue1)
        {
            this.ConditionalSourceField = conditionalSourceField;
            this.EnumValue1 = enumValue1;
            this.EnumValue2 = enumValue1;
        }

        public ConditionalEnumHideAttribute(string conditionalSourceField, int enumValue1, bool hideInInspector)
        {
            this.ConditionalSourceField = conditionalSourceField;
            this.EnumValue1 = enumValue1;
            this.HideInInspector = hideInInspector;
        }
    }
}
