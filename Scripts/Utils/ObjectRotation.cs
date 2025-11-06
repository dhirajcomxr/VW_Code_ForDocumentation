using UnityEngine;

public class ObjectRotation : MonoBehaviour {

    public Vector3 rotAmount;
    public bool isAssembly = false;
    Vector3 scale;

    // Start is called before the first frame update
    void Start() {
        scale = transform.localScale;
    }

    private void OnEnable() {
        isAssembly = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            if (gameObject.tag.ToLower() == "rotationindicator" && isAssembly)
            {
                transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
            }
            else
            {
                transform.localScale = scale;
            }
            transform.Rotate((isAssembly ? -1f : 1f) * rotAmount * Time.deltaTime, Space.Self);
        }
    }
}
