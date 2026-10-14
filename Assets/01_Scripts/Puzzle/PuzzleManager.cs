using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private string correctCode = "1234";

    public bool ValidateCode(string code)
    {
        return code == correctCode;
    }
}
