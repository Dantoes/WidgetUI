using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WidgetUI
{
	public class TextTooltipWidget
		: UIBehaviour
		, IWidget<String>
	{
		public void Enable(string p_text)
		{
			Text textComponent = this.gameObject.GetComponentInChildren<Text>();
			
			if (textComponent != null)
			{
				textComponent.text = p_text;
			}
			else
			{
				Debug.LogWarning(String.Format("Unable to find Text component on object '{0}' or its children.", this.name));
			}

		}

		public void Disable()
		{
		}
	}
}
