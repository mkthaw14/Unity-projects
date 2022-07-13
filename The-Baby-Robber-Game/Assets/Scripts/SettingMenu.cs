using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingMenu : UIMenu
{
	[SerializeField]
	GameObject[] selectableObjects;
	[SerializeField]
	Text[] cameraSettingValues;

    public override void SetUp()
    {
        base.SetUp();
		LoadGameSettings();
    }

    public override void Tick()
    {
        base.Tick();
		CameraSettingAdjusting();
	}

    private void OnDisable()
    {
		SaveGameSetting();
    }



    private void CameraSettingAdjusting()
	{
		if (EventSystem.current.currentSelectedGameObject == selectableObjects[0])
		{
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				ChangeCameraSensitivity("X", -0.1f);
			}
			else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				ChangeCameraSensitivity("X", 0.1f);
			}
		}
		else if (EventSystem.current.currentSelectedGameObject == selectableObjects[1])
		{
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				ChangeCameraSensitivity("Y", -0.1f);
			}
			else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				ChangeCameraSensitivity("Y", 0.1f);
			}
		}
		else if (EventSystem.current.currentSelectedGameObject == selectableObjects[2])
		{
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				ChangeCameraAxis("X", false);
			}
			else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				ChangeCameraAxis("X", true);
			}
		}
		else if (EventSystem.current.currentSelectedGameObject == selectableObjects[3])
		{
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
			{
				ChangeCameraAxis("Y", false);
			}
			else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				ChangeCameraAxis("Y", true);
			}
		}
	}

	private void ChangeCameraSensitivity(string axis, float amount)
	{
		if (axis == "X")
		{
			GameManager.instance.cameraSetting.x_rotateSpeed += amount;
			GameManager.instance.cameraSetting.x_rotateSpeed = Mathf.Clamp(GameManager.instance.cameraSetting.x_rotateSpeed, 1, 6);
			ShowCameraSettingValues(0);
		}
		else if (axis == "Y")
		{
			GameManager.instance.cameraSetting.y_rotateSpeed += amount;
			GameManager.instance.cameraSetting.y_rotateSpeed = Mathf.Clamp(GameManager.instance.cameraSetting.y_rotateSpeed, 1, 6);
			ShowCameraSettingValues(1);
		}
	}

	private void ShowCameraSettingValues(int index)
	{
		switch (index)
		{
			case 0:
				cameraSettingValues[0].text = GameManager.instance.cameraSetting.x_rotateSpeed + "";
				break;
			case 1:
				cameraSettingValues[1].text = GameManager.instance.cameraSetting.y_rotateSpeed + "";
				break;
			case 2:
				cameraSettingValues[2].text = GameManager.instance.cameraSetting.invertX + "";
				break;
			case 3:
				cameraSettingValues[3].text = GameManager.instance.cameraSetting.invertY + "";
				break;
			case 4:
				cameraSettingValues[0].text = GameManager.instance.cameraSetting.x_rotateSpeed + "";
				cameraSettingValues[1].text = GameManager.instance.cameraSetting.y_rotateSpeed + "";
				cameraSettingValues[2].text = GameManager.instance.cameraSetting.invertX + "";
				cameraSettingValues[3].text = GameManager.instance.cameraSetting.invertY + "";
				break;
		}
	}

	private void ChangeCameraAxis(string axis, bool value)
	{
		if (axis == "X")
		{
			GameManager.instance.cameraSetting.invertX = value;
			ShowCameraSettingValues(2);
		}
		else if (axis == "Y")
		{
			GameManager.instance.cameraSetting.invertY = value;
			ShowCameraSettingValues(3);
		}
	}

	private void SaveGameSetting()
    {
		GameManager.instance.SaveGameSetting();
    }

	public void LoadGameSettings()
	{
		GameManager.instance.LoadGameSetting();
		ShowCameraSettingValues(4);
	}

	public void ResetCameraSetting()
	{
		GameManager.instance.cameraSetting.SetDefaultValues();
		ShowCameraSettingValues(4);
	}
}
