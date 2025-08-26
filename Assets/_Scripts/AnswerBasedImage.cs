using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnswerBasedRawImage : MonoBehaviour
{
    [Header("Main Image Settings")]
    public RawImage displayImage;
    public Texture[] imageTextures; // Should contain exactly 27 textures

    [Header("QR Code Settings")]
    public RawImage qrCodeImage;
    public Texture[] qrCodeTextures; // Should contain exactly 27 QR code textures
    public NavigationController navigationController;

    [Header("Idle Mode Settings")]
    public float idleTime = 30f; // Time in seconds before idle mode activates

    [Header("Current Answers")]
    public int[] answers = new int[3]; // Stores answers for 3 questions (0-2 each)

    private float lastInteractionTime;
    private Coroutine idleCoroutine;

    void Start()
    {
        // Validate array sizes
        if (imageTextures != null && imageTextures.Length != 27)
        {
            Debug.LogError("Main texture array must contain exactly 27 textures!");
        }

        if (qrCodeTextures != null && qrCodeTextures.Length != 27)
        {
            Debug.LogError("QR code texture array must contain exactly 27 textures!");
        }

        // Initialize with default values
        if (answers.Length != 3)
        {
            answers = new int[3];
        }

        // Record initial interaction time
        lastInteractionTime = Time.time;

        // Start idle mode check
        idleCoroutine = StartCoroutine(CheckForIdleMode());

        UpdateImages();
    }

    void OnDisable()
    {
        // Stop the idle coroutine when the script is disabled
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
        }
    }

    // Call this method from UI buttons to set answers
    public void SetAnswerForQuestion(int questionIndex, int answerValue)
    {
        if (questionIndex < 0 || questionIndex > 2)
        {
            Debug.LogError("Question index must be 0, 1, or 2!");
            return;
        }

        if (answerValue < 0 || answerValue > 2)
        {
            Debug.LogError("Answer value must be 0, 1, or 2!");
            return;
        }

        // Update interaction time
        lastInteractionTime = Time.time;

        answers[questionIndex] = answerValue;
        UpdateImages();
    }

    // Alternative methods that can be called with a single parameter (for Unity Events)
    public void SetAnswerForQuestion0(int answerValue) { SetAnswerForQuestion(0, answerValue); }
    public void SetAnswerForQuestion1(int answerValue) { SetAnswerForQuestion(1, answerValue); }
    public void SetAnswerForQuestion2(int answerValue) { SetAnswerForQuestion(2, answerValue); }

    // Calculate and update the displayed images
    private void UpdateImages()
    {
        int imageIndex = CalculateImageIndex();

        // Update main image
        if (displayImage != null && imageTextures != null &&
            imageIndex >= 0 && imageIndex < imageTextures.Length &&
            imageTextures[imageIndex] != null)
        {
            displayImage.texture = imageTextures[imageIndex];
        }

        // Update QR code image
        if (qrCodeImage != null && qrCodeTextures != null &&
            imageIndex >= 0 && imageIndex < qrCodeTextures.Length &&
            qrCodeTextures[imageIndex] != null)
        {
            qrCodeImage.texture = qrCodeTextures[imageIndex];
        }
    }

    private int CalculateImageIndex()
    {
        // Convert ternary answers to decimal index (0-26)
        return answers[0] * 9 + answers[1] * 3 + answers[2];
    }

    // Check for idle mode
    IEnumerator CheckForIdleMode()
    {
        while (true)
        {
            // Check if idle time has passed
            if (Time.time - lastInteractionTime > idleTime)
            {
                StartCoroutine(CallIdleMode());
                yield break; // Exit this coroutine
            }

            yield return new WaitForSeconds(1f); // Check every second
        }
    }

    IEnumerator CallIdleMode()
    {
        yield return new WaitForSeconds(5f);

        if (navigationController != null)
        {
            navigationController.RestartFlow();
        }
        else
        {
            Debug.LogError("NavigationController reference is not assigned!");
        }

        // Reset answers after calling idle mode
        ResetAnswers();
    }

    // Reset all answers to default (can be called from a UI button)
    public void ResetAnswers()
    {
        answers = new int[3];
        UpdateImages();

        // Restart idle mode check
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
        }
        lastInteractionTime = Time.time;
        idleCoroutine = StartCoroutine(CheckForIdleMode());
    }

    // For testing purposes - simulate answers
    public void TestAnswers(int q1, int q2, int q3)
    {
        // Update interaction time
        lastInteractionTime = Time.time;

        answers[0] = q1;
        answers[1] = q2;
        answers[2] = q3;
        UpdateImages();
    }

    // Public method to manually trigger idle mode
    public void TriggerIdleMode()
    {
        StartCoroutine(CallIdleMode());
    }
}