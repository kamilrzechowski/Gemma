using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Assertions;

using Vuforia;

public class Game2 : MonoBehaviour
{
    public Text timerText, colorText, color2findText, scoreText;
    public GameObject panel_top, panel_right, panel_bottom, panel_left;
    //public RawImage rawImage;

    //GUI variables
    private Matrix4x4 myMatix;
    private bool first = true;
    private Rect pos_timerText = new Rect(Screen.width + (Screen.height - Screen.width)/2 - 400, (Screen.height - Screen.width)/2 + 50, 300, 100);
    private Rect pos_colorText = new Rect(Screen.width/2 - 50, Screen.height/2 - 50, 300, 100);
    private Rect pos_color2findText = new Rect((Screen.width - Screen.height) / 2 + 50, Screen.height / 2, 300, 100);
    private Rect pos_scoreText = new Rect((Screen.width - Screen.height) / 2 + 50, Screen.height / 2 - 100, 300, 100);


    private bool mAccessCameraImage = true;
    private Texture2D texture;
    private int margin_w, margin_h;
    private int avg_R = 0, avg_G = 0, avg_B = 0;
    //private Rect txtTimer = new Rect(Screen.);
    private float startTime;
    private int dedline = 2;
    private int color2find;
    private int score = 0;
    private System.Random random;
    private float t;
    public struct ColorInfo
    {
        public string color;
        public string color_data;
        public ColorInfo(string p1, string p2)
        {
            color = p1;
            color_data = p2;
        }
    };
    public struct MyColor
    {
        public string name;
        public Color color;

        public MyColor(string p1, Color p2)
        {
            name = p1;
            color = p2;
        }
    };
    private MyColor[] mycolor = {
        new MyColor("Red", new Color(1,0,0)),
        new MyColor("Green", new Color(0,1,0)),
        new MyColor("Blue", new Color(0,0,1)),
        new MyColor("Yellow", new Color(1,1,0)),
        new MyColor("Black", new Color(0,0,0)),
        new MyColor("White", new Color(1,1,1))};

    private ColorInfo colorInfo = new ColorInfo("", "");
    private int varaiance = -1;



    // The desired camera image pixel format
    // private Image.PIXEL_FORMAT mPixelFormat = Image.PIXEL_FORMAT.RGB565;// or RGBA8888, RGB888, RGB565, YUV
    // Boolean flag telling whether the pixel format has been registered


#if UNITY_EDITOR
    private Vuforia.PIXEL_FORMAT mPixelFormat = Vuforia.PIXEL_FORMAT.GRAYSCALE;
#elif UNITY_ANDROID
   private Vuforia.PIXEL_FORMAT mPixelFormat =  Vuforia.PIXEL_FORMAT.RGB888;
#elif UNITY_IOS
    private Vuforia.PIXEL_FORMAT mPixelFormat =  Vuforia.PIXEL_FORMAT.RGB888;
#endif

    private bool mFormatRegistered = false;

    private void Awake()
    {
        Screen.autorotateToPortrait = true;
        Screen.autorotateToLandscapeLeft = false;
        Screen.orientation = ScreenOrientation.Portrait;
    }

    void Start()
    {
        margin_w = Screen.width / 5;
        margin_h = Screen.height / 5;

        texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        // Register Vuforia life-cycle callbacks:
        Vuforia.VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        Vuforia.VuforiaARController.Instance.RegisterOnPauseCallback(OnPause);
        Vuforia.VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);

        /*GameObject ngo = new GameObject("myTextGO");
        ngo.transform.SetParent(this.transform);
        colorText = AddTextToCanvas("null", ngo, new Rect(0, 0, 0, 0));*/
        //initTextFileds();
        startTime = Time.time;
        random = new System.Random();
        setColor2FindandScore();

        RectTransform panelTopRectTransform = panel_top.GetComponent<RectTransform>();
        panelTopRectTransform.sizeDelta = new Vector2(panelTopRectTransform.sizeDelta.x, margin_h * 2);
        panel_bottom.GetComponent<RectTransform>().sizeDelta = new Vector2(panelTopRectTransform.sizeDelta.x, margin_h * 2);
        panel_right.GetComponent<RectTransform>().sizeDelta = new Vector2(margin_w *2, Screen.height - (2*margin_h) + 6);
        panel_left.GetComponent<RectTransform>().sizeDelta = new Vector2(margin_w * 2, Screen.height - (2 * margin_h) + 6);
    }

    /// <summary>
    /// Called when Vuforia is started
    /// </summary>
    private void OnVuforiaStarted()
    {
        // Try register camera image format
        if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
        {
            Debug.Log("Successfully registered pixel format " + mPixelFormat.ToString());
            mFormatRegistered = true;
        }
        else
        {
            Debug.LogError("Failed to register pixel format " + mPixelFormat.ToString() +
                "\n the format may be unsupported by your device;" +
                "\n consider using a different pixel format.");
            mFormatRegistered = false;
        }
    }

    /*private void OnDestroy()
    {
        Destroy(timerText);
        timerText = null;
        Destroy(colorText);
        colorText = null;
        Destroy(color2findText);
        color2findText = null;
        Destroy(scoreText);
        scoreText = null;
    }*/
    /// <summary>
    /// Called when app is paused / resumed
    /// </summary>
    private void OnPause(bool paused)
    {
        if (paused)
        {
            Debug.Log("App was paused");
            UnregisterFormat();
        }
        else
        {
            Debug.Log("App was resumed");
            RegisterFormat();
        }
    }

    /// <summary>
    /// Called each time the Vuforia state is updated
    /// </summary>
    private void OnTrackablesUpdated()
    {
        t = Time.time - startTime;
        //if we have still time
        if (t / 60 < dedline)
        {
            //display time left
            if (t / 60 > dedline * 0.9)
                timerText.color = new Color(0, 0, 1);
            timerText.text = ((int)(dedline - (t / 60))).ToString() + ":" + (60 - (t % 60)).ToString("00");

            if (mFormatRegistered)
            {
                if (mAccessCameraImage)
                {
                    Vuforia.Image image = CameraDevice.Instance.GetCameraImage(mPixelFormat);
                    if (image != null && image.IsValid())
                    {
                        /* imageInfo = mPixelFormat + " image: \n";
                        imageInfo += " size: " + image.Width + " x " + image.Height + "\n";
                        imageInfo += " bufferSize: " + image.BufferWidth + " x " + image.BufferHeight + "\n";
                        imageInfo += " stride: " + image.Stride;*/
                        byte[] pixels = image.Pixels;
                        var varianceR = new List<float>();
                        var varianceG = new List<float>();
                        var varianceB = new List<float>();
                        float H = 0, S = 0, V = 0;

                        if (pixels != null && pixels.Length > 0)
                        {
                            ///info screen_W = 1080, screen_H = 2190 for Huaweii P20 Lite
                            ///info image_W = 1280, image_H = 720 for Huaweii P20 Lite

                            //Texture2D tex = new Texture2D(Screen.width/2, Screen.height/2, TextureFormat.RGB24, false); // RGB24
                            //texture.Resize(image.Width, image.Height);
                            //texture.Resize(image.Width, image.Height);
                            //texture.LoadRawTextureData(pixels);
                            //texture.Apply();
                            //rawImage.texture = texture;
                            //rawImage.material.mainTexture = texture;

                            //make sure, that margin % = 0
                            int image_w = image.Width, image_h = image.Height;
                            int image_margin_w = image_w / 5, image_margin_h = image_h / 5;

                            if (image_margin_w % 3 != 0)
                            {
                                image_margin_w += (3 - image_margin_w % 3);
                            }
                            if (image_margin_h % 3 != 0)
                            {
                                image_margin_h += (3 - image_margin_h % 3);
                            }
                            float sumeR = 0.0f, sumeG = 0.0f, sumeB = 0.0f;

                            int size = 0;
                            for (int j = image_margin_h; j < image_h - image_margin_h; j += 6)
                            {
                                for (int i = image_margin_w; i < image_w - image_margin_w; i += 6)
                                {
                                    if (mPixelFormat == Vuforia.PIXEL_FORMAT.RGB888)
                                    {
                                        sumeR += pixels[(i + j * image_w) * 3];
                                        sumeG += pixels[(i + j * image_w) * 3 + 1];
                                        sumeB += pixels[(i + j * image_w) * 3 + 2];
                                        if (i % (image_margin_w * 6) == 0)
                                        {
                                            varianceR.Add(pixels[(i + j * image_w) * 3]);
                                            varianceG.Add(pixels[(i + j * image_w) * 3 + 1]);
                                            varianceB.Add(pixels[(i + j * image_w) * 3 + 2]);
                                        }
                                    }
                                    else if (mPixelFormat == Vuforia.PIXEL_FORMAT.GRAYSCALE)
                                    {
                                        sumeR += pixels[(i + j * image_w)];
                                        sumeG += pixels[(i + j * image_w)];
                                        sumeB += pixels[(i + j * image_w)];
                                        if (i % (image_margin_w * 6) == 0)
                                        {
                                            varianceR.Add(pixels[(i + j * image_w)]);
                                            varianceG.Add(pixels[(i + j * image_w)]);
                                            varianceB.Add(pixels[(i + j * image_w)]);
                                        }
                                    }
                                    size++;
                                }
                            }

                            avg_R = (int)(sumeR / size);
                            avg_G = (int)(sumeG / size);
                            avg_B = (int)(sumeB / size);
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(avg_R, avg_G, avg_B);
                            H = color.GetHue();
                            S = color.GetSaturation();
                            V = color.GetBrightness();
                        }
                        varaiance = (int)variance(varianceR);
                        colorInfo = getColor(H, S, V, varaiance);
                        if (colorInfo.color == mycolor[color2find].name)
                        {
                            //color found -> increase score and set new color to find
                            score++;
                            setColor2FindandScore();
                        }
                       colorText.text = colorInfo.color + colorInfo.color_data + " " + varaiance.ToString();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Unregister the camera pixel format (e.g. call this when app is paused)
    /// </summary>
    private void UnregisterFormat()
    {
        Debug.Log("Unregistering camera pixel format " + mPixelFormat.ToString());
        CameraDevice.Instance.SetFrameFormat(mPixelFormat, false);
        mFormatRegistered = false;
    }
    /// <summary>
    /// Register the camera pixel format
    /// </summary>
    private void RegisterFormat()
    {
        if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
        {
            Debug.Log("Successfully registered camera pixel format " + mPixelFormat.ToString());
            mFormatRegistered = true;
        }
        else
        {
            Debug.LogError("Failed to register camera pixel format " + mPixelFormat.ToString());
            mFormatRegistered = false;
        }
    }

    /*void OnGUI()
    {
        if (first)
        {
            GUIUtility.RotateAroundPivot(90, new Vector2(Screen.width / 2, Screen.height / 2));
            myMatix = GUI.matrix;
            first = false;
        }
        // Your GUI code goes here
        GUI.matrix = myMatix;
        GUIStyle txtStyle = new GUIStyle("label");
        txtStyle.fontSize = 60;
        GUI.Label(pos_colorText, colorInfo.color + colorInfo.color_data + " " + varaiance.ToString(), txtStyle);
        GUI.Label(pos_scoreText, "Score:", txtStyle);
        if (t / 60 > dedline * 0.9)
            txtStyle.normal.textColor = new Color(0, 0, 1);
        GUI.Label(pos_timerText, ((int)(dedline - (t / 60))).ToString() + ":" + (60 - (t % 60)).ToString("00"), txtStyle);
        txtStyle.normal.textColor = mycolor[color2find].color;
        GUI.Label(pos_color2findText, "Find: " + mycolor[color2find].name, txtStyle);
    }*/

    void DrawQuad(Rect position, Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.Box(position, GUIContent.none);
    }

    private float variance(List<float> nums)
    {
        if (nums.Any())
        {
            // Get the average of the values
            float avg = nums.Average();
            // Now figure out how far each point is from the mean
            // So we subtract from the number the average
            // Then raise it to the power of 2
            double sumOfSquares = 0.0;
            foreach (int num in nums)
            {
                sumOfSquares += System.Math.Pow((num - avg), 2.0);
            }
            // Finally divide it by n - 1 (for standard deviation variance)
            // Or use length without subtracting one ( for population standard deviation variance)
            return (float)(sumOfSquares / (double)(nums.Count() - 1));
        }
        else { return 0.0f; }
    }

    private ColorInfo getColor(float H, float S, float V, int variance)
    {
        if (variance < 500)
        {
            if (V < 0.25)
            {
                return new ColorInfo("Black" , "(" + H.ToString("0.0") + "," + S.ToString("0.0") + "," + V.ToString("0.0") + ")");
            }
            else if (S < 0.2 && 0.6 < V)
            {
                return new ColorInfo("White","(" + H.ToString("0.0") + "," + S.ToString("0.0") + "," + V.ToString("0.0") + ")");
            }
            else if ((H < 25 || H > 330) && S > 0.4 && V > 0.35)
            {
                return new ColorInfo("Red","(" + H.ToString("0.0") + "," + S.ToString("0.0") + "," + V.ToString("0.0") + ")");
            }
            else if (70 < H && H < 165 && S > 0.4 && V > 0.35)
            {
                return new ColorInfo("Green","(" + H.ToString("0.0") + "," + S.ToString("0.0") + "," + V.ToString("0.0") + ")");
            }
            else if (165 < H && H < 260 && S > 0.4 && V > 0.35)
            {
                return new ColorInfo("Blue","(" + H.ToString("0.0") + "," + S.ToString("0.0") + "," + V.ToString("0.0") + ")");
            }
            else if (50 < H && H < 70 && S > 0.4 && V > 0.35)
            {
                return new ColorInfo("Yellow","(" + H.ToString("0.0") + "," + S.ToString("0.0") + "," + V.ToString("0.0") + ")");
            }
            else
            {
                return new ColorInfo("Unknown","(" + H.ToString("0.0") + "," + S.ToString("0.0") + "," + V.ToString("0.0") + ")");
            }
        }

        return new ColorInfo("None", "None");
    }

    private void setColor2FindandScore()
    {
        color2find = random.Next(mycolor.Length);
        color2findText.color = mycolor[color2find].color;
        color2findText.text = "Find: " + mycolor[color2find].name;
        scoreText.text = "Score: " + score.ToString();
    }

    private void initTextFileds()
    {
        if(timerText == null)
            timerText = GameObject.Find("Txt_time").GetComponent<Text>();
        if (colorText == null)
            colorText = GameObject.Find("Txt_color").GetComponent<Text>();
        if (color2findText == null)
            color2findText = GameObject.Find("Txt_color2find").GetComponent<Text>();
        if (scoreText == null)
            scoreText = GameObject.Find("Txt_score").GetComponent<Text>();
    }

    public static Text AddTextToCanvas(string textString, GameObject canvasGameObject, Rect pos)
    {
        Text text = canvasGameObject.AddComponent<Text>();
        text.text = textString;

        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = ArialFont;
        text.fontSize = 60;
        text.transform.position = new Vector2(pos.x, pos.y);
        text.material = ArialFont.material;

        return text;
    }
}
