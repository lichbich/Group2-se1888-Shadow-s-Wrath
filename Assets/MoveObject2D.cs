using UnityEngine;
using System.Collections;
public class MoveObject2D : MonoBehaviour
{

    void Start()
    {
        // Gọi coroutine khi bắt đầu game
        StartCoroutine(MoveUp());
    }

    // Coroutine: giúp làm việc theo thời gian (vd: di chuyển dần)
    IEnumerator MoveUp()
    {
        // Lặp 100 lần
        for (int i = 0; i < 100; i++)
        {
            // Di chuyển object lên trên mỗi frame
            transform.position += new Vector3(0, 0.05f, 0);

            // Chờ 0.02 giây rồi mới lặp tiếp (để nhìn thấy chuyển động mượt)
            yield return new WaitForSeconds(0.02f);
        }

        // In ra khi xong
        Debug.Log("Di chuyển xong!");
    }
}
