using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckInstanceInventory : MonoBehaviour
{
    private void Awake()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (PersistObject.InventoryInstance != null)
        {
            if (scene.name == "MainMenu"||scene.name == "CutsceneBossIntro")
            {
                Destroy(PersistObject.InventoryInstance.gameObject);
            }
            
        }

    }
}
