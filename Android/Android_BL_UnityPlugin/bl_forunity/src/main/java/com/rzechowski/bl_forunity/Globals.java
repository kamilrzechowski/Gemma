package com.rzechowski.bl_forunity;

import android.bluetooth.BluetoothDevice;

import java.util.ArrayList;

public class Globals {
    private static final String TAG = "Globals Class";

    private static Globals instance;
    private ArrayList<BluetoothDevice> mDiscoveredBLDevices;
    private int connected = 0;  //0 - disconnected, 1 - connecting, 2 - conneceted

    // Restrict the constructor from being instantiated
    private Globals(){}

    public static synchronized Globals getInstance(){
        if(instance==null){
            instance=new Globals();
        }
        return instance;
    }

    public ArrayList<BluetoothDevice> getAvailableDeviceList() {
        if(this.mDiscoveredBLDevices == null){
            this.mDiscoveredBLDevices = new ArrayList<BluetoothDevice>();
        }
        return this.mDiscoveredBLDevices;
    }

    public void setAvailableDeviceList(ArrayList<BluetoothDevice> mDeviceList) {
        this.mDiscoveredBLDevices = mDeviceList;
    }

    public void setConnectionState(int connected){
        this.connected = connected;
    }

    public int getConnectionState(){
        return this.connected;
    }
}
