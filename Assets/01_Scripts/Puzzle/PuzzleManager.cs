using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private ChestController chest;
    [SerializeField] private int maxAttempts = 3;

    private int attempts;

    public bool ValidateCode(string code)
    {
        if (attempts >= maxAttempts)
        {
            Debug.Log("Sin intentos disponibles.");
            return false;
        }

        attempts++;
        bool valid = code.Trim() == correctCode;
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
            Debug.Log("Código incorrecto. Intentos restantes: " + (maxAttempts - attempts));
        }

        return valid;
    }
}
