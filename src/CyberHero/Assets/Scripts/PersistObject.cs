using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistObject : MonoBehaviour
{
    static GameObject inventoryInstance;
    [SerializeField] private GameObject inventory;

    public static GameObject InventoryInstance { get => inventoryInstance; set => inventoryInstance = value; }

    private void Awake()
    {
        if (InventoryInstance != null)
        {
            Destroy(inventory);
            inventory = InventoryInstance;
        }
        else
        {
            InventoryInstance = inventory;
            inventory.transform.SetParent(null);
            DontDestroyOnLoad(inventory);
        }
    }

    // private void Update()
    // {
    //     Scene scene = SceneManager.GetActiveScene();
    //     Debug.Log(scene.name);

    //     if (scene.name == "CutsceneBossIntro")
    //     {
    //         Destroy(InventoryInstance.gameObject);
    //         Destroy(inventory.gameObject);
    //     }

    //     if (scene.name == "MainMenu"
    //         || scene.name == "LevelSelector"
    //         || scene.name == "CutsceneIntro"
    //         || scene.name == "CutsceneAlexDialogue")
    //     {
    //         Destroy(InventoryInstance.gameObject);
    //         Destroy(inventory.gameObject);
    //     }
    // }
}
