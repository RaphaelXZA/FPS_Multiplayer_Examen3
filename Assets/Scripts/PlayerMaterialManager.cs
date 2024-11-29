using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaterialManager : MonoBehaviour
{
    public static PlayerMaterialManager Instance { get; private set; }

    public Material[] availableMaterials;
    private HashSet<int> usedMaterialIndices = new HashSet<int>();

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

    public int GetUnusedMaterialIndex()
    {
        List<int> unusedIndices = new List<int>();

        for (int i = 0; i < availableMaterials.Length; i++)
        {
            if (!usedMaterialIndices.Contains(i))
            {
                unusedIndices.Add(i);
            }
        }

        int randomIndex = unusedIndices[Random.Range(0, unusedIndices.Count)];
        usedMaterialIndices.Add(randomIndex);
        return randomIndex;
    }

    public void ReleaseMaterialIndex(int index)
    {
        usedMaterialIndices.Remove(index);
    }

    public Material GetMaterialByIndex(int index)
    {
        return availableMaterials[index];
    }
}
