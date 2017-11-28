using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Helpers;
using Assets.Scripts.LS;
using Assets.Scripts.Managers;
using Assets.Scripts.Objects;
using UnityEngine;

public class CreateGame : MonoBehaviour
{
    // Use this for initialization
	void Start ()
	{
	    NewGame();
	}

    public void NewGame()
    {
        var ssName = StarNames.GetUniqueStarName(SSManager.Instance);

        var ssm = SolarSystem.GenerateSolarSystem(ssName, ssName);

        ssm.Init();
    }
}
