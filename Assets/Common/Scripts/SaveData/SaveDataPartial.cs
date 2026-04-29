using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

public partial class SaveData
{
    public static void Save<T>(string path, T data) where T : class
    {
        try
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new VersionBinder();
                using (Stream crypto = new CryptoStream(stream, 
                    GetCryptoProvider().CreateEncryptor(), CryptoStreamMode.Write))
                    formatter.Serialize(crypto, data);
            }
        }
        catch (System.Exception e) 
        { 
            UnityEngine.Debug.LogWarning(e.ToString()); 
        }
    }

    public static void Load<T>(string path, ref T data) where T : class, new()
    {
        try
        {
            if (File.Exists(path))
            {
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Binder = new VersionBinder();
                    using (Stream crypto = new CryptoStream(stream, 
                        GetCryptoProvider().CreateDecryptor(), CryptoStreamMode.Read))
                        data = (T)formatter.Deserialize(crypto);
                }
            }
        }
        catch (System.Exception e) 
        { 
            UnityEngine.Debug.LogWarning(e.ToString()); 
        }
    }
}