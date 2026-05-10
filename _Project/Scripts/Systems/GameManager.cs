using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TheUnique.Core.SaveSystem;
using TheUnique.Core.World;
using TheUnique.Core.Environment;

public class GameManager : MonoBehaviour
{
    [Header("Ссылки на системы")]
    public MapGenerator WorldGenerator;
    public PlayerSpawner Spawner;
    public DayNightCycle TimeCycle;
    public GameObject LoadingScreen;

    [Header("База данных префабов")]
    [Tooltip("Сюда в инспекторе перетащи все префабы деревьев/камней, на которых есть скрипт SaveableObject")]
    public List<GameObject> PrefabDatabase;

    private void Start()
    {
        if (LoadingScreen != null) LoadingScreen.SetActive(true);

        bool isLoading = PlayerPrefs.GetInt("IsLoadingSave", 0) == 1;
        
        StartCoroutine(GameInitializationRoutine(isLoading));
    }

    private IEnumerator GameInitializationRoutine(bool isLoading)
    {
        GameSaveData saveData = null;
        int seed;
    
        int width = PlayerPrefs.GetInt("WorldWidth", 50);
        int height = PlayerPrefs.GetInt("WorldHeight", 50);

        if (isLoading)
        {
            saveData = SaveManager.LoadGame();
            if (saveData == null)
            {
                seed = Random.Range(1000, 99999);
            }
            else
            {
                seed = saveData.WorldSeed;
            }
        }
        else
        {
            seed = Random.Range(1000, 99999);
        }

        WorldGenerator.GenerateWithSeed(seed, width, height);

        yield return new WaitUntil(() => WorldGenerator.IsDone);

        if (isLoading && saveData != null)
        {
            ClearDefaultSpawnedObjects();
            foreach (var savedObj in saveData.WorldObjects)
            {
                GameObject prefab = GetPrefabByName(savedObj.PrefabName);
                if (prefab != null) Instantiate(prefab, savedObj.Position, Quaternion.identity);
            }
            Spawner.SpawnPlayerAt(saveData.PlayerPosition);
            if (TimeCycle != null) TimeCycle.TimeOfDay = saveData.TimeOfDay;
        }
        else
        {
            Spawner.SpawnPlayerAfterGeneration();
            if (TimeCycle != null) TimeCycle.TimeOfDay = 0.3f;
        }

        yield return new WaitForSeconds(0.2f);
    
        if (LoadingScreen != null) LoadingScreen.SetActive(false);
    }

    private void ClearDefaultSpawnedObjects()
    {
        SaveableObject[] objects = Object.FindObjectsByType<SaveableObject>(FindObjectsSortMode.None);
        foreach (var obj in objects)
        {
            Destroy(obj.gameObject);
        }
    }

    public void SaveCurrentGame()
    {
        GameSaveData data = new GameSaveData();
        
        data.WorldSeed = WorldGenerator.CurrentSeed;
        if (TimeCycle != null) data.TimeOfDay = TimeCycle.TimeOfDay;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) data.PlayerPosition = player.transform.position;

        SaveableObject[] currentObjects = Object.FindObjectsByType<SaveableObject>(FindObjectsSortMode.None);
        foreach (var obj in currentObjects)
        {
            SavedObject sObj = new SavedObject();
            sObj.PrefabName = obj.PrefabName;
            sObj.Position = obj.transform.position;
            data.WorldObjects.Add(sObj);
        }

        SaveManager.SaveGame(data);
        Debug.Log("Прогресс успешно сохранен!");
    }

    private GameObject GetPrefabByName(string prefabName)
    {
        foreach (var prefab in PrefabDatabase)
        {
            SaveableObject so = prefab.GetComponent<SaveableObject>();
            if (prefab.name == prefabName || (so != null && so.PrefabName == prefabName))
            {
                return prefab;
            }
        }
        Debug.LogWarning("Префаб с именем " + prefabName + " не найден в базе GameManager!");
        return null;
    }
}