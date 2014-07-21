using UnityEngine;
using System.Collections;
using POSH.sys;
using System.Collections.Generic;

public class ChargingBehaviour : MonoBehaviour {

    public Transform chargerLocation;
    public int chargingDistance;
    public int chargingRate;
    
    
    public float energy;
    public int max_energy;
    public float energyUsage;

    public bool alive=false;
   
   
    private bool charge = false;  // Trigger to determine if the Behaviour should charge
    public Dictionary<string,RampActivation> ergos;

	// Use this for initialization
	void Start () {
        alive = true;
        
	}

    public bool WantsToCharge()
    {
        return ergos["charging"].Challenge(ergos);
    }


    public bool NeedToCharge()
    {
        if (energy < max_energy * 0.30)
            return true;

        return false;
    }

    public void Charging()
    {
        charge = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (alive)
        {
            if (charge)
            {
                energy += chargingRate;
                charge = false;
            }
            else
                energy -= energyUsage;
            
            

            if (energy > max_energy)
                ergos["charging"].ReachedGoal();

            if (energy <= 0)
                alive = false;
            
           
        }

        

        
	}
}
