using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Android;

public class BL_communication : MonoBehaviour
{
    //package name
    const string pluginName = "com.rzechowski.bl_forunity.BluetoothPlugin";

    static AndroidJavaClass _pluginClass;
    static AndroidJavaObject _pluginInstance;

    public static AndroidJavaClass PluginClass
    {
        get
        {
            if (_pluginClass == null)
            {
                _pluginClass = new AndroidJavaClass(pluginName);
                AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
                _pluginClass.SetStatic<AndroidJavaObject>("mainActivity", activity);
            }
            return _pluginClass;
        }
    }

    public static AndroidJavaObject PluginInstance
    {
        get
        {
            if (_pluginInstance == null)
            {
                _pluginInstance = PluginClass.CallStatic<AndroidJavaObject>("getInstance");
            }
            return _pluginInstance;
        }
    }

    //UI elements
    public Button btScan, btConnect, btApply;
    public Dropdown mDropdown;
    public GameObject ConnectionPanle, MessagePannel;
    public InputField childeName;
    //variables
    private List<string> deviceList;
    private string connectedDeviceName;
    private bool isConnected = false, connectionScreen = true;

    private void Awake()
    {
        //check if bluetooth is enabled
        PluginInstance.Call("requestBL");
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
        PluginInstance.Call("onStart");

        btScan.onClick.AddListener(Scan);
        btConnect.onClick.AddListener(connect);
        btApply.onClick.AddListener(sendNameToESP32);
        deviceList = new List<string>();
        mDropdown.options.Clear();
        mDropdown.AddOptions(deviceList);
        InvokeRepeating("getAvailableDeviceLsit", 4.0f, 4.0f);
    }

    //scan for available devices. Scan is automatically call in onStart procedure for 20 seconds. If after this time
    //your device is not found you need to call scan procedure.
    private void Scan()
    {
        PluginInstance.Call("scan");
    }

    //thread initialized on script start. Responsible for updating 'available device list'
    private void getAvailableDeviceLsit()
    {
        if (!isConnected)
        {
            mDropdown.options.Clear();
            string availableDevices = PluginInstance.Call<string>("getAvailableDevicesList");
            deviceList = availableDevices.Split(';').ToList();
            deviceList.RemoveAt(deviceList.Count - 1);  //remove last elemnet, becouse it is empty
            mDropdown.AddOptions(deviceList);
        }
    }

    //connect to the device selected on dropdown list
    private void connect()
    {
        if (deviceList.Count > 0)
        {
            PluginInstance.Call("connect", mDropdown.value);
            InvokeRepeating("getConnectionStatus", 0.5f, 3.0f);
            connectedDeviceName = deviceList.ElementAt<string>(mDropdown.value);
        }
    }

    //thread initialized by connect button, updating each 4 seconds
    private void getConnectionStatus()
    {
        isConnected = PluginInstance.Call<bool>("isConnected");
        if (isConnected)
        {
            //if we are connected move to the next scene
            if (connectionScreen)
            {
                changeScreen();
                connectionScreen = false;
            }
        }
    }

    //changes screen to 'input name' screen
    private void changeScreen()
    {
        float width = ConnectionPanle.GetComponent<RectTransform>().rect.width;
        ConnectionPanle.GetComponent<RectTransform>().position = new Vector2(-width / 2, ConnectionPanle.transform.position.y);
        MessagePannel.GetComponent<RectTransform>().position = new Vector2(width / 2, MessagePannel.transform.position.y);
    }

    //send message with childe name to server and chnge screen to instruction
    private void sendNameToESP32()
    {
        if (isConnected)
        {
            if (PluginInstance.Call<bool>("sendMessgae", childeName.text))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    private void OnDestroy()
    {
        PluginInstance.Call("onStop");
    }
}