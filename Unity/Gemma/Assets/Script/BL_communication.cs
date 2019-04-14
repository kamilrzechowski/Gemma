using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

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
    public Text connectionStatusText;
    //variables
    private List<string> deviceList;
    private string connectedDeviceName;
    private bool connectionScreen = true;
    private int isConnected = 0;    //0 - disconnected, 1 - connecting, 2 - conneceted
    //UI debuging
    public InputField MsgToSend;
    public Button btSendMsg;
    public Text recivedMsg;
    public Button btDisconnect;


    private void Awake()
    {
        //check if bluetooth is enabled
        PluginInstance.Call("requestBL");
    }

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(transform.gameObject);
        PluginInstance.Call("onStart");

        btScan.onClick.AddListener(Scan);
        btConnect.onClick.AddListener(connect);
        btApply.onClick.AddListener(sendNameToESP32);
        deviceList = new List<string>();
        mDropdown.options.Clear();
        mDropdown.AddOptions(deviceList);
        InvokeRepeating("getAvailableDeviceLsit", 3.0f, 2.0f);

        //listners for debuging purpose
        btSendMsg.onClick.AddListener(sendMsg);
        btDisconnect.onClick.AddListener(disconnect);
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
        if (isConnected == 0)   //no connection
        {
            mDropdown.options.Clear();
            string availableDevices = PluginInstance.Call<string>("getAvailableDevicesList");
            deviceList = availableDevices.Split(';').ToList();
            deviceList.RemoveAt(deviceList.Count - 1);  //remove last elemnet, becouse it is empty
            if (deviceList.Any())   //if we found at list one device
            {
                mDropdown.AddOptions(deviceList);
            }
            else
            {
                mDropdown.AddOptions(new List<string>{ "no avaliable devices found" });
            }

        }
    }

    //connect to the device selected on dropdown list
    private void connect()
    {
        if (deviceList.Count > 0)
        {
            PluginInstance.Call("connect", mDropdown.value);
            InvokeRepeating("getConnectionStatus", 0.5f, 1.5f);
            connectedDeviceName = deviceList.ElementAt<string>(mDropdown.value);
        }
    }

    //thread initialized by connect button, updating each 4 seconds
    private void getConnectionStatus()
    {
        isConnected = PluginInstance.Call<int>("isConnected");
        if (isConnected == 2)   //we are connected
        {
            //if we are connected move to the next scene
            if (connectionScreen)
            {
                changeScreen();
                connectionScreen = false;
            }
            if (connectionStatusText != null)
            {
                connectionStatusText.text = "Connected";
            }
        } else if(isConnected == 1) //we are connecting
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = "Connecting...";
            }
        } else if (isConnected == 0)    //we are disconnected
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = "Disconnected";
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
        if (isConnected == 2)   //we are connected
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



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///Debuging procedures
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void sendMsg()
    {
        if(isConnected == 2)    //we are connected
        {
            PluginInstance.Call<bool>("sendMessgae", MsgToSend.text);
        }
    }

    private void disconnect()
    {
        if (isConnected == 2)    //we are connected
        {
            //TODO disconnection
        }
    }
}