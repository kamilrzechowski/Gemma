using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoScreen : MonoBehaviour
{
    public RawImage rawImage;
    public float videoRatio = 1.74f;

    private int prev_width = 0;
    private int videoWidth;
    private int videoHeight;
    private RectTransform objectRectTransform;
    private bool isPlaying = true;
    private VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoWidth = Screen.width;
        videoHeight = (int)(videoWidth / videoRatio);
        objectRectTransform = rawImage.GetComponent<RectTransform>();
        objectRectTransform.sizeDelta = new Vector2(objectRectTransform.sizeDelta.x, videoHeight);

        Screen.autorotateToPortrait = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.orientation = ScreenOrientation.AutoRotation;

        prev_width = Screen.width;
    }

    // Update is called once per frame
    void Update()
    {
        if(prev_width != Screen.width)
        {
            videoWidth = Screen.width;
            videoHeight = (int)(videoWidth / videoRatio);
            if(videoHeight > Screen.height)
            {
                videoHeight = Screen.height;
                videoWidth = (int)(videoHeight / videoRatio);
            }
            objectRectTransform.sizeDelta = new Vector2(objectRectTransform.sizeDelta.x, videoHeight);
            prev_width = Screen.width;
        }

        if (Input.touchCount > 0)
        {
            if (isPlaying)
            {
                isPlaying = false;
                videoPlayer.Pause();
            }
            else
            {
                isPlaying = true;
                videoPlayer.Play();
            }
        }
    }
}
