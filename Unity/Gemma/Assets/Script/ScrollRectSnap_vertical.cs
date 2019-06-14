using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectSnap_vertical : MonoBehaviour
{
    public RectTransform panel;
    public Button[] bttn;
    public RectTransform center;
    public int shift;   //how much is whole panel shift according to the center

    private float[] distance;
    private bool dragging = false;
    private int bttnDistance;
    private int minButtonNum;     //holds number of buttons with smallest distance 


    // Start is called before the first frame update
    void Start()
    {
        int bttnLength = bttn.Length;
        distance = new float[bttnLength];

        bttnDistance = (int)Mathf.Abs(-bttn[1].GetComponent<RectTransform>().anchoredPosition.y + bttn[0].GetComponent<RectTransform>().anchoredPosition.y);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < bttn.Length; i++)
        {
            distance[i] = Mathf.Abs(center.transform.position.y - bttn[i].transform.position.y);
        }

        float minDistance = Mathf.Min(distance);    //get the min distance

        for (int a = 0; a < bttn.Length; a++)
        {
            if (minDistance == distance[a])
            {
                minButtonNum = a;
            }
        }

        if (!dragging)
        {
            LerpToBttn(minButtonNum * bttnDistance + shift);
        }
    }

    void LerpToBttn(int position)
    {
        float newY = Mathf.Lerp(panel.anchoredPosition.y, position, Time.deltaTime * 10f);
        Vector2 newPosition = new Vector2(panel.anchoredPosition.x, newY);

        panel.anchoredPosition = newPosition;
    }

    public void StartDragging()
    {
        dragging = true;
    }

    public void EndDragging()
    {
        dragging = false;
    }
}
