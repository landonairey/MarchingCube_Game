using UnityEngine;
using TMPro;

public class DebugDisplay : MonoBehaviour
{
    public TextMeshProUGUI FpsText;
    public CharacterController player;
    public TextMeshProUGUI wXYZ;
    public TextMeshProUGUI vXYZ;
    public TextMeshProUGUI volume;
    public TextMeshProUGUI volume0;
    public TextMeshProUGUI volume1;
    public TextMeshProUGUI volume2;
    public TextMeshProUGUI volume3;
    public TextMeshProUGUI volume4;

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

        float[] vols = player.GetComponent<PlayerMovement>().volumes;
        volume0.text = string.Format("Gold Vol: {0:0.000}", vols[0]); //Use format for better performance
        volume1.text = string.Format("Silv Vol: {0:0.000}", vols[1]); //Use format for better performance
        volume2.text = string.Format("Copp Vol: {0:0.000}", vols[2]); //Use format for better performance
        volume3.text = string.Format("Dirt Vol: {0:0.000}", vols[3]); //Use format for better performance
        volume4.text = string.Format("Gras Vol: {0:0.000}", vols[4]); //Use format for better performance

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
