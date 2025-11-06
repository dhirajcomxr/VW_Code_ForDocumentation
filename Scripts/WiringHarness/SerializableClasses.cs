using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node 
{
    public bool toInclude;
    
    public string NodeSTR;
    public double nodeCrossSection;
    public string nodeColorCode;

    public string Node2STR;
    public double node2CrossSection;
    public string node2ColorCode;

    public string Node3STR;
    public double node3CrossSection;
    public string node3ColorCode;

    public string Node4STR;

    public int endPointPinNum;
    public string endPointDesignation;
    public string endComponentDesignation;
    public GameObject model;
    
    public string details;
    public GameObject endPointObj;
    public GameObject endPointPin;
    public GameObject endComponentObj;
    public ExtendedWireComponent ExtraComponent;
}

[System.Serializable]
public class Wire 
{
    public int wireNumber;
    public GameObject pin;
    public double crossSection;
    public string colorCode;
    public Node[] nodes;
    
}

[System.Serializable]
public class ExtendedWireComponent
{
    public double crossSection;
    public string colorCode;
    public string node;
    public string endConnectorDesignation;
    public int pinNumber;
    public string extraConnectorDesignation;
    public string extraComponentDesignation;
    public int extraPinNumber;
}

[System.Serializable]
public class EndConnectorAndPin
{
    public bool include;
    public GameObject endConnector;
    public int endPinNum;

    public GameObject startConnector;
    public int startPinNum;

    public Wire ReverseWire;
}

public class SerializableClasses
{
    
}
