using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    ///////////////////////////////////////////////////////////////////////
    ///Android library variables
    ///////////////////////////////////////////////////////////////////////

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

    ///////////////////////////////////////////////////////////////////////
    ///Class callbacks
    ///////////////////////////////////////////////////////////////////////

    // guarantee this will be always a singleton only - can't use the constructor!
    protected GameManager()
    {
    }

    protected override void Awake()
    {
        base.Awake();
        //check if bluetooth is enabled
        PluginInstance.Call("requestBL");
    }

    protected virtual void Start()
    {
        //DontDestroyOnLoad(transform.gameObject);
        PluginInstance.Call("onStart");
    }

    protected virtual void OnDestroy()
    {
        PluginInstance.Call("onStop");
    }


    ///////////////////////////////////////////////////////////////////////
    ///Class metods
    ///////////////////////////////////////////////////////////////////////

    public void connect(int position)
    {
        PluginInstance.Call("connect", position);
    }

    public bool sendMsg(string msg)
    {
        return PluginInstance.Call<bool>("sendMessgae", msg + "|");
    }

    public void scan()
    {
        Debug.Log("Scan in gamemenager called");
        PluginInstance.Call("scan");
    }

    public int getConnectionStatus()
    {
        Debug.Log("Get connection status called");
        return PluginInstance.Call<int>("isConnected");
    }

    public string getAvalibaleDeviceList()
    {
        return PluginInstance.Call<string>("getAvailableDevicesList");
    }
}
