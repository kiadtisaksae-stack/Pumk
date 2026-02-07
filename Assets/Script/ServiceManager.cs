using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServiceManager : MonoBehaviour
{
    public List<ItemSO> listService = new List<ItemSO>();
    public float serviceCooldown = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ProcessRequests()
    {
        foreach (ItemSO service in listService)
        {
            Debug.Log("ลูกค้าขอ: " + service.name);

            yield return new WaitForSeconds(service.deliverTime);
            Debug.Log(service.name + " หมดเวลา ");

            yield return new WaitForSeconds(serviceCooldown);
        }

    }

    public void ServicePopUp(ItemSO service)
    {
        //serviceButton.image = service.itemIcon;
    }

    public void ServiceSetUp(List<ItemSO> allService)
    {
        //หา Luggage ก่อน
        FindLuggageService(allService);

        List<ItemSO> randomService = new List<ItemSO>(allService);
        ShuffleList(randomService);
        listService.AddRange(randomService);
        Debug.Log($"<color=green>Set Up รายการService เรียบร้อย</color>");
    }
    void ShuffleList(List<ItemSO> allService)
    {
        for (int i = 0; i < allService.Count; i++)
        {
            ItemSO service = allService[i];
            int random = Random.Range(i, allService.Count);
            allService[i] = allService[random];
            allService[random] = service;
        }
    }

    public void FindLuggageService(List<ItemSO> allService)
    {
        foreach (ItemSO service in allService)
        {
            if (service.requiredForService == ServiceRequestType.DeliveryLuggage)
            {
                listService.Add(service);

                //allService.Remove(service);
                break;
            }
        }
    }
}
