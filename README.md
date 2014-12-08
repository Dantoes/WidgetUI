## README

WidgetUI aims to add basic components to UGUI (Unity's UI System introduced in version 4.6) in a programmer-friendly manner. 


#### Getting started

There is no documentation available yet. Take a look at the [WidgetUI-Examples repository](https://github.com/knowbuddy/WidgetUI-Examples) to see some examples on how to use the components or read the tutorial below.


#### Contribute

Any contribution is highly welcome.


## 10 Minute Tutorial: Creating a simple list

1. Create a new Unity project and clone this repository somewhere under the Assets folder.
2. Create a button by selecting `GameObject > UI > Button` from the menu. This will be our list element.
3. Now we need a way to assign the text to this element by code. To do that, create a new class that implements the `IWidget<T>` interface. Our list element is supposed to display strings and thus `String` is required to be the template argument. Create a new C# file, name it `TextListElement.cs` and paste in the following code. 

    ```c#
    using System;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using WidgetUI;

    public class TextListElement 
        : UIBehaviour
        , IWidget<String>
    {
        private Text m_textComponent;

        private void Awake()
        {
            m_textComponent = this.GetComponentInChildren<Text>();
        }

        public void Enable(string p_text)
        {
            m_textComponent.text = p_text;
        }

        public void Disable()
        {
            // ignore
        }
    }
    ```
4. Add the TextListElement behaviour to the button we created in step 2 (make sure you assign it to the Button object and not to its child)
5. Set the anchor to the top-left corner (*important!*)
6. Rename the button to something meaningful (TextListElement) and save it as a prefab. Delete the button from the hierarchy afterwards.
7. Time to design the list. Create three empty GameObjects in the following structure:
  - MyList
    - ScrollArea
	  - Content
8. Create a vertical Scrollbar and set the direction to `Bottom to Top`. Reparent it to MyList and move it to the right of ScrollArea.
9. Add a `ScrollRect` component to the ScrollArea object and set the following values:
  1. Content: the Content object you created in step 7
  2. Uncheck `Horizontal`
  3. Drag the Scrollbar object you created into the field `Vertical Scrollbar`
10. Also add an `Image` and a `Mask` component (uncheck the field `Show Mask Graphic`)
11. That's it for the design. Now we need to create a class for the list. Create a new C# file `MyList.cs` and paste in the following code:
    
	```c#
    using WidgetUI;
    using System;

    public class MyList : InspectableFixedListWidget<String, TextListElement>
    {
    }
    ```
    _Note that the second template argument is the class we created earlier._
    
12. Add the MyList behaviour to the MyList object you created in step 7 and set the values:
  1. `Widget Prefab` to the prefab from step 6
  2. `Scroll Rect Component` should point to the ScrollArea object created in step 6
  3. `Layout Class` to `FixedVerticalLayout`
  4. `Width` to `Stretch`
13. At this point the list is fully set up. The only thing that's left is to fill the list.
14. Create a new C# file `ListInit.cs` and paste in the following code:

    ```c#
    using UnityEngine;

    public class ListInit : MonoBehaviour
    {
        public MyList m_list;

        void Start()
        {
            // fill the list
            for(int i=0; i<40; ++i)
            {
                m_list.Add("Element " + i);
            }

            // register a callback
            m_list.OnSelectItem.AddListener(this.OnSelectItem);
        }

        void OnSelectItem(string p_item)
        {
            Debug.Log("Selected item: " + p_item);
        }
    }
	```
15. Attach this behaviour to an object in the scene (`Main Camera` for instance) and set the `List` field to `MyList`
16. Hit play!
