using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private string secretCode = "1234";
    [SerializeField] private ChestController chest;

    public bool ValidateCode(string input)
    {
        if (input != secretCode)
        {
            Debug.Log("Código incorrecto.");
            return false;
        }

        if (chest != null)
        {
            chest.Unlock();
        }

        Debug.Log("Código correcto.");
        return true;
    }
}
