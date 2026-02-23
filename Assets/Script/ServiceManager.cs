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
    public bool isSuccess;
    public Counter counter;

    
    private Room room;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomServiceButton.gameObject.SetActive(false);
        counter = FindAnyObjectByType<Counter>();
        room = GetComponent<Room>();
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
            ServicePopUp(service, roomServiceButton);
            guest.currentService = service; //ใช้ service ตัวนี้

            float timer = 0f;
            isSuccess = false;
            guest.isDecaying = true;
            guest.StartDelay(guest.isDecaying);

            while (true)
            {
                timer += Time.deltaTime;

                // เช็คเงื่อนไข (ต้องเช็คทุกเฟรม)
                if (isSuccess == true)
                {
                    Debug.Log("ส่งของสำเร็จ! (ใช้เวลา " + timer.ToString("F1") + " วิ)");
                    guest.isDecaying = false;
                    
                    guest.StopAllCoroutines();
                    guest.heart = 5f;
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
        guest.CheckOut(counter.interactObjData);
        room.DirtyRoom();
        StopAllCoroutines();
    }

    public bool RequestCheck(ItemSO service , MoveHandleAI actor)
    {
        List<ItemSO> actorInventory = null;
        Player playerActor = actor as Player;
        Employee employeeActor = actor as Employee;

        if (playerActor != null) actorInventory = playerActor.inventory;
        else if (employeeActor != null) actorInventory = employeeActor.inventory;

        if (actorInventory == null || service == null) return false;

        // ค้นหาไอเทม
        ItemSO itemToDeliver = actorInventory.Find(x => x == service);

        if (itemToDeliver != null)
        {
            // ส่งสำเร็จ! ให้ลบผ่าน Method ของ Actor เพื่ออัปเดต UI
            if (playerActor != null) playerActor.RemoveItem(itemToDeliver);
            else if (employeeActor != null) employeeActor.RemoveItem(itemToDeliver);

            isSuccess = true;
            Debug.Log($"<color=cyan>ส่ง {itemToDeliver.itemName} สำเร็จและอัปเดต UI แล้ว</color>");
            return true;
        }

        Debug.Log("<color=red>ไม่มีไอเทมที่ลูกค้าต้องการในตัว!</color>");
        return false;
    }

    public void ServicePopUp(ItemSO service , Button serviceButton)
    {
        serviceButton.gameObject.SetActive(true);
        serviceButton.image.sprite = service.itemIcon;
    }

    public void ServiceSetUp(List<ItemSO> allService , int serviceCount) //Set Up service ก่อนใช้งาน
    {
        //หา Luggage ก่อน
        FindLuggageService(allService);

        List<ItemSO> randomService = new List<ItemSO>(allService);
        ShuffleList(randomService);
        listService.AddRange(randomService);
        if (listService.Count > serviceCount)
        {
            listService.RemoveAt(serviceCount);
        }
        Debug.Log($"<color=green>Set Up รายการService เรียบร้อย</color>");
    }

    void ShuffleList(List<ItemSO> allService) //สุ่มลำดับ service ที่เหลือ
    {
        for (int i = 0; i < allService.Count; i++)
        {
            ItemSO service = allService[i];
            int random = Random.Range(i, allService.Count);
            allService[i] = allService[random];
            allService[random] = service;
        }
    }

    public void FindLuggageService(List<ItemSO> allService) //หาสัมภาระเสมอ
    {
        foreach (ItemSO service in allService)
        {
            if (service.requiredForService == ServiceRequestType.DeliveryLuggage)
            {
                listService.Add(service);
                allService.Remove(service);
                break;
            }
        }
    }
}
