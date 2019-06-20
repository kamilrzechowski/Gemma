using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InstructionSwiping : MonoBehaviour
{
    public RectTransform instruction_p1, instruction_p2, instruction_p3, instruction_p4, instruction_p5;
    private const float distance = 1080f;
    private float shift = 0f;
    private int current_page = 0;
    private bool free2speak = true;

    private void Start()
    {
        //.transform.position returns center of the panel and DOAnchorPos has point (0,0) on the left bottom of panel, so we need
        //to know the shift to properly set new panel position
        shift = instruction_p1.transform.position.x;
        current_page = 0;
        speak();
    }

    public void NextPage()
    {
        //set new position of panel
        instruction_p1.DOAnchorPos(new Vector2(instruction_p1.transform.position.x - distance - shift, 0), 0.25f);
        instruction_p2.DOAnchorPos(new Vector2(instruction_p2.transform.position.x - distance - shift, 0), 0.25f);
        instruction_p3.DOAnchorPos(new Vector2(instruction_p3.transform.position.x - distance - shift, 0), 0.25f);
        instruction_p4.DOAnchorPos(new Vector2(instruction_p4.transform.position.x - distance - shift, 0), 0.25f);
        instruction_p5.DOAnchorPos(new Vector2(instruction_p5.transform.position.x - distance - shift, 0), 0.25f);
        current_page++;
        speak();
    }
    public void PrevPage()
    {
        instruction_p1.DOAnchorPos(new Vector2(instruction_p1.transform.position.x + distance - shift, 0), 0.25f);
        instruction_p2.DOAnchorPos(new Vector2(instruction_p2.transform.position.x + distance - shift, 0), 0.25f);
        instruction_p3.DOAnchorPos(new Vector2(instruction_p3.transform.position.x + distance - shift, 0), 0.25f);
        instruction_p4.DOAnchorPos(new Vector2(instruction_p4.transform.position.x + distance - shift, 0), 0.25f);
        instruction_p5.DOAnchorPos(new Vector2(instruction_p5.transform.position.x + distance - shift, 0), 0.25f);
        current_page--;
        speak();
    }

    public void speak()
    {
        //if we have BL connection, flowerpot is not speaking now and we are not on the last page
        if (GameManager.Instance.getConnectionStatus() == 2 && free2speak && current_page < 4) {
            GameManager.Instance.sendMsg("Inst" + (current_page+1).ToString());
        }
        else if(free2speak)
        {
            Debug.Log("Instruction - no BL connection");
        }
        else
        {
            Debug.Log("Instruction - I'm not free to speak");
        }
    }
}
