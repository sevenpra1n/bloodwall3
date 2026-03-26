using UnityEngine;
using UnityEngine.UI;

public class BrightnessController : MonoBehaviour
{
    [SerializeField] private Image brightnessOverlay;

    private void Start()
    {
        if (brightnessOverlay == null)
        {
            Debug.LogError("Brightness Overlay не привязан!");
            return;
        }

        // Загружаем сохранённое значение
        float savedBrightness = PlayerPrefs.GetFloat("Brightness", 50f);
        SetBrightness(savedBrightness);
    }

    public void SetBrightness(float brightnessValue)
    {
        if (brightnessOverlay == null)
            return;

        // brightnessValue: 0-100
        // 50 = нормально (прозрачный)
        // 0 = полная чернота
        // 100 = полная белизна

        Color overlayColor;

        if (brightnessValue < 50f)
        {
            // Затемнение: 0-50 → чёрный с альфой 0-1
            float alpha = (50f - brightnessValue) / 50f;
            overlayColor = new Color(0, 0, 0, alpha);
        }
        else if (brightnessValue > 50f)
        {
            // Осветление: 50-100 → белый с альфой 0-1
            float alpha = (brightnessValue - 50f) / 50f;
            overlayColor = new Color(1, 1, 1, alpha);
        }
        else
        {
            // Нормально
            overlayColor = new Color(0, 0, 0, 0);
        }

        brightnessOverlay.color = overlayColor;
    }
}