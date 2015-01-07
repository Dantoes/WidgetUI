using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WidgetUI.Detail
{
	public abstract class ListWidgetBase<T, WidgetType, LayoutType>
		: UIBehaviour
		, IListWidget<T>
		, IEnumerable<T>
		where WidgetType : UIBehaviour, IWidget<T>
		where LayoutType : ILayout
	{
		protected IWidgetAllocator<WidgetType> m_allocator;
		protected LayoutType m_layout;

		protected List<T> m_items;
		protected List<WidgetType> m_widgets;
		protected IComparer<T> m_comparer;

		protected ScrollRect m_scroll;
		protected RectTransform m_scrollTransform;
		protected RectTransform m_contentArea;

		// item selection
		protected int m_selectedItemIndex = -1;
		protected ItemSelectEvent<T> m_onItemSelect = new ItemSelectEvent<T>();
		
		// button highlight variables
		protected Button m_selectedButton;
		protected int m_previouslySelectedItem;

		public ItemSelectEvent<T> OnSelectItem
		{
			get
			{
				return m_onItemSelect;
			}
			set
			{
				m_onItemSelect = value;
			}
		}


		protected virtual void Construct()
		{
			m_items = new List<T>();
			m_widgets = new List<WidgetType>();
			m_comparer = null;

			m_allocator = this.GetWidgetAllocator();
			
			m_scroll = this.GetScrollRect();
			m_scrollTransform = m_scroll.transform as RectTransform;

			m_layout = this.GetLayout();

			// get the ScrollRect's content area
			m_contentArea = m_scroll.content;
			if (m_contentArea == null)
			{
				GameObject contentAreaObject = new GameObject("Content");
				m_contentArea = contentAreaObject.AddComponent<RectTransform>();
				m_contentArea.SetParent(m_scroll.transform, false);
				m_scroll.content = m_contentArea;
			}
		}

		protected virtual void Destruct()
		{
		}

		protected abstract IWidgetAllocator<WidgetType> GetWidgetAllocator();
		protected abstract LayoutType GetLayout();
		protected abstract ScrollRect GetScrollRect();
		public abstract T this[int index] { get; set; }


		public void Enable(IList<T> p_dataObject)
		{
			this.AddRange(p_dataObject);
		}

		public void Disable()
		{
			this.Clear();
		}

		protected override void Awake()
		{
			base.Awake();
			this.Construct();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.Destruct();
		}

		protected virtual void LateUpdate()
		{
			this.KeepHighlightedButtonPressed();
		}

		public LayoutType Layout
		{
			get { return m_layout; }
		}

		public Vector2 ScrollArea
		{
			get
			{
				Rect scrollRectArea = m_scrollTransform.rect;
				return new Vector2(scrollRectArea.width, scrollRectArea.height);
			}
		}

		public Vector2 ScrollRange
		{
			get
			{
				Vector2 contentSize = m_contentArea.sizeDelta;
				Vector2 scrollArea = this.ScrollArea;
				return new Vector2()
				{
					// the content size can be smaller than the visible area; don't return negative values
					x = Mathf.Max(0, contentSize.x - scrollArea.x),
					y = Mathf.Max(0, contentSize.y - scrollArea.y)
				};
			}
		}

		public Vector2 ScrollPosition
		{
			get
			{
				Vector2 normalized = this.NormalizedScrollPosition;
				Vector2 range = this.ScrollRange;
				return new Vector2(normalized.x * range.x, normalized.y * range.y);
			}
			set
			{
				Vector2 range = this.ScrollRange;
				float scrollX = value.x / range.x;
				float scrollY = (value.y - range.y) / -range.y;
				this.NormalizedScrollPosition = new Vector2(scrollX, scrollY);
			}
		}

		public Vector2 NormalizedScrollPosition
		{
			get
			{
				return m_scroll.normalizedPosition;
			}
			set
			{
				m_scroll.normalizedPosition = value;
			}
		}

		public int SelectedItemIndex
		{
			get
			{
				return m_selectedItemIndex;
			}
			set
			{
				if(value >= this.Count)
				{
					throw new ArgumentOutOfRangeException("SelectedItemIndex out of range");
				}

				if (value >= 0)
				{
					this.HightlightButton(value);
				}

				m_selectedItemIndex = value;
			}
		}

		public T SelectedItem
		{
			get
			{
				return (m_selectedItemIndex < 0) ? default(T) : this[m_selectedItemIndex];
			}
		}

		public virtual void Insert(int p_index, T p_item)
		{
			this.Insert_Internal(p_index, p_item);
			this.RecreateWidgetListeners(p_index + 1);
		}

		private void Insert_Internal(int p_index, T p_item)
		{
			if (p_index <= m_selectedItemIndex)
			{
				++m_selectedItemIndex;
			}

			m_items.Insert(p_index, p_item);
			m_widgets.Insert(p_index, null);
		}

		public virtual void RemoveAt(int p_index)
		{
			this.RemoveAt_Internal(p_index);
			this.RecreateWidgetListeners(p_index);
		}

		private void RemoveAt_Internal(int p_index)
		{
			// update selected item index
			if (p_index == m_selectedItemIndex)
			{
				m_selectedItemIndex = -1;
			}
			else if (p_index < m_selectedItemIndex)
			{
				--m_selectedItemIndex;
			}

			m_items.RemoveAt(p_index);
			m_widgets.RemoveAt(p_index);
		}

		public virtual void Clear()
		{
			m_selectedItemIndex = -1;
			m_items.Clear();
			m_widgets.Clear();
		}

		public virtual void ScrollTo(int p_index)
		{
			Vector2 widgetPosition = m_layout.GetWidgetPosition(p_index);
			this.ScrollPosition = widgetPosition;
		}

		public void ScrollTo(T p_item)
		{
			int index = this.IndexOf(p_item);
			if (index >= 0)
			{
				this.ScrollTo(index);
			}
		}

		public virtual IList<T> Remove(Predicate<T> p_match)
		{
			List<T> removedItems = new List<T>(this.Count / 4);

			for (int i = 0; i < m_items.Count; )
			{
				T item = m_items[i];
				if (p_match(item))
				{
					removedItems.Add(item);
					this.RemoveAt_Internal(i); // does not recreate listeners
				}
				else
				{
					++i;
				}
			}

			// If items have been removed, recreate all listeners.
			if(removedItems.Count > 0)
			{
				this.RecreateWidgetListeners();
			}

			return removedItems;
		}

		public void Add(T p_item)
		{
			if (m_comparer == null)
			{
				this.Insert(this.Count, p_item);
			}
			else
			{
				this.AddSorted(p_item, m_comparer);
			}
		}

		private void AddSorted(T p_item, IComparer<T> p_comparer)
		{
			int index = m_items.BinarySearch(p_item, p_comparer);
			if (index < 0)
			{
				// MSDN: The zero-based index of item in the sorted List<T>, if item is found; otherwise, 
				// a negative number that is the bitwise complement of the index of the next element that 
				// is larger than item or, if there is no larger element, the bitwise complement of Count.
				index = ~index;
			}
			this.Insert(index, p_item);
		}

		public bool Remove(T p_item)
		{
			int i = m_items.IndexOf(p_item);
			if (i < 0)
			{
				return false;
			}

			this.RemoveAt(i);
			return true;
		}

		public void AddRange(IEnumerable<T> p_collection)
		{
			foreach(T item in p_collection)
			{
				this.Add(item);
			}
		}

		public int IndexOf(T p_item)
		{
			return m_items.IndexOf(p_item);
		}

		public bool Contains(T p_item)
		{
			return m_items.Contains(p_item);
		}

		public void CopyTo(T[] p_array, int p_arrayIndex)
		{
			m_items.CopyTo(p_array, p_arrayIndex);
		}

		public int Count
		{
			get { return m_items.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return m_items.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}


		protected void CreateWidgetAt(int p_index)
		{
			WidgetType widget = m_allocator.Construct();

			RectTransform widgetTransform = widget.transform as RectTransform;
			widgetTransform.SetParent(m_contentArea, false);
			this.UpdateWidgetPosition(p_index, widgetTransform);

			Button button = widget.GetComponent<Button>();
			if(button != null)
			{
				button.onClick.AddListener(() => { this.OnWidgetClicked(p_index); });
			}

			m_widgets[p_index] = widget;
			widget.Enable(m_items[p_index]);
		}

		protected void RemoveWidgetAt(int p_index)
		{
			WidgetType widget = m_widgets[p_index];
			if (widget != null)
			{
				Button button = widget.GetComponent<Button>();
				if (button != null)
				{
					button.onClick.RemoveAllListeners();

					// if the button is currently selected, unselect it
					if (p_index == m_selectedItemIndex)
					{
						this.RemoveButtonHighlight();
					}
				}

				m_allocator.Destroy(widget);
				m_widgets[p_index] = null;
			}
		}

		protected virtual void RecreateWidgetListener(int p_index)
		{
			WidgetType widget = m_widgets[p_index];
			if (widget != null)
			{
				Button button = widget.GetComponent<Button>();
				if (button != null)
				{
					button.onClick.RemoveAllListeners();
					button.onClick.AddListener(() => { this.OnWidgetClicked(p_index); });
				}
			}
		}

		protected virtual void RecreateWidgetListeners(int p_startIndex = -1, int p_endIndex = -1)
		{
			if(p_startIndex < 0)
			{
				p_startIndex = 0;
			}

			if(p_endIndex < 0)
			{
				p_endIndex = this.Count - 1;
			}

			for(int i = p_startIndex; i <= p_endIndex; ++i)
			{
				this.RecreateWidgetListener(i);
			}
		}

		protected void UpdateWidgetPosition(int p_index)
		{
			this.UpdateWidgetPosition(p_index, m_widgets[p_index].transform as RectTransform);
		}

		protected void UpdateWidgetPosition(int p_index, RectTransform p_transform)
		{
			m_layout.SetWidgetPosition(p_index, p_transform);
		}


		protected virtual void OnWidgetClicked(int p_index)
		{
			m_selectedItemIndex = p_index;
			m_onItemSelect.Invoke(m_items[p_index]);
			this.HightlightButton(p_index);
		}

		protected C GetWidgetComponent<C>(int p_index) where C : Component
		{
			WidgetType widget = m_widgets[p_index];
			return (widget == null) ? null : widget.GetComponent<C>();
		}


		#region Button highlight
		protected void HightlightButton(int p_index)
		{
			// reset the previous button
			this.RemoveButtonHighlight();

			m_selectedButton = this.GetWidgetComponent<Button>(p_index);
			if (m_selectedButton != null)
			{
				this.PressButton(m_selectedButton);
			}
		}

		protected void RemoveButtonHighlight()
		{
			if(m_selectedButton != null)
			{
				this.ReleaseButton(m_selectedButton);
				m_selectedButton = null;
			}
		}

		protected void KeepHighlightedButtonPressed()
		{
			// set the selected button again, in case it has been destroyed and became visible again
			if (m_selectedItemIndex >= 0 && m_selectedItemIndex == m_previouslySelectedItem && m_selectedButton == null)
			{
				this.HightlightButton(m_selectedItemIndex);
			}

			// inject faked mouse events into Selectable
			if (m_selectedButton != null)
			{
				this.PressButton(m_selectedButton);
			}

			m_previouslySelectedItem = m_selectedItemIndex;
		}

		protected virtual void PressButton(int p_index)
		{
			Button button = this.GetWidgetComponent<Button>(p_index);
			if (button != null)
			{
				this.PressButton(button);
			}
		}

		protected virtual void PressButton(Button p_button)
		{
			PointerEventData fakeClickEvent = new PointerEventData(EventSystem.current)
			{
				button = PointerEventData.InputButton.Left
			};

			p_button.OnPointerEnter(fakeClickEvent);
			p_button.OnPointerDown(fakeClickEvent);
		}

		protected virtual void ReleaseButton(int p_index)
		{
			Button button = this.GetWidgetComponent<Button>(p_index);
			if(button != null)
			{
				this.ReleaseButton(button);
			}
		}

		protected virtual void ReleaseButton(Button p_button)
		{
			PointerEventData fakeMouseEvent = new PointerEventData(EventSystem.current);
			p_button.OnPointerUp(fakeMouseEvent);
			p_button.OnPointerExit(fakeMouseEvent);
		}

		#endregion

	}
}
