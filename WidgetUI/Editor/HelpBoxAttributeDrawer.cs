using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;

namespace WidgetUI.Editor
{
	[CustomPropertyDrawer(typeof(HelpBox))]
	public class HelpBoxAttributeDrawer : PropertyDrawer
	{
		private static readonly float helpBoxLineHeight = 12;
		private static readonly float helpBoxPadding = 4;
		private static readonly float minHelpBoxHeight = GetHelpBoxHeight(2);

		private static float GetHelpBoxHeight(int p_lines = 1)
		{
			return p_lines * helpBoxLineHeight + 2 * helpBoxPadding;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			int lines = 1 + this.GetMessage().Count(n => n == '\n');
			float helpBoxHeight = Mathf.Max(minHelpBoxHeight, GetHelpBoxHeight(lines));

			return helpBoxHeight + EditorGUIUtility.standardVerticalSpacing + base.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			float propertyHeight = EditorGUI.GetPropertyHeight(property, label);
			float propertyHeightWithSpacing = propertyHeight + EditorGUIUtility.standardVerticalSpacing;

			Rect propertyPosition = position;
			propertyPosition.height = propertyHeight;

			++EditorGUI.indentLevel;
			Rect helpBoxPosition = EditorGUI.IndentedRect(position);
			helpBoxPosition.y += propertyHeightWithSpacing;
			helpBoxPosition.height -= propertyHeightWithSpacing;
			--EditorGUI.indentLevel;

			EditorGUI.PropertyField(propertyPosition, property, label);
			EditorGUI.HelpBox(helpBoxPosition, this.GetMessage(), MessageType.Info);
		}

		private string GetMessage()
		{
			HelpBox helpBoxAttribute = (HelpBox)this.attribute;
			return helpBoxAttribute.message;
		}
	}
}
