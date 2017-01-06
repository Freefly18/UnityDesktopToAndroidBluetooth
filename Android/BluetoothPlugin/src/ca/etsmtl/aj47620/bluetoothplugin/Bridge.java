package ca.etsmtl.aj47620.bluetoothplugin;

import com.unity3d.player.*;

public class Bridge
{
	// tests
	public static String ReturnString()
	{
		return "Static String";
	}
	
	public static int ReturnInt()
	{
		return 5;
	}
	
	public String ReturnInstanceString()
	{
		return "Instance String";
	}
	
	public int ReturnInstanceInt()
	{
		return 34656;
	}
	
	/********************************************************************/
	
	private BluetoothTransmitter bbt;
	
	public void SetupBluetoothTransmitter(	final String gameObjectName, final String startMethodName, 
											final String dataMethodName, final String endMethodName)
	{
		bbt = new BluetoothTransmitter(new CallBack()
		{
			public void method(String data, int size)
			{
				System.out.println("***************** 4- Sending data '" + data + "' to Unity");
				
				UnityPlayer.UnitySendMessage(gameObjectName, startMethodName, "Start");
				
				if (size < 1024)
				{
					UnityPlayer.UnitySendMessage(gameObjectName, dataMethodName, data);
				}
				else
				{
					int nbMessages = (size / 1024) + 1;
					int messagesLength = data.length() / nbMessages;
					for (int i=0; i<nbMessages; i++)
					{
						int startPos = i * messagesLength;
						int endPos = (i == nbMessages-1 ? data.length() : startPos + messagesLength);
						String subData = data.substring(startPos, endPos);
						UnityPlayer.UnitySendMessage(gameObjectName, dataMethodName, subData);
					}
				}
				
				UnityPlayer.UnitySendMessage(gameObjectName, endMethodName, "End");
			}
		});
	}
	
	public void StartClientListening()
	{
		bbt.startClientListening();
	}
	
	public void SendData(String data)
	{
		bbt.sendData(data);
	}
	
	public void StopClientListening()
	{
		bbt.stopClientListening();
	}
	
	public String GetConnectionState()
	{
		return bbt.connectionState;
	}
}











