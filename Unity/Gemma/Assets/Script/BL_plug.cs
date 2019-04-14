using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class BL_plug : MonoBehaviour
{
    //variables
    protected List<string> deviceList;
    protected string connectedDeviceName;
    protected bool connectionScreen = true;
    protected int isConnected = 0;    //0 - disconnected, 1 - connecting, 2 - conneceted
    protected bool askedForBLPermission = false;
    public float deviceListUpdateInterval = 2.0f, connectionStatusUpdateInterval = 1.5f; //how often is device list and connection status update

    public bool debug_mode = false;

    //UI mandatory elements
    [Header("Mandatory objects")]
    public Button btSendMsg;
    public Button btScan, btConnect;
    public Dropdown mDropdown;
    public InputField sendMsgField;
    public Text connectionStatusText;
    //UI optional elements
    [Header("Optional objects")]
    [Tooltip("ConnectionPanle and MessagePannel are mandatory if debug_mode is not selected.")]
    public Button btDisconnect = null;
    public GameObject ConnectionPanle, MessagePannel;

    // Start is called before the first frame update
    void Start()
    {
        //buttons listners
        btSendMsg.onClick.AddListener(sendMsg);
        if(btDisconnect != null)
            btDisconnect.onClick.AddListener(disconnect);
        btConnect.onClick.AddListener(connect);
        btScan.onClick.AddListener(scan);

        deviceList = new List<string>();
        mDropdown.options.Clear();
        mDropdown.AddOptions(new List<string> { "no avaliable devices found" });
        InvokeRepeating("updateAvalibleDeviceList", 3.0f, deviceListUpdateInterval);
        InvokeRepeating("updateConnectionStatus", 0.5f, connectionStatusUpdateInterval);
    }

    //method call in time intervals
    protected void updateAvalibleDeviceList()
    {
        if (isConnected == 0)   //no connection
        {
            mDropdown.options.Clear();
            string availableDevices = GameManager.Instance.getAvalibaleDeviceList();
            deviceList = availableDevices.Split(';').ToList();
            deviceList.RemoveAt(deviceList.Count - 1);  //remove last elemnet, becouse it is empty
            if (deviceList.Any())   //if we found at list one device
            {
                mDropdown.AddOptions(deviceList);
            }
            else
            {
                mDropdown.AddOptions(new List<string> { "no avaliable devices found" });
            }
        }
    }

    //method call in time intervals
    protected void updateConnectionStatus()
    {
        isConnected = GameManager.Instance.getConnectionStatus();
        if (isConnected == 2)   //we are connected
        {
            //if we are in app (not debug mode) and if we are connected move to the next scene
            if (!debug_mode && connectionScreen)
            {
                changeScreen();
                connectionScreen = false;
            }
            if (connectionStatusText != null)
            {
                connectionStatusText.text = "Connected";
            }
        }
        else if (isConnected == 1) //we are connecting
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = "Connecting...";
            }
        }
        else if (isConnected == 0)    //we are disconnected
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text = "Disconnected";
            }
        }
    }

    //changes screen to 'input name' screen
    protected void changeScreen()
    {
        float width = ConnectionPanle.GetComponent<RectTransform>().rect.width;
        ConnectionPanle.GetComponent<RectTransform>().position = new Vector2(-width / 2, ConnectionPanle.transform.position.y);
        MessagePannel.GetComponent<RectTransform>().position = new Vector2(width / 2, MessagePannel.transform.position.y);
    }

    //scan for available devices. Scan is automatically call in onStart procedure for 20 seconds. If after this time
    //your device is not found you need to call scan procedure.
    protected void scan()
    {
        GameManager.Instance.scan();
    }

    //connect to the device selected on dropdown list
    protected void connect()
    {
        if (deviceList.Count > 0)
        {
            GameManager.Instance.connect(mDropdown.value);
            connectedDeviceName = deviceList.ElementAt<string>(mDropdown.value);
        }
    }

    protected void disconnect()
    {
        //TODO disconnection
    }

    protected void sendMsg()
    {
        if (isConnected == 2)    //we are connected
        {
            if (GameManager.Instance.sendMsg(sendMsgField.text)) //if the message was send succesfully
            {
                if (!debug_mode)
                {
                    SceneManager.LoadScene("Instruction_p1");
                }
            }
        }
    }
}
