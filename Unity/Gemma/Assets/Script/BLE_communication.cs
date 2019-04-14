using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class BLE_communication : MonoBehaviour
{

    const string pluginName = "com.rzechowski.ble_connection.MyPlugin";

    static AndroidJavaClass _pluginClass;
    static AndroidJavaObject _pluginInstance;

    public static AndroidJavaClass PluginClass
    {
        get
        {
            if(_pluginClass == null)
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
            if(_pluginInstance == null)
            {
                _pluginInstance = PluginClass.CallStatic<AndroidJavaObject>("getInstance");
            }
            return _pluginInstance;
        }
    }

    public Button btScan, btConnect;
    public Text text;
    public InputField input;
    public Dropdown mDropdown;
    private bool isConnected = false;
    private string connectedDeviceName = "";     //name of device we are connected to
    private const string flowerPotName = "Kamil";

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
            // We do not have permission to use the fine location.
            // Ask for permission or proceed without the functionality enabled.
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        //init the bluethooth componetns
        PluginInstance.Call("init");

        btScan.onClick.AddListener(Scan);
        btConnect.onClick.AddListener(connect);
        //javaClass.Call("init", new object[] { context, currentActivityObject });

        mDropdown.options.Clear();
        mDropdown.AddOptions(new List<string> { });

        InvokeRepeating("checkConnectionState", 4.0f, 4.0f);
    }

    //update available device list and check connection status. If connected don't update available device list
    private void checkConnectionState()
    {
        isConnected = PluginInstance.Call<bool>("isConnected");
        if (!isConnected)
        {
            avalibleDeviceList();
        }
        else
        {
            connectedDeviceName = PluginInstance.Call<string>("getConnectedDeviceName");
            Debug.Log("is connected to device: " + connectedDeviceName);
            if (connectedDeviceName.Equals(flowerPotName) && SceneManager.GetActiveScene().buildIndex < 1) //if we connected to our flowerpot and we are in the first scene
            {
                SceneManager.LoadScene(1);
            }
        }
    }

    //scan for available devices
    private void Scan()
    {
        PluginInstance.Call("scan");
    }

    //get aviable devices list
    private void avalibleDeviceList()
    {
        mDropdown.options.Clear();
        string avalibleDevices = PluginInstance.Call<string>("getAvailableDevicesLit");
        List<string> avalibeDeviceList = avalibleDevices.Split(';').ToList();
        mDropdown.AddOptions(avalibeDeviceList);
    }

    //connect to selected on dropdown list device
    private void connect()
    {
        if (mDropdown.options.Count() > 0) {
            string deviceName = mDropdown.options[mDropdown.value].text;
            PluginInstance.Call("connectDevice", deviceName);
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            //android onPause()
            PluginInstance.Call("onPause");
        }
        else
        {
            //android onResume()
            PluginInstance.Call("onResume");
        }
    }

    // generate a message when the app shuts down
    void OnDestroy()
    {
        PluginInstance.Call("onDestroy");
    }
}
