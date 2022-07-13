using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class SaveLoadSystem 
{
    private readonly string filePath = Application.persistentDataPath + " /MySaveData.dat";

    public void SaveGame()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(filePath);
        SaveData data = new SaveData
        {
            levelProgress = GameManager.instance.maxSceneCount
        };
        bf.Serialize(file, data);
        file.Close();

        Debug.Log("Data is saved");
        Debug.Log(filePath);
    }

    public void LoadGame()
    {
        FileStream file = null;
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            file = File.Open(filePath, FileMode.Open);

            SaveData data = (SaveData)bf.Deserialize(file);

            GameManager.instance.maxSceneCount = data.levelProgress;
        }
        catch(Exception e)
        {
            if(e != null) { }
        }
    }

    public void SaveCameraSetting(CameraSetting cameraSetting)
    {
        SaveData data = new SaveData();

        PlayerPrefs.SetFloat(data.saveKey1, cameraSetting.x_rotateSpeed);
        PlayerPrefs.SetFloat(data.saveKey2, cameraSetting.y_rotateSpeed);
        PlayerPrefs.SetInt(data.saveKey3, data.ConvertToInt(cameraSetting.invertX));
        PlayerPrefs.SetInt(data.saveKey4, data.ConvertToInt(cameraSetting.invertY));

        PlayerPrefs.Save();
    }

    public void LoadCameraSetting(ref CameraSetting cameraSetting)
    {
        SaveData data = new SaveData();

        if (PlayerPrefs.HasKey(data.saveKey1))
        {
            cameraSetting.x_rotateSpeed = PlayerPrefs.GetFloat(data.saveKey1);
            cameraSetting.y_rotateSpeed = PlayerPrefs.GetFloat(data.saveKey2);
            cameraSetting.invertX = data.ConvertToBool(PlayerPrefs.GetInt(data.saveKey3));
            cameraSetting.invertY = data.ConvertToBool(PlayerPrefs.GetInt(data.saveKey4));
        }
        else
        {
            Debug.LogError("There is no save data for camera setting");
        }
    }
}

[System.Serializable]
public class SaveData
{
    public int levelProgress;

    //CameraSettings
    //Variables order must be the same order of variables in SetDefaultValues() of the cameraSetting
    public string saveKey1 = "X";
    public string saveKey2 = "Y";
    public string saveKey3 = "invertX";
    public string saveKey4 = "invertY";

    public int ConvertToInt(bool value)
    {
        int intValue = 0;

        if (value)
            intValue = 1;

        return intValue;
    }

    public bool ConvertToBool(int value)
    {
        bool bValue = false;

        if (value == 1)
            bValue = true;

        return bValue;
    }
}

