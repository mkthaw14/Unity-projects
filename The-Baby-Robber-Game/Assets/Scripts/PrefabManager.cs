using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager instance;

	[HideInInspector]
    public PrefabModels prefabModel;


    private void Awake()
    {
        instance = this;
		prefabModel = GetComponent<PrefabModels>();
    }

	public int GetNumberOfColor()
    {
		return prefabModel.prefabCharacterModels.Length;
    }

	public Character GetCharacterPrefabModels(string name)
    {
		Character model = null;

		switch (name)
		{
			case "Blue":
				model = prefabModel.prefabCharacterModels[0];
				break;
			case "Red":
				model = prefabModel.prefabCharacterModels[1];
				break;
			case "Yellow":
				model = prefabModel.prefabCharacterModels[2];
				break;
			case "Green":
				model = prefabModel.prefabCharacterModels[3];
				break;
			case "Black":
				model = prefabModel.prefabCharacterModels[4];
				break;
		}
		
		return model;
	}
		

	public GameObject GetHelicopterPrefab(string name)
    {
		GameObject chopper = null;

		switch (name)
		{
			case "Blue":
				chopper = prefabModel.helicopters[0];
				break;
			case "Red":
				chopper = prefabModel.helicopters[1];
				break;
			case "Yellow":
				chopper = prefabModel.helicopters[2];
				break;
			case "Green":
				chopper = prefabModel.helicopters[3];
				break;
			case "Black":
				chopper = prefabModel.helicopters[4];
				break;
		}

		return chopper;
    }

	public string GetAvailibleFactionName(int index)
    {
		string name = "";
        switch (index)
        {
			case 0:
				name = "Blue";
				break;
			case 1:
				name = "Red";
				break;
			case 2:
				name = "Yellow";
				break;
			case 3:
				name = "Green";
				break;
			case 4:
				name = "Black";
				break;
        }

		return name;
    }
}
