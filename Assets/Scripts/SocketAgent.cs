using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;
using static Constants;
using static Helpers;
using static GameStates;

public class SocketAgent : MonoBehaviour
{
    public float SPEED = 1;
    bool socket_ready = false;
    TcpClient my_socket;
    NetworkStream the_stream;
    StreamWriter the_writer;
    StreamReader the_reader;
    public GameObject BALL;
    public GameObject GOAL;
    StepRequest step_request;
    Rigidbody r_ball;
    int no_data_counter = 0;
    bool started = false;
    DateTime start_time;

    void Start()
    {
        r_ball = BALL.gameObject.GetComponent<Rigidbody>();
        step_request = new StepRequest();
    }

    void FixedUpdate()
    {
        if (!socket_ready)
        {
            setup_socket();
            if (socket_ready)
                StartCoroutine(read());
            else
                return;
        }


        if (step_request.timed_out)
        {
            // reset_ball(gameObject, BALL);
        }

        var z = -step_request.action_agent * SPEED;
        var local_rotation = transform.eulerAngles;
        local_rotation.z = Mathf.Clamp(check_angle(local_rotation.z + z), LOWER_BOUND, UPPER_BOUND);
        transform.eulerAngles = local_rotation;
    }

    float[] get_observation()
    {
        var input_x = Input.GetAxis("Horizontal");
        var x_speed = 0f;
        if (input_x > 0)
            x_speed = SPEED;
        else if (input_x < 0)
            x_speed = -SPEED;

        var position = BALL.transform.position;
        var velocity = r_ball.velocity;
        var local_rotation = transform.eulerAngles;
        return new[]
        {
            position.z, -position.x,
            velocity.z, -velocity.x,
            check_angle(local_rotation.x), check_angle(local_rotation.z),
            x_speed, step_request.action_agent * SPEED
        };
    }

    IEnumerator read()
    {
        started = true;
        var step_response = new StepResponse();
        var reset_response = new ResetResponse();
        start_time = DateTime.Now;
        while (true)
        {
            var mill_diff = (DateTime.Now - start_time).TotalMilliseconds;
            if (mill_diff > MAX_NO_DATA)
            {
                print("SocketTimeout");
                start_time = DateTime.Now;
                freeze_game = true;
                close_socket();
                socket_ready = false;
                yield break;
            }


            if (!socket_ready)
            {
                yield return new WaitForSeconds(.2f);
                continue;
            }

            var read = read_socket();
            if (read == null)
            {
                // print("AGENT read == null");
                yield return new WaitForSeconds(.2f);
                continue;
            }

            var res = read.Split('|');
            switch (res[0])
            {
                case "reset":
                    start_time = DateTime.Now;
                    print("reset");
                    // reset_ball(gameObject, BALL);

                    reset_response.observation = get_observation();
                    reset_response.setting_up_duration = 0;
                    var res_json = JsonUtility.ToJson(reset_response);
                    write_socket(res_json);
                    break;
                case "step":
                    start_time = DateTime.Now;

                    print("step");
                    var ser_step = JsonUtility.FromJson<StepRequest>(res[1]);
                    // do the actions
                    step_request = ser_step;
                    freeze_game = step_request.timed_out;
                    if (!step_request.timed_out)
                        yield return new WaitForSeconds(.2f);

                    step_response.observation = get_observation();
                    step_response.distance_from_goal =
                        Vector3.Distance(GOAL.transform.localPosition, BALL.transform.localPosition);
                    step_response.done = is_done ? is_done : step_request.timed_out;
                    step_response.fps = 60;
                    step_response.duration_pause = 0;
                    try
                    {
                        write_socket(JsonUtility.ToJson(step_response));
                    }
                    catch (SocketException)
                    {
                        print("SocketException");
                        start_time = DateTime.Now;
                        freeze_game = true;
                        close_socket();
                        socket_ready = false;
                        yield break;
                    }


                    break;
                default:
                    // print("AGENT default");
                    yield return new WaitForSeconds(.005f);
                    break;
            }
        }
    }


    void setup_socket()
    {
        try
        {
            print("Socket Starting");
            my_socket = new TcpClient(HOST, PORT);
            the_stream = my_socket.GetStream();
            the_writer = new StreamWriter(the_stream);
            the_reader = new StreamReader(the_stream);
            print(my_socket.ReceiveBufferSize);

            socket_ready = true;
            print("Socket Connected");
        }
        catch (Exception e)
        {
            print("Socket error: " + e);
        }
    }

    void write_socket(string the_line)
    {
        print("write_socket");

        if (!socket_ready)
            return;

        string foo = the_line + "\n";
        print("write->" + foo);

        the_writer.Write(foo);
        the_writer.Flush();
    }

    string read_socket()
    {
        if (!socket_ready)
            return "";

        if (!the_stream.DataAvailable)
        {
            no_data_counter++;
            return "";
        }

        no_data_counter = 0;
        print("read with data");

        var res = the_reader.ReadLine();
        print("read->" + res);
        return res;
    }

    void close_socket()
    {
        try
        {
            if (!socket_ready)
                return;
            the_writer.Close();
            the_reader.Close();
            my_socket.Close();
            socket_ready = false;
        }
        catch (Exception ex)
        {
            print("Exception in 'close_socket'");
            print(ex.ToString());
        }
    }
}