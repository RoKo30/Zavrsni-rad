using UnityEngine;

public class PlayerSpawn : MonoBehaviour {
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject cam;
    void Start() {
        var a = transform.position;
        Instantiate(cam, new Vector3(a.x + 5, a.y + 8, a.z), Quaternion.identity);
        Instantiate(player, a , Quaternion.identity);

    }


}
