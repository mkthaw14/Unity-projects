using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public Transform levelPrefab;
    public Transform spawnZone;
    public Transform[] LevelPieces;
    public Material m;
    public Color c;
    public string FarestDist;

    private NavMeshSurface surface;
    private Transform bestFarestSpot;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        surface = GetComponent<NavMeshSurface>();
    }

    public void SpawnLevels()
    {
        Transform levelHolder = Instantiate(levelPrefab, transform.position, Quaternion.identity);
        EmptySpace[] spaces = levelHolder.GetComponentsInChildren<EmptySpace>();

        for (int x = 0; x < spaces.Length; x++)
        {
            int random = Random.Range(0, LevelPieces.Length);
            int randomRotation = Random.Range(1, 3);

            if (!spaces[x].GetComponent<EmptySpace>().isOccupied)
            {
                Transform levels = Instantiate(LevelPieces[random], spaces[x].transform.position, RandomRotation(randomRotation));
                spaces[x].GetComponent<EmptySpace>().TakeOverPosition(true);
                GameManager.instance.spawnManager.Add(levels.GetComponent<SpawnManager>());
            }
        }

        for (int x = 0; x < spaces.Length; x++)
        {
            Destroy(spaces[x].gameObject);
        }

        surface.BuildNavMesh();
    }


    private Quaternion RandomRotation(int randomNum)
    {
        Quaternion rotation = Quaternion.identity;

        switch (randomNum)
        {
            case 1:
                rotation = Quaternion.Euler(0, 90, 0);
                break;
            case 2:
                rotation = Quaternion.Euler(0, 0, 0);
                break;
        }

        return rotation;
    }
}

public class LevelMaterial 
{
    private Material[] allMaterialInEachLevelPiece;
    private MeshRenderer[] allMeshRendererInEacheLevelPiece;

    public LevelMaterial(int size, MeshRenderer[] meshes)
    {
        allMeshRendererInEacheLevelPiece = new MeshRenderer[size];
        allMeshRendererInEacheLevelPiece = meshes;
    }

    public void GetMaterialFromEachRenderer(Color newMaterial)
    {
        allMaterialInEachLevelPiece = new Material[allMeshRendererInEacheLevelPiece.Length];
        for(int x = 0; x < allMeshRendererInEacheLevelPiece.Length; x++)
        {
            allMaterialInEachLevelPiece[x] = allMeshRendererInEacheLevelPiece[x].material;

            if (allMeshRendererInEacheLevelPiece[x].gameObject.layer == 8 || allMeshRendererInEacheLevelPiece[x].gameObject.layer == 12)
            {
                allMaterialInEachLevelPiece[x].color = newMaterial;
            }

        }
    }

    public void AssignMaterials(Material newMaterial)
    {
        for (int x = 0; x < allMaterialInEachLevelPiece.Length; x++)
        {
            if(allMeshRendererInEacheLevelPiece[x].gameObject.layer == 8 || allMeshRendererInEacheLevelPiece[x].gameObject.layer == 12)
            {

            }
            allMaterialInEachLevelPiece[x] = newMaterial;
        }
    }
}


