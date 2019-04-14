package com.rzechowski.bl_forunity;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.ComponentName;
import android.content.Intent;
import android.content.ServiceConnection;
import android.os.IBinder;
import android.util.Log;

import java.util.ArrayList;

import static android.content.Context.BIND_AUTO_CREATE;

public class BluetoothPlugin {

    private static final BluetoothPlugin ourInstance = new BluetoothPlugin();
    private static final String TAG = "BluetoothPlugin";
    private static final int REQUEST_ENABLE_BT = 3;


    public static Activity mainActivity;

    private boolean mBounded;
    private MainService mService;
    private Globals globals = Globals.getInstance();

    public static BluetoothPlugin getInstance() {
        Log.d(TAG,"MyPlugin getInstance");
        return ourInstance;
    }

    private BluetoothPlugin() {;}

    public void requestBL(){
        BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
        if (!mBluetoothAdapter.isEnabled()) {
            Intent enableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
            mainActivity.startActivityForResult(enableIntent, REQUEST_ENABLE_BT);
        }
    }

    public void onStart() {
        Log.d(TAG,"onStart()");
        Intent mIntent = new Intent(mainActivity, MainService.class);
        mainActivity.bindService(mIntent, mConnection, BIND_AUTO_CREATE);
    }

    public void onStop() {
        Log.d(TAG,"onStop()");
        if(mBounded) {
            mainActivity.unbindService(mConnection);
            mBounded = false;
        }
    };

    ServiceConnection mConnection = new ServiceConnection() {
        @Override
        public void onServiceDisconnected(ComponentName name) {
            Log.d(TAG,"Service is disconnected");
            mBounded = false;
            mService = null;
        }

        @Override
        public void onServiceConnected(ComponentName name, IBinder service) {
            Log.d(TAG,"Service is connected");
            mBounded = true;
            MainService.LocalBinder mLocalBinder = (MainService.LocalBinder)service;
            mService = mLocalBinder.getServerInstance();
            //scan for new devices if we are not connected
            if(globals.getConnectionState() == 0){
                scan();
            }
        }
    };

    public void scan(){
        if(mService != null) {
            mService.scan();
        } else{
            Log.d(TAG,"mService is null");
        }
    }

    public String getAvailableDevicesList(){
        String availableDevices = "";
        ArrayList<BluetoothDevice> mDiscoveredBLDevices = globals.getAvailableDeviceList();
        for(int i = 0;i<mDiscoveredBLDevices.size();i++){
            availableDevices += mDiscoveredBLDevices.get(i).getName() + ";";
        }

        return availableDevices;
    }

    public void connect(int position){
        if(mService != null) {
            //Log.d(TAG, "Connect to device: " + position);
            mService.connectDevice(position, false);
        } else{
            Log.d(TAG,"mService is null");
        }
    }

    public int isConnected(){
        return globals.getConnectionState();
    }

    public boolean sendMessgae(String messgae){
        if(mService != null){
            mService.sendMessage(messgae);
            return true;
        }
        return false;
    }

    //TODO: 3 stages - disconnected, connecting, connected
}
