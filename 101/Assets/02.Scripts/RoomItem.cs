using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    //외부 접근을 위한 public 변수들
    [HideInInspector] public string roomName = "";
    [HideInInspector] public int connectPlayer = 0;
    [HideInInspector] public int maxPlayer = 0;

    //룸 이름 표시할 Text UI 항목
    public Text textRoomName;
    //룸 접속자 수와 최대 접속자 수를 표시할 Text UI 항목
    public Text textConnectInfo;

    [HideInInspector] public string ReadyState = ""; //레디 상태 표시

    // Start is called before the first frame update
    void Start()
    {

        this.GetComponent<Button>().onClick.AddListener(()=>
        {
            PhotonInit refInit = FindObjectOfType<PhotonInit>();
            if(refInit != null)
            {
                refInit.OnClickRoomItem(roomName);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DispRoomData(bool a_IsOpen)
    {
        if(a_IsOpen == true)
        {
            textRoomName.color = new Color32(0, 0, 0, 255);
            textConnectInfo.color = new Color32(0, 0, 0, 255);
        }
        else
        {
            textRoomName.color = new Color32(0, 0, 255, 255);
            textConnectInfo.color = new Color32(0, 0, 255, 255);
        }

        textRoomName.text = roomName;
        textConnectInfo.text = "(" + connectPlayer.ToString() + "/"
                                   + maxPlayer.ToString() + ")";
    }//public void DispRoomData(bool a_IsOpen)
}
