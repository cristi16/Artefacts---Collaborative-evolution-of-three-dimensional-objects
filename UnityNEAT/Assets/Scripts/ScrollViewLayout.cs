using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollViewLayout : MonoBehaviour
{
    public float cellSize = 100;

    public ScrollRect scrollRect;

    public int selectedIndex = 0;

    private float verticalPosition = 1;

    void Start()
    {
        
    }

    void Update()
    {
        var numberOfChildren = transform.childCount;
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize * transform.root.GetComponent<Canvas>().scaleFactor * numberOfChildren);

        for(int i = 0; i < numberOfChildren; i++)
        {
            var rect = rectTransform.rect;
            var height = rect.height;
            var cellHeight = height/numberOfChildren;

            transform.GetChild(i).localPosition = rect.center - new Vector2(0f, height / 2f - cellHeight * i - cellHeight / 2f);
        }

        var scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) >= 0.1f && numberOfChildren > 1)
        {
            verticalPosition += Mathf.Sign(scrollInput) * 1f/ (numberOfChildren - 1);
            verticalPosition = Mathf.Clamp01(verticalPosition);
            selectedIndex += (int)Mathf.Sign(scrollInput);
            selectedIndex = Mathf.Clamp(selectedIndex, 0, numberOfChildren - 1);
        }

        scrollRect.verticalNormalizedPosition = verticalPosition;
    }

    public void Reset()
    {
        verticalPosition = 1;
        selectedIndex = transform.childCount - 1;
    }

    public void CenterOnItem(Transform target)
    {
        
    }
}
