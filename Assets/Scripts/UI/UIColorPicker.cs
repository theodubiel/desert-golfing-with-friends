using UnityEngine;
using UnityEngine.UI;

public class UIColorPicker : MonoBehaviour
{
    [SerializeField]
    private Slider redChannel;

    [SerializeField]
    private Slider greenChannel;

    [SerializeField]
    private Slider blueChannel;

    [SerializeField]
    private Image sampleColor;

    public Color selectedColor = Color.white;

    public float[] asFloatArray {
        get {
            return new float[] { selectedColor.r, selectedColor.g, selectedColor.b };
        }
    }

    void Start()
    {
        redChannel.onValueChanged.AddListener (delegate { UpdateColor(); });
        greenChannel.onValueChanged.AddListener (delegate { UpdateColor(); });
        blueChannel.onValueChanged.AddListener (delegate { UpdateColor(); });
    }

    public void SetColor(Color color) {
        redChannel.value = color.r;
        greenChannel.value = color.g;
        blueChannel.value = color.b;
        sampleColor.color = color;
        selectedColor = color;
    }

    public void SetColorWithFloats(float[] color) {
        SetColor(new Color(color[0], color[1], color[2]));
    }

    void UpdateColor() {
        selectedColor.r = redChannel.value;
        selectedColor.g = greenChannel.value;
        selectedColor.b = blueChannel.value;
        sampleColor.color = selectedColor;
    }
}
