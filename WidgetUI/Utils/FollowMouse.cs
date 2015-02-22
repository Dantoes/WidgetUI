using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WidgetUI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("WidgetUI/Follow Mouse")]
	public class FollowMouse 
		: UIBehaviour
	{
		[SerializeField]
		private TextAnchor m_anchor = TextAnchor.UpperLeft;

		[SerializeField]
		private Vector2 m_offset = Vector2.zero;

		[SerializeField]
		[Tooltip("If enabled, this object will stick to the mouse. Otherwise, the position will be set once on initialization.")]
		private bool m_follow = true;

		[SerializeField]
		[Tooltip("If enabled, this object will be kept inside the parent canvas")]
		private bool m_preventOverflow = true;

		#region Properties
		public Vector2 Offset
		{
			get
			{
				return m_offset;
			}
			set
			{
				m_offset = value;
			}
		}

		public TextAnchor Anchor
		{
			get
			{
				return m_anchor;
			}
			set
			{
				m_anchor = value;
				this.RectTransform.pivot = this.CalculatePivotFromAnchor(m_anchor);
			}
		}

		public bool Follow
		{
			get
			{
				return m_follow;
			}
			set
			{
				m_follow = value;
			}
		}

		public bool PreventOverflow
		{
			get
			{
				return m_preventOverflow;
			}
			set
			{
				m_preventOverflow = value;
			}
		}

		// RectTransform reference and caching mechanism
		[NonSerialized]
		private RectTransform m_rectTransform;
		RectTransform RectTransform
		{
			get
			{
				if (m_rectTransform == null)
				{
					m_rectTransform = this.GetComponent<RectTransform>();
				}
				return m_rectTransform;
			}
		}

		// Canvas transform reference and caching mechanism
		[NonSerialized]
		private RectTransform m_canvasTransform;
		RectTransform Canvas
		{
			get
			{
				if (m_canvasTransform == null)
				{
					Canvas canvas = this.GetComponentInParent<Canvas>();
					if(canvas != null)
					{
						m_canvasTransform = canvas.GetComponent<RectTransform>();
					}
				}
				return m_canvasTransform;
			}
		}
		#endregion

		protected override void Start()
		{
			base.Start();

			this.RectTransform.pivot = this.CalculatePivotFromAnchor(m_anchor);
			this.RectTransform.anchorMin = Vector2.zero;
			this.RectTransform.anchorMax = Vector2.zero;

			this.UpdatePosition();
		}

		/// <summary>
		/// Update the position if the RectTransform changes
		/// </summary>
		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			this.UpdatePosition();
		}

		private void LateUpdate()
		{
			if(m_follow)
			{
				this.UpdatePosition();
			}
		}

		private void UpdatePosition()
		{
			Vector3 mousePos = Input.mousePosition;
			Vector2 mouseScreenPos = new Vector2(mousePos.x, mousePos.y);
			Vector2 anchoredPosition = mouseScreenPos + m_offset;

			this.RectTransform.anchoredPosition = anchoredPosition;

			if(m_preventOverflow)
			{
				Rect canvasRect = this.Canvas.GetWorldRect();
				Rect ownRect = this.RectTransform.GetWorldRect();

				Vector2 offset = Vector2.zero;

				float dx1 = ownRect.xMin - canvasRect.xMin;
				float dx2 = ownRect.xMax - canvasRect.xMax;
				float dy1 = ownRect.yMin - canvasRect.yMin;
				float dy2 = ownRect.yMax - canvasRect.yMax;

				if (dx1 < 0)
				{
					offset.x -= dx1;
				}
				else if(dx2 > 0)
				{
					offset.x -= dx2;
				}

				if (dy1 < 0)
				{
					offset.y -= dy1;
				}
				else if (dy2 > 0)
				{
					offset.y -= dy2;
				}

				if (offset != Vector2.zero)
				{
					anchoredPosition += offset;
					this.RectTransform.anchoredPosition = anchoredPosition;
				}
			}
		}

		/// <summary>
		/// Calculates the normalized pivot position from a TextAnchor enumeration
		/// </summary>
		/// <param name="p_anchor"></param>
		/// <returns></returns>
		private Vector2 CalculatePivotFromAnchor(TextAnchor p_anchor)
		{
			float x = ((int)p_anchor % 3) * .5F;
			float y = 1.0F - ((int)p_anchor / 3) * .5F;
			return new Vector2(x, y);
		}
	}
}
