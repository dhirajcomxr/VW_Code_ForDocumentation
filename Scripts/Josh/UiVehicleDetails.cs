using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UiVehicleDetails : MonoBehaviour
{
    public VehicleDetailsScreen details;
    [SerializeField] AppApiManager.VehicleDetails currentVehicleDetails;
    [System.Serializable]
    public struct VehicleDetailsScreen
    {
        public Text vin, EngineNumber,variantName,
            engineCode,gearboxCode,ac, modelCode,modelName, saleDate, productionDate,
            deliveryDate, dealershipName, dealershipCode,
         //   customerState, customerCity, commissionNumber,
            colorDesc, colorCode;

    }
    public AppApiManager.VehicleDetails GetCurrentVehicleDetails()
    {
        return currentVehicleDetails;
    }
    public void Load(AppApiManager.VehicleDetails vd)
    {
        if (vd.VIN.Length > 0)
            currentVehicleDetails = vd;
        details.vin.text = vd.VIN;
        details.variantName.text = vd.VariantName;
        details.saleDate.text = vd.SaleDate;
        details.modelCode.text = vd.ModelCode;
        details.modelName.text = vd.ModelName;
        details.EngineNumber.text = vd.EngineNumber;
        details.engineCode.text = vd.Engine;
        details.gearboxCode.text = vd.Transmission;
        details.productionDate.text = vd.ProductionDate;
        details.deliveryDate.text = vd.DeliveryDate;
        details.dealershipName.text = vd.DealershipName;
        details.dealershipCode.text = vd.DealershipCode;

        details.ac.text = vd.HVAC;
        details.colorDesc.text = vd.ColorDesc;
        details.colorCode.text = vd.ColorCode;
    }
  public void Load(AppApiManager.ServerData data)
    {
        Load(data.vehicle_details);
    }
  
}
