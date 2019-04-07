package com.rzechowski.bl_forunity;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Handler;
import android.util.Log;

import java.util.ArrayList;

public class DeviceScanning {

    //Tag for log
    private static final String TAG = "BluetoothPlugin";

    //Return Intent extra
    public static String EXTRA_DEVICE_ADDRESS = "device_address";

    //Scan for 20 seconds and than stop -> don't waste battery
    private static final long SCAN_PERIOD = 20000;

    // Member fields
    private BluetoothAdapter mBtAdapter;

    //Newly discovered devices
    private ArrayList<BluetoothDevice> mDiscoveredBLDevices;


    private Context context = null;
    private Handler mHandler;
    private boolean mScanning = false;
    Globals globals = Globals.getInstance();

    public DeviceScanning(Context context){
        this.context = context;
        // Register for broadcasts when a device is discovered
        IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
        context.registerReceiver(mReceiver, filter);

        // Register for broadcasts when discovery has finished
        filter = new IntentFilter(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);
        context.registerReceiver(mReceiver, filter);

        // Get the local Bluetooth adapter
        mBtAdapter = BluetoothAdapter.getDefaultAdapter();

        mHandler = new Handler();
        mDiscoveredBLDevices = new ArrayList<BluetoothDevice>();
    }

    //unregister receiver on destroy
    public void mUnregisterReceiver(){
        context.unregisterReceiver(mReceiver);
    }

    /**
     * Start device discover with the BluetoothAdapter
     */
    public void doDiscovery(final boolean enable) {
        Log.d(TAG, "doDiscovery()");

        if (enable) {
            mDiscoveredBLDevices.clear();
            // If we're already discovering, stop it
            if (mBtAdapter.isDiscovering()) {
                mBtAdapter.cancelDiscovery();
            }

            // Stops scanning after a pre-defined scan period.
            mHandler.postDelayed(new Runnable() {
                @Override
                public void run() {
                    mScanning = false;
                    mBtAdapter.cancelDiscovery();
                }
            }, SCAN_PERIOD);

            mScanning = true;
            mBtAdapter.startDiscovery();
        } else {
            mScanning = false;
            mBtAdapter.cancelDiscovery();
        }
    }

    public ArrayList<BluetoothDevice> getAvailableDevicesList(){
        return mDiscoveredBLDevices;
    }

    /**
     * The BroadcastReceiver that listens for discovered devices and changes the title when
     * discovery is finished
     */
    private final BroadcastReceiver mReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();

            // When discovery finds a device
            if (BluetoothDevice.ACTION_FOUND.equals(action)) {
                // Get the BluetoothDevice object from the Intent
                BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                mDiscoveredBLDevices.add(device);
                globals.setAvailableDeviceList(mDiscoveredBLDevices);
            } else if (BluetoothAdapter.ACTION_DISCOVERY_FINISHED.equals(action)) {
                // TODO do something after discovery finished
            }
        }
    };
}

