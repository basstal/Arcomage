using UnityEngine;
[ExecuteInEditMode]
public class EditUIMaterial : MonoBehaviour
{
    public Material m_defaultMaterial;
    // Start is called before the first frame update
    void Start()
    {
        if(m_defaultMaterial != null)
        {
            Material _defaultMaterial = Canvas.GetDefaultCanvasMaterial();
            _defaultMaterial.shader = m_defaultMaterial.shader;
            _defaultMaterial.CopyPropertiesFromMaterial(m_defaultMaterial);
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
