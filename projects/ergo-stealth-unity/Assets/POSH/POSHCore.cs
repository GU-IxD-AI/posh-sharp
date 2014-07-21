using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using POSH.sys;
using POSH.unity;



namespace POSH
{
	public class POSHCore : POSHController
	{
        void Awake()
        {
            InitPOSH();
            
         }

        void Start()
        {
            
        }

        void Update()
        {
            Loom.QueueOnMainThread(() =>
            {
                if (Application.isPlaying)
                    RunPOSH();
            });
        }

    }
}
