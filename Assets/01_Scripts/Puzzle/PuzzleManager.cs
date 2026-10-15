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
            Debug.Log("Código incorrecto. Intentos restantes: " + (maxAttempts - attempts));
        }

        return valid;
    }

    // WIP: sistema de pistas, todavia no esta listo para Dev.
    public string GetHint(int level)
    {
        return level <= 1 ? "El codigo tiene 4 digitos." : "Empieza por 1.";
    }
}
