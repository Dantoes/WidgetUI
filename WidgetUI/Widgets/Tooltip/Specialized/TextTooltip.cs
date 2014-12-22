using System;
using UnityEngine;

namespace WidgetUI
{
	public class TextTooltip
		: Tooltip<String, TextTooltipWidget>
	{
		[SerializeField]
		[Multiline]
		protected string m_text;

		[SerializeField]
		protected TextTooltipWidget m_widgetPrefab;

		protected override string GetContent()
		{
			return m_text;
		}

		protected override IWidgetAllocator<TextTooltipWidget> GetWidgetAllocator()
		{
			return new DefaultWidgetAllocator<TextTooltipWidget>(m_widgetPrefab.gameObject);
		}
	}
}
