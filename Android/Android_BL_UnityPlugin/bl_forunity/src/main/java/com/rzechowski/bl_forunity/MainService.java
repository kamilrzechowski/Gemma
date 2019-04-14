package com.rzechowski.bl_forunity;

import android.app.Service;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.Intent;
import android.os.Binder;
import android.os.Handler;
import android.os.IBinder;
import android.os.Message;
import android.util.Log;
import android.widget.ArrayAdapter;

import java.util.ArrayList;

public class MainService extends Service {

    private static final String TAG = "BluetoothPlugin";

    IBinder mBinder = new LocalBinder();
    private DeviceScanning mDeviceScanning = null;

    // Intent request codes
    private static final int REQUEST_CONNECT_DEVICE_SECURE = 1;
    private static final int REQUEST_CONNECT_DEVICE_INSECURE = 2;
    private static final int REQUEST_ENABLE_BT = 3;

    /**
     * Name of the connected device
     */
    private String mConnectedDeviceName = null;

    /**
     * Array adapter for the conversation thread
     */
    private ArrayAdapter<String> mConversationArrayAdapter;

    /**
     * String buffer for outgoing messages
     */
    private StringBuffer mOutStringBuffer;

    /**
     * Local Bluetooth adapter
     */
    private BluetoothAdapter mBluetoothAdapter = null;

    /**
     * Member object for the chat services
     */
    private BluetoothConnectionService mChatService = null;
    Globals globals = Globals.getInstance();

    @Override
    public void onCreate(){
        super.onCreate();
        // Get local Bluetooth adapter
        mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();

        // If the adapter is null, then Bluetooth is not supported
        if (mBluetoothAdapter == null) {
            Log.d(TAG, "Bluetooth is not available");
            //activity.finish();
        }
    }

    @Override
    public IBinder onBind(Intent intent) {
        // If BT is not on, request that it be enabled.
        if (!mBluetoothAdapter.isEnabled()) {
            //exit service
            this.stopSelf();
        } else if (mChatService == null) {
            mDeviceScanning = new DeviceScanning(this);
            Log.d(TAG, "setupChat()");
            // Initialize the BluetoothChatService to perform bluetooth connections
            mChatService = new BluetoothConnectionService(this, mHandler);
            // Initialize the buffer for outgoing messages
            mOutStringBuffer = new StringBuffer("");
        }
        return mBinder;
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        if (mChatService != null) {
            mChatService.stop();
        }

        if(mDeviceScanning!=null) {
            mDeviceScanning.doDiscovery(false);
            mDeviceScanning.mUnregisterReceiver();
        }
    }

    public class LocalBinder extends Binder {
        public MainService getServerInstance() {
            return MainService.this;
        }
    }

    private void sendDataToActivity()
    {
        Intent sendLevel = new Intent();
        sendLevel.setAction("GET_SIGNAL_STRENGTH");
        sendLevel.putExtra( "LEVEL_DATA","Strength_Value");
        sendBroadcast(sendLevel);

    }

    /**
     * Makes this device discoverable for 300 seconds (5 minutes).
     */
    public void ensureDiscoverable() {
        if (mBluetoothAdapter.getScanMode() !=
                BluetoothAdapter.SCAN_MODE_CONNECTABLE_DISCOVERABLE) {
            Intent discoverableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
            discoverableIntent.putExtra(BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, 300);
            startActivity(discoverableIntent);
        }
    }

    /**
     * Sends a message.
     *
     * @param message A string of text to send.
     */
    public void sendMessage(String message) {
        // Check that we're actually connected before trying anything
        if (mChatService.getState() != BluetoothConnectionService.STATE_CONNECTED) {
            Log.d(TAG,"not_connected");
            return;
        }

        // Check that there's actually something to send
        if (message.length() > 0) {
            // Get the message bytes and tell the BluetoothChatService to write
            byte[] send = message.getBytes();
            mChatService.write(send);

            // Reset out string buffer to zero and clear the edit text field
            mOutStringBuffer.setLength(0);
            //mOutEditText.setText(mOutStringBuffer);
        }
    }

    /**
     * Establish connection with other device
     */
    public void connectDevice(int position, boolean secure) {
        //cancel discovery if we are about to connect, because it is costly in terms of power consumption
        mDeviceScanning.doDiscovery(false);
        // Get the device MAC address
        ArrayList<BluetoothDevice> mDiscoveredBLDevices = mDeviceScanning.getAvailableDevicesList();
        String address = mDiscoveredBLDevices.get(position).getAddress();
        // Get the BluetoothDevice object
        BluetoothDevice device = mBluetoothAdapter.getRemoteDevice(address);
        // Attempt to connect to the device
        mChatService.connect(device, secure);
    }

    public void scan(){
        if(mDeviceScanning != null){
            mDeviceScanning.doDiscovery(true);
        } else{
            Log.d(TAG, "mDeviceScanning is null. Unable to perform scanning.");
        }
    }

    /**
     * The Handler that gets information back from the BluetoothChatService
     */
    private final Handler mHandler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            switch (msg.what) {
                case Constants.MESSAGE_STATE_CHANGE:
                    switch (msg.arg1) {
                        case BluetoothConnectionService.STATE_CONNECTED:
                            globals.setConnectionState(2);
                            break;
                        case BluetoothConnectionService.STATE_CONNECTING:
                            globals.setConnectionState(1);
                            break;
                        case BluetoothConnectionService.STATE_LISTEN:
                        case BluetoothConnectionService.STATE_NONE:
                            //we are disconnected
                            globals.setConnectionState(0);
                            break;
                    }
                    break;
                case Constants.MESSAGE_WRITE:
                    byte[] writeBuf = (byte[]) msg.obj;
                    // construct a string from the buffer
                    String writeMessage = new String(writeBuf);
                    break;
                case Constants.MESSAGE_READ:
                    byte[] readBuf = (byte[]) msg.obj;
                    // construct a string from the valid bytes in the buffer
                    String readMessage = new String(readBuf, 0, msg.arg1);
                    break;
                case Constants.MESSAGE_DEVICE_NAME:
                    // save the connected device's name
                    Log.d(TAG, "Connected to " + mConnectedDeviceName);

                    break;
                case Constants.MESSAGE_TOAST:
                    Log.d(TAG, msg.getData().getString(Constants.TOAST));
                    break;
            }
        }
    };
}
