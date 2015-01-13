using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WidgetUI
{
	[SelectionBase]
	[RequireComponent(typeof(RectTransform))]
	public class Window
		: UIBehaviour
		, IWidget
		, IPointerDownHandler
	{
		[Serializable]
		public class WindowCloseEvent : UnityEvent { }

		[SerializeField]
		protected RectTransform m_contentArea;

		[SerializeField]
		protected Button m_closeButton;

		[SerializeField]
		[HelpBox("Leave unassigned to disable dragging.")]
		protected RectTransform m_dragArea;

		[SerializeField]
		protected Text m_titleText;

		protected DragAndDrop m_DragAndDropHandler;

		[SerializeField]
		protected WindowCloseEvent m_onClose = new WindowCloseEvent();

		public RectTransform Content { get; private set; }

		public RectTransform ContentArea
		{
			get { return m_contentArea; }
			set { m_contentArea = value; }
		}

		public Button CloseButton
		{
			get { return m_closeButton; }
			set
			{
				if(m_closeButton != null)
				{
					m_closeButton.onClick.RemoveListener(this.CloseButtonClickHandler);
				}

				if(value != null)
				{
					value.onClick.AddListener(this.CloseButtonClickHandler);
				}

				m_closeButton = value;
			}
		}

		public RectTransform DragArea
		{
			get { return m_dragArea; }
			set 
			{
				if (m_DragAndDropHandler != null)
				{
					GameObject.Destroy(m_DragAndDropHandler);
					m_DragAndDropHandler = null;
				}

				if (value != null)
				{
					// Don't reassign the component if it's already attached.
					// This also prevents deletion of a previously attached component since m_DragAndDropHandler is not set.
					if(value.GetComponent<DragAndDrop>() == null)
					{
						m_DragAndDropHandler = value.gameObject.AddComponent<DragAndDrop>();
						m_DragAndDropHandler.TargetTransform = this.GetComponent<RectTransform>();
					}
				}

				m_dragArea = value; 
			}
		}

		public WindowCloseEvent OnClose
		{
			get { return m_onClose; }
			set { m_onClose = value; }
		}

		public String Title
		{
			get
			{
				return (m_titleText == null) ? null : m_titleText.text;
			}
			set
			{
				if(m_titleText != null)
				{
					m_titleText.text = value;
				}
			}
		}

	
		protected override void Awake()
		{
			base.Awake();

			// register event handlers
			this.CloseButton = m_closeButton;
			this.DragArea = m_dragArea;
		}


		protected virtual void CloseButtonClickHandler()
		{
			this.Close();
		}

		public virtual RectTransform SetContent(GameObject p_content, WindowParams p_params = null)
		{
			RectTransform contentTransform = p_content.GetComponent<RectTransform>();
			if(contentTransform == null)
			{
				throw new ArgumentException("Content must have a RectTransform component attached", "p_content");
			}

			if(p_params != null)
			{
				IWidget<WindowParams> paramReceiver = p_content.GetComponent(typeof(IWidget<WindowParams>)) as IWidget<WindowParams>;
				if(paramReceiver != null)
				{
					p_params.Window = this;
					paramReceiver.Enable(p_params);
				}
			}

			contentTransform.SetParent(m_contentArea, false);
			this.Content = contentTransform;
			return contentTransform;
		}
		
		public virtual void Close()
		{
			m_onClose.Invoke();

			GameObject.Destroy(this.gameObject);
		}

		public virtual void BringToFront()
		{
			this.transform.SetAsLastSibling();
		}

		public virtual void SendToBack()
		{
			this.transform.SetAsFirstSibling();
		}

		public virtual void CenterOnScreen()
		{
			RectTransform rectTransform = this.GetComponent<RectTransform>();
			Vector2 screenCenter = new Vector2(Screen.width >> 1, Screen.height >> 1);
			Vector2 selfCenter = rectTransform.GetWorldRect().center;
			Vector2 centerDelta = screenCenter - selfCenter;
			rectTransform.anchoredPosition += centerDelta;
		}

		public virtual void OnPointerDown(PointerEventData eventData)
		{
			this.BringToFront();
		}
	}
}
