using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaterialManager : MonoBehaviour
{
    public static PlayerMaterialManager Instance { get; private set; }

    public Material[] availableMaterials;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Material GetRandomMaterial()
    {
        if (availableMaterials == null || availableMaterials.Length == 0)
        {
            Debug.LogError("¡No materials assigned in PlayerMaterialManager!");
            return null;
        }
        return availableMaterials[Random.Range(0, availableMaterials.Length)];
    }
}
