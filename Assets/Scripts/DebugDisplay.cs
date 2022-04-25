using UnityEngine;
using TMPro;

public class DebugDisplay : MonoBehaviour
{
    public TextMeshProUGUI FpsText;
    public CharacterController player;
    public TextMeshProUGUI wXYZ;
    public TextMeshProUGUI vXYZ;
    public TextMeshProUGUI volume;

    [SerializeField] float vScale = 0.5f;

    private float pollingTime = 1f; //how frequently the FPS display will update
    private float time;
    private int frameCount; //How many frames have past inside our polling interval

    // Update is called once per frame
    void Update()
    {
        //I noticed that in my game, this doesn't work while the game is paused (the text doesn't update). For those that have pause functionality that sets Time.timeScale to 0, use Time.unscaledDeltaTime in the FPS script instead of Time.deltaTime
        time += Time.unscaledDeltaTime;

        frameCount++;

        float wX = player.transform.position.x;
        float wY = player.transform.position.y;
        float wZ = player.transform.position.z;
        //wXYZ.text = "wXYZ: " + wX.ToString("#.000");
        wXYZ.text = string.Format("wXYZ: {0:0.000}, {1:0.000}, {2:0.000}", wX, wY, wZ); //Use format for better performance

        int vX = Mathf.CeilToInt(player.transform.position.x * vScale);
        int vY = Mathf.CeilToInt(player.transform.position.y * vScale);
        int vZ = Mathf.CeilToInt(player.transform.position.z * vScale);
        vXYZ.text = string.Format("vXYZ: {0:0.}, {1:0.}, {2:0.}", vX, vY, vZ); //Use format for better performance

        float vol = player.GetComponent<PlayerMovement>().volume;
        volume.text = string.Format("Volume: {0:0.000}", vol); //Use format for better performance

        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            FpsText.text = frameRate.ToString() + " FPS";

            //time = 0;
            time -= pollingTime; //They prefer this beacuse some time could have elapsed
            frameCount = 0;
        }

        
    }
}
