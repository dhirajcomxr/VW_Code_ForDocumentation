using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car3d : MonoBehaviour
{
    public string carName = "";
    public GameObject car;
    public string catalogAddressDEV, catalogAddressSTAG, catalogAddressPROD;
    public GameObject harness;
    public GameObject[] sections;
    public List<CarModules3D> modules, curModules;
    public List<AssetLoader.NameAddress> nameAddresses;
    [System.Serializable]
   public class CarModules3D
    {
        public string name;
        public string address;
        public GameObject[] objects;
        public int section;
        public bool isActive = true, displayModule = true;
        public int moduleId;
    }
   
}
