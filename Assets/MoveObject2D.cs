using UnityEngine;
using System.Collections;
public class MoveObject2D : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(MoveUp());
    }

    IEnumerator MoveUp()
    {

        for (int i = 0; i < 100; i++)
        {

            transform.position += new Vector3(0, 0.05f, 0);


            yield return new WaitForSeconds(0.02f);
        }


        Debug.Log("Di chuyển xong!");
    }
}
