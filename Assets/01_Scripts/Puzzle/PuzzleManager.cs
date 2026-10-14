using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private ChestController chest;

    public bool ValidateCode(string code)
    {
        bool valid = code == correctCode;
        if (valid)
        {
            Debug.Log("Código correcto.");
            if (chest != null)
            {
                chest.Unlock();
            }
        }
        else
        {
            Debug.Log("Código incorrecto.");
        }

        return valid;
    }
}
