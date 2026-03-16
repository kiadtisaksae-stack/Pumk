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

    void Start()
    {
        roomServiceButton.gameObject.SetActive(false);
        counter = FindAnyObjectByType<Counter>();
        room = GetComponent<Room>();
    }

    // ─────────────────────────────────────────────
    //  Entry Point
    // ─────────────────────────────────────────────

    public void StartRequests(GuestAI guest)
    {
        StartCoroutine(ProcessRequests(guest));
    }

    // ─────────────────────────────────────────────
    //  ทุก behavior ส่งผ่าน hooks ของ GuestAI
    // ─────────────────────────────────────────────

    IEnumerator ProcessRequests(GuestAI guest)
    {
        FrankenGuest franken = guest as FrankenGuest;
        MummyGuest mummy = guest as MummyGuest;

        for (int slotIndex = 0; slotIndex < listService.Count; slotIndex++)
        {
            // Mummy: เช็กว่า slot นี้มี forced service หรือเปล่า
            ItemSO service = listService[slotIndex];
            if (mummy != null)
            {
                ItemSO forced = mummy.GetForcedServiceForSlot(slotIndex);
                if (forced != null) service = forced;
            }

            isSuccess = false;
            guest.OnServiceStart(service);
            ServicePopUp(service, roomServiceButton);
            guest.currentService = service;

            Coroutine decayCoroutine = guest.StartDecayCoroutine();

            while (true)
            {
                if (isSuccess)
                {
                    guest.isDecaying = false;
                    if (decayCoroutine != null) guest.StopCoroutine(decayCoroutine);
                    break;
                }

                if (guest.isExit)
                {
                    roomServiceButton.gameObject.SetActive(false);
                    room.RoomData.isUnAvailable = false;
                    if (decayCoroutine != null) guest.StopCoroutine(decayCoroutine);
                    StopAllCoroutines();
                    yield break;
                }

                yield return null;
            }

            roomServiceButton.gameObject.SetActive(false);
            guest.currentService = null;

            if (!isSuccess)
                guest.OnServiceFail(service);
            else
                guest.OnServiceSuccess(service);

            // ── Cooldown + Franken sleepwalk window ──
            // Franken สุ่ม sleepwalk ระหว่าง cooldown ก่อน service ถัดไป
            if (franken != null && slotIndex < listService.Count - 1)
            {
                bool sleeping = franken.TrySleepwalk();
                if (sleeping)
                {
                    // รอให้ตื่นก่อนค่อยนับ cooldown
                    yield return new WaitUntil(() => !franken.IsSleepwalking);
                }
            }

            yield return new WaitForSeconds(serviceCooldown);
        }

        guest.OnAllServicesComplete(counter);
        room.DirtyRoom();
    }

    // ─────────────────────────────────────────────
    //  Request Validation
    // ─────────────────────────────────────────────

    public bool RequestCheck(ItemSO service, MoveHandleAI actor)
    {
        List<ItemSO> actorInventory = null;
        Player playerActor = actor as Player;
        Employee employeeActor = actor as Employee;

        if (playerActor != null) actorInventory = playerActor.inventory;
        else if (employeeActor != null) actorInventory = employeeActor.inventory;

        if (actorInventory == null || service == null) return false;

        ItemSO found = actorInventory.Find(x => x == service);
        if (found == null)
        {
            Debug.Log("<color=red>ไม่มีไอเทมที่ลูกค้าต้องการ</color>");
            return false;
        }

        if (playerActor != null) playerActor.RemoveItem(found);
        else if (employeeActor != null) employeeActor.RemoveItem(found);

        isSuccess = true;
        Debug.Log($"<color=cyan>ส่ง {found.itemName} สำเร็จ</color>");
        return true;
    }

    // ─────────────────────────────────────────────
    //  Setup Helpers
    // ─────────────────────────────────────────────

    public void ServicePopUp(ItemSO service, Button serviceButton)
    {
        serviceButton.gameObject.SetActive(true);
        serviceButton.image.sprite = service.itemIcon;
    }

    public void ServiceSetUp(List<ItemSO> allService, int serviceCount)
    {
        FindLuggageService(allService);

        List<ItemSO> randomService = new List<ItemSO>(allService);
        ShuffleList(randomService);
        listService.AddRange(randomService);

        if (listService.Count > serviceCount)
            listService.RemoveRange(serviceCount, listService.Count - serviceCount);

        Debug.Log($"<color=green>Set Up Service เรียบร้อย ({listService.Count} รายการ)</color>");
    }

    void ShuffleList(List<ItemSO> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void FindLuggageService(List<ItemSO> allService)
    {
        for (int i = 0; i < allService.Count; i++)
        {
            if (allService[i].requiredForService == ServiceRequestType.DeliveryLuggage)
            {
                listService.Add(allService[i]);
                allService.RemoveAt(i);
                break;
            }
        }
    }
}