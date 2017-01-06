package ca.etsmtl.aj47620.bluetoothplugin;

//import android.app.Activity;
import java.io.*;
import java.util.Arrays;
import java.util.Set;
import java.util.UUID;

import android.annotation.SuppressLint;
import android.annotation.TargetApi;
import android.bluetooth.*;
import android.os.Build;
//import android.content.Intent;
import android.os.Handler;
import android.os.Message;

import com.unity3d.player.*;

@TargetApi(Build.VERSION_CODES.GINGERBREAD)
@SuppressLint("HandlerLeak")
public class BluetoothTransmitter /*extends Activity*/
{
	final int MESSAGE_READ = 9999;
	private UUID myUUID = UUID.fromString("f8a8bae3-3eba-493f-89e9-c221964b449b");
	private ConnectThread mConnectThread;
	private ConnectedThread mConnectedThread;
	private Handler handler;
	private BluetoothDevice firstDevice = null;
	//private CallBack onDataReceived;
	
	public String connectionState = "Bluetooth Offline";
	
	public BluetoothTransmitter(final CallBack onDataReceived)
	{
		//this.onDataReceived = onDataReceived;
		
		UnityPlayer.currentActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{				
				handler = new Handler()
				{
					@Override
					public void handleMessage(Message msg)
					{
						if (msg.what == MESSAGE_READ)
						{
							String data = (String)(msg.obj);
							int size = (int)(msg.arg1);
							onDataReceived.method(data, size);
						}
					}
				};
				
			}
		});
		
		connectionState = "Bluetooth Transmitter initialized";
	}
	
	public void startClientListening()
	{
		BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
		if (mBluetoothAdapter == null)
		{
		    // Device does not support Bluetooth
			connectionState = "Device does not support Bluetooth";
			return;
		}
		
		if (!mBluetoothAdapter.isEnabled())
		{
		    /*Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
		    int REQUEST_ENABLE_BT = 1;
			startActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);*/
			connectionState = "Bluetooth is currently not enabled";
			return;
		}
		
		Set<BluetoothDevice> pairedDevices = mBluetoothAdapter.getBondedDevices();
		// If there are paired devices
		if (pairedDevices.size() == 0)
		{
			connectionState = "Bluetooth: There is no paired devices";
		    return;
		}
		
		for (BluetoothDevice device : pairedDevices)
		{
			firstDevice = device;
			break;
	    }
		
		connectionState = "Bluetooth: Connecting to " + firstDevice.getName();
		
		// Start the thread to connect with the given device
        mConnectThread = new ConnectThread(firstDevice);
        mConnectThread.start();
	}
	
	public void clientReconnect()
	{
		if (firstDevice == null)
		{
			startClientListening();
			return;
		}
		
		connectionState = "Bluetooth: Reconnecting to " + firstDevice.getName();
		
		// Start the thread to connect with the given device
        mConnectThread = new ConnectThread(firstDevice);
        mConnectThread.start();
	}
	
	public void manageConnectedSocket(BluetoothSocket mmSocket)
	{
		mConnectedThread = new ConnectedThread(mmSocket);
        mConnectedThread.start();
	}
	
	public void sendData(final String data)
	{
		UnityPlayer.currentActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
		        if (mConnectedThread != null)
		        {
		        	// Since we use StreamReader.ReadLine() on the other side,
		        	// we need to add a NewLine to our \0 marker.
		        	// It's probably overkill to do it on both side, but whatever
		        	String newData = data + "\r\n\0\r\n";
		        	byte[] b = null;
		        	try
		        	{
						b = newData.getBytes("UTF-8");
					}
		        	catch (UnsupportedEncodingException e) {}
		        	mConnectedThread.write(b);
		        }
			}
		});
	}
	
	public void stopClientListening()
	{
		UnityPlayer.currentActivity.runOnUiThread(new Runnable()
		{
			public void run()
			{
		        if (mConnectedThread != null)
		        {
		        	mConnectedThread.cancel();
		        	mConnectedThread.interrupt();
		        }
		        if (mConnectThread != null)
		        {
		        	mConnectThread.cancel();
		        	mConnectThread.interrupt();
		        }
		        connectionState = "Bluetooth: Connection closed by local request";
			}
		});
	}
	
	
	
	private class ConnectThread extends Thread
	{
	    private final BluetoothSocket mmSocket;
	    private boolean socketIsClosed = true;
	    //private final BluetoothDevice mmDevice;
	 
	    public ConnectThread(BluetoothDevice device)
	    {
	        // Use a temporary object that is later assigned to mmSocket,
	        // because mmSocket is final
	        BluetoothSocket tmp = null;
	        //mmDevice = device;
	 
	        // Get a BluetoothSocket to connect with the given BluetoothDevice
	        try
	        {
	            // MY_UUID is the app's UUID string, also used by the server code
	            tmp = device.createRfcommSocketToServiceRecord(myUUID);
	        }
	        catch (IOException e)
	        {
	        	connectionState = "Bluetooth: problem with the UUID";
	        }
	        socketIsClosed = false;
	        mmSocket = tmp;
	    }
	 
	    public void run()
	    {
	    	while (true)
	        {
	    		if (socketIsClosed)
	    		{
	    			// Socket has been closed, get out of here
	    			return;
	    		}
		        try
		        {
		            // Connect the device through the socket. This will block
		            // until it succeeds or throws an exception
		            mmSocket.connect();
		            break;
		        }
		        catch (IOException connectException)
		        {
		            // Unable to connect
		            connectionState = "Bluetooth: " + mmSocket.getRemoteDevice().getName() + 
		            		" unreachable. Retrying...";
		        }
	        }
	 
	        connectionState = "Bluetooth connection established to " + mmSocket.getRemoteDevice().getName();
	        // Do work to manage the connection (in a separate thread)
	        manageConnectedSocket(mmSocket);
	    }
	 
	    /** Will cancel an in-progress connection, and close the socket */
	    public void cancel()
	    {
	        try
	        {
	            mmSocket.close();
	        }
	        catch (IOException e) { }
	        socketIsClosed = true;
	    }
	}
	
	
	
	private class ConnectedThread extends Thread
	{
	    private final BluetoothSocket mmSocket;
	    private final InputStream mmInStream;
	    private final OutputStream mmOutStream;
	 
	    public ConnectedThread(BluetoothSocket socket)
	    {
	        mmSocket = socket;
	        InputStream tmpIn = null;
	        OutputStream tmpOut = null;
	 
	        // Get the input and output streams, using temp objects because
	        // member streams are final
	        try
	        {
	            tmpIn = socket.getInputStream();
	            tmpOut = socket.getOutputStream();
	        }
	        catch (IOException e) { }
	 
	        mmInStream = tmpIn;
	        mmOutStream = tmpOut;
	    }
	 
	    public void run()
	    {
	        byte[] buffer = new byte[4096];  // buffer store for the stream
	        int bytes; // bytes returned from read()
	        String data = "";
	        int size = 0;
	 
	        // Keep listening to the InputStream until an exception occurs
	        while (true)
	        {
	            try
	            {
	                // Read from the InputStream
	                bytes = mmInStream.read(buffer);
	                // Send the obtained bytes to the UI activity
	                //handler.obtainMessage(MESSAGE_READ, bytes, -1, buffer).sendToTarget();
					try
					{
						byte[] subBuffer = Arrays.copyOfRange(buffer, 0, bytes);
						data += new String(subBuffer, "UTF-8");
						size += bytes;
					}
					catch (UnsupportedEncodingException e) {}
	            }
	            catch (IOException e)
	            {
	            	connectionState = "Bluetooth: Communication interrupted";
	            	clientReconnect();
	                break;
	            }
	            
	            if (data.endsWith("\0"))
	            {
	            	data = data.substring(0, data.length()-1);
	            	handler.obtainMessage(MESSAGE_READ, size, -1, data).sendToTarget();
	            	data = "";
	            	size = 0;
	            }
	        }
	    }
	 
	    /* Call this from the main activity to send data to the remote device */
	    public void write(byte[] bytes)
	    {
	        try
	        {
	            mmOutStream.write(bytes);
	        }
	        catch (IOException e)
	        {
	        	connectionState = "Bluetooth: Unable to send data";
	        }
	    }
	 
	    /* Call this from the main activity to shutdown the connection */
	    public void cancel()
	    {
	        try
	        {
	            mmSocket.close();
	        }
	        catch (IOException e) { }
	    }
	}
}










