using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int Level { get; private set; }

    private void Awake()
    {
        Level = 1;
    }

    public void NextLevel()
    {
        Level++;
    }
}

