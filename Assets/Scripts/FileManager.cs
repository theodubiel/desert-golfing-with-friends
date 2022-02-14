using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
class ClientInfo
{
    public static string FILENAME = "client-info";
    public string ipPort;
    public string passcode;

    public static readonly ClientInfo Default = new ClientInfo() {
        ipPort = "127.0.0.1:7777",
        passcode = ""
    };
}

[Serializable]
class HostInfo
{
    public static string FILENAME = "host-info";
    public string port;
    public string passcode;

    public static readonly HostInfo Default = new HostInfo() {
        port = "7777",
        passcode = ""
    };
}

[Serializable]
public class PlayerSettings
{
    public static string FILENAME = "player-settings";
    public string name;
    public float[] color;
    public byte ballType;

    public static readonly PlayerSettings Default = new PlayerSettings() {
        name = "Player",
        color = new float[] {1.0f, 1.0f, 1.0f},
        ballType = (byte)BallType.Circle
    };
}

public class FileManager
{ 
    public static void SaveData<T>(T data, string filename) {
        BinaryFormatter bf = new BinaryFormatter();
        var fullPath = string.Format("{0}/{1}.dat", Application.persistentDataPath, filename);
        FileStream file = File.Create(fullPath);
        bf.Serialize(file, data);
        file.Close();
    }

    public static T LoadData<T>(string filename, T fallback = default(T))
    {
        try {
            var fullPath = string.Format("{0}/{1}.dat", Application.persistentDataPath, filename);
            if (File.Exists(fullPath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file =  File.Open(fullPath, FileMode.Open);
                T data = (T)bf.Deserialize(file);
                file.Close();
                return data;
            }
            else
                return fallback;
        } catch (Exception ex) {
            // Can occur when save state formats are obsolete.
            Debug.Log(ex);
            return fallback;
        }
    }
}
