using UnityEngine.UI;
using UnityEngine;

public class VersionDisplay : MonoBehaviour
{
    public Text m_Text;
    
    void Awake()
    {
        m_Text.text = $"Version: {Application.version}\n";
    }

    
}
