using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServiceManager : MonoBehaviour
{
    public List<ItemSO> listService = new List<ItemSO>();
    public Button roomServiceButton;
    public float serviceCooldown = 5f;


    public bool isPlayerinRange;
    public Counter counter;

    public bool isSuccess;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomServiceButton.gameObject.SetActive(false);
        counter = FindAnyObjectByType<Counter>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartRequests(GuestAI guest)
    {
        StartCoroutine(ProcessRequests(guest));

    }



    IEnumerator ProcessRequests(GuestAI guest)
    {
        foreach (ItemSO service in listService)
        {
            Debug.Log("ลูกค้าขอ: " + service.name);
            guest.enabled = true;
            ServicePopUp(service, roomServiceButton);
            guest.currentService = service; //ใช้ service ตัวนี้

            float timer = 0f;
            isSuccess = false;
            guest.isDecaying = true;

            //if (isSuccess)
            //{
            //    guest.isDecaying = false;
            //    roomServiceButton.gameObject.SetActive(false);
            //    guest.currentService = null;
            //    Debug.Log(service.name + " ส่งสำเร็จ");
            //    guest.servicePoint++;
            //}

            while (true)
            {
                timer += Time.deltaTime;

                // เช็คเงื่อนไข (ต้องเช็คทุกเฟรม)
                if (isSuccess == true)
                {
                    Debug.Log("ส่งของสำเร็จ! (ใช้เวลา " + timer.ToString("F1") + " วิ)");
                    guest.isDecaying = false;
                    guest.enabled = false;
                    break; // <--- พระเอกของเรา! สั่งให้ออกจาก loop เวลาทันที (ไม่ต้องรอจนหมดเวลา)
                }

                if (guest.isExit)
                {
                    Debug.Log("หยุดการ Request ทั้งหมด!!!");
                    roomServiceButton.gameObject.SetActive(false);
                    StopAllCoroutines();
                    break;
                }

                yield return null; // พัก 1 เฟรม แล้วกลับมาเช็คใหม่
            }

            if (!isSuccess)
            {
                // ถ้าหลุด loop มาโดยที่ isSuccess ยังเป็น false แปลว่าหมดเวลา
                roomServiceButton.gameObject.SetActive(false);
                guest.currentService = null;
                Debug.Log(service.name + " หมดเวลา! (ลูกค้าโกรธ)");
                guest.servicePoint--;
            }
            else
            {
                // ถ้าสำเร็จ (อาจจะให้คะแนน หรือเล่นเสียงตรงนี้)
                roomServiceButton.gameObject.SetActive(false);
                guest.currentService = null;
                Debug.Log(service.name + " ส่งสำเร็จ");
                guest.servicePoint++;
                //yield return service; // (ถ้าโค้ดเดิมของคุณต้องการ return ค่านี้)
            }

            // 3. พัก Cooldown ก่อนไป service ต่อไป
            yield return new WaitForSeconds(serviceCooldown);
        }
        Debug.Log("Service หมดแล้ว! (Check Out All)");
        guest.enabled = true;
        guest.CheckOut(counter.interactObjData);
        StopAllCoroutines();
    }

    public bool RequestCheck(ItemSO service , List<ItemSO> inventory)
    {
        //ฟังก์ชันสำหรับ check ของใน inven ที่มาถึงห้องแล้วว่ามีของที่ลูกค้า request ไหม
        foreach (var item in inventory)
        {
            if (item == service)
            {
                inventory.Remove(item);
                Debug.Log("ลบไอเท็ม");
                isSuccess = true;
                break;
            }
        }

        return isSuccess;
        // ใช้ไปก่อน
    }

    public void ServicePopUp(ItemSO service , Button serviceButton)
    {
        serviceButton.gameObject.SetActive(true);
        serviceButton.image.sprite = service.itemIcon;
    }

    public void ServiceSetUp(List<ItemSO> allService) //Set Up service ก่อนใช้งาน
    {
        //หา Luggage ก่อน
        FindLuggageService(allService);

        List<ItemSO> randomService = new List<ItemSO>(allService);
        ShuffleList(randomService);
        listService.AddRange(randomService);
        Debug.Log($"<color=green>Set Up รายการService เรียบร้อย</color>");
    }

    void ShuffleList(List<ItemSO> allService) //สุ่มลำดับ service ที่เหลือ
    {
        //for (int i = 0; i < allService.Count; i++)
        //{
        //    ItemSO service = allService[i];
        //    int random = Random.Range(i, allService.Count);
        //    allService[i] = allService[random];
        //    allService[random] = service;
        //}
    }

    public void FindLuggageService(List<ItemSO> allService) //หาสัมภาระเสมอ
    {
        foreach (ItemSO service in allService)
        {
            if (service.requiredForService == ServiceRequestType.DeliveryLuggage && allService == null)
            {
                listService.Add(service);
                allService.Remove(service);
                break;
            }
        }
    }
}
