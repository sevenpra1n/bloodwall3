// Hide the arrow before and after launch
// Ensure it's hidden during ultimate animations
// Add rotation animation so the arrow tilts down as it flies

public class ArrowProjectile : MonoBehaviour {
    private bool isLaunched = false;

    void Start() {
        // Hide arrow before launch
        gameObject.SetActive(false);
    }

    public void Launch() {
        gameObject.SetActive(true);
        isLaunched = true;
        // Add launch logic here
    }

    void Update() {
        if (isLaunched) {
            // Rotate the arrow downwards as it flies
            transform.Rotate(Vector3.right, Time.deltaTime * rotationSpeed);
            // Additional flying logic here
        }
    }

    public void UltimateAnimation() {
        // Hide the arrow during ultimate animations
        gameObject.SetActive(false);
    }
}