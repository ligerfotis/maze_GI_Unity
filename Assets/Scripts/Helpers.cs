using UnityEngine;
using UnityEngine.Networking;
using static Constants;
using static GameStates;

public static class Helpers
{
    public static float check_angle(float value)
    /*
     * value: maximum angle
     * check that the tray angle does not pass the maximum angle.
     */
    {
        var angle = value - 180;
        if (angle > 0)
            return angle - 180;
        return angle + 180;
    }


    public static bool is_request_success(UnityWebRequest request)
    /*
     * Returns true if request has been received successfully or false otherwise
     */
    {
        return request.result != UnityWebRequest.Result.ConnectionError &&
               (request.responseCode == 0 || request.responseCode == (long) System.Net.HttpStatusCode.OK);
    }
}