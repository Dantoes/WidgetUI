using UnityEngine;
using UnityEngine.EventSystems;

namespace WidgetUI
{
	[RequireComponent(typeof(RectTransform))]
	public class DragAndDrop
		: UIBehaviour
		, IDragHandler
	{
		[SerializeField]
		private RectTransform m_targetTransform;

		public RectTransform TargetTransform
		{
			get
			{
				if (m_targetTransform == null)
				{
					m_targetTransform = this.GetComponent<RectTransform>();
				}

				return m_targetTransform;
			}
			set 
			{ 
				m_targetTransform = value;
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			this.TargetTransform.anchoredPosition += eventData.delta;
		}
	}
}
