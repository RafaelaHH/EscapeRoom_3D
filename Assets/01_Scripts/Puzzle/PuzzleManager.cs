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
            if (chest == null)
            {
                Debug.LogError("PuzzleManager: no hay cofre asignado, el código correcto no desbloquea nada.");
                return false;
            }

            Debug.Log("Código correcto.");
            chest.Unlock();
        }
        else
        {
            Debug.Log("Código incorrecto.");
        }

        return valid;
    }
}
